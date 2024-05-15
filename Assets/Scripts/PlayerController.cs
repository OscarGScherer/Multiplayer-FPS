using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;

public class PlayerController : NetworkBehaviour
{
	// Network variables
	public NetworkVariable<Vector3> facingDirection = new NetworkVariable<Vector3>(Vector3.one, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	public NetworkVariable<float> health = new NetworkVariable<float>(200, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	private Player _player;
	public Player player
	{
		get { return _player; }
		set
		{
			_player = value;
			_player.team.OnValueChanged += SetTeam;
			SetTeam(_player.team.Value, _player.team.Value);
		}
	}
	public Player.Team team => player.team.Value;
	
	// Other references
	
	// Player stats
	public float maxHealth = 200;
	public float maxMovementSpeed = 7, movementForce = 5;
	public float groundDrag = 7, airSpeedMult = 0.4f;
	public float jumpForce = 15;
	
	public float lookSensitivity = 1f;
	[HideInInspector] public Transform spawnPoint;
	[HideInInspector] public bool canMove = true;
	public Rigidbody rb;
	private Transform lookTransform;
	private Gun equippedGun;
	private bool canJump = true;
	private Coroutine respawnCoroutine;

	public override void OnDestroy()
	{
		player.team.OnValueChanged -= SetTeam;
		MatchInfo.playersCharacters.Remove(gameObject);
	}
	
	public override void OnNetworkSpawn()
	{
		MatchInfo.playersCharacters.Add(gameObject);
		foreach(Player client in MatchInfo.playerClients) 
		{
			if (client.OwnerClientId == OwnerClientId)
			{
				player = client;
				break;
			}
		}
	}
	
	void Awake()
	{	
		rb = GetComponent<Rigidbody>();
		if(!IsOwner) rb.isKinematic = true;
		lookTransform = transform.GetChild(0).GetChild(0).GetChild(0);
		equippedGun = GetComponentInChildren<Gun>();
		
	}
	
	void SetTeam(Player.Team previousTeam, Player.Team newTeam)
	{
		spawnPoint = GameObject.FindGameObjectWithTag("Team " + newTeam + " Spawn").transform;
		transform.GetChild(0).GetComponent<MeshRenderer>().material = Resources.Load<Material>("Team " + newTeam);
		var children = transform.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (var child in children)
		{
			if(child.gameObject.layer != 5) child.gameObject.layer = (int)newTeam + 5;
		}
	}

	// -----------------------------------
	// Only runs client side
	// -----------------------------------
	
	public void Update()
	{
		if(!IsOwner) return;
		
		if(health.Value > 0)
		{
			if(IsTouchingGround()) rb.drag = groundDrag;
			else rb.drag = 0;	
		}
		
		facingDirection.Value = lookTransform.forward;
	}
	
	public void EnableCamera()
	{
		lookTransform.GetComponent<Camera>().enabled = true;
		lookTransform.GetComponent<AudioListener>().enabled = true;
		
		lookTransform.GetChild(0).GetComponent<CanvasGroup>().alpha = 1.0f;
	}
	
	public void Teleport(Vector3 to)
	{
		rb.isKinematic = true;
		rb.position = to;
		rb.isKinematic = false;
	}
	
	public bool IsTouchingGround()
	{
		return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector2.down, 0.4f, ~( (1 << gameObject.layer) + (1 << 9)));
	}
	
	public void Jump(float magnitude)
	{
		if(IsTouchingGround() && canJump)
		{
			StartCoroutine(JumpCooldown());
			if(rb.velocity.y < 0) rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
			AddForce(Vector3.up, magnitude);
		}
	}
	
	private IEnumerator JumpCooldown()
	{
		canJump = false;
		yield return new WaitForSeconds(0.5f);
		canJump = true;
	}
	
	public void AddForce(Vector3 force, float magnitude)
	{
		rb.AddForce(force.normalized * magnitude, ForceMode.Impulse);
	}
	
	public void LookAt(Vector2 mouseRotation)
	{
		float yRot = transform.eulerAngles.y + mouseRotation.x * lookSensitivity;
		transform.eulerAngles = new Vector3(0, yRot, 0);
		
		float xRot = lookTransform.localEulerAngles.x - mouseRotation.y * lookSensitivity;
		lookTransform.localEulerAngles = new Vector3(xRot, 0, 0);
		
		equippedGun.transform.localEulerAngles = new Vector3(xRot, 0, 0);
	}
	
	public void AddForceWithSpeedCheck(Vector3 force)
	{
		Vector3 velocityInDirection = Vector3.Project(rb.velocity, force.normalized);
		
		if(velocityInDirection.magnitude > maxMovementSpeed) return;
		else
		{
			rb.AddForce(force, ForceMode.Force);
			velocityInDirection = Vector3.Project(rb.velocity, force.normalized);
			if(velocityInDirection.magnitude > maxMovementSpeed)
			{
				Vector3 perpForce1 = Quaternion.Euler(90,0,0) * force.normalized;
				Vector3 perpForce2 = Vector3.Cross(force, perpForce1);
				rb.velocity = 
					velocityInDirection.normalized * maxMovementSpeed
					+ Vector3.Project(rb.velocity, perpForce1) 
					+ Vector3.Project(rb.velocity, perpForce2);
			}
		}	
	}
	
	public void Move(Vector2 direction)
	{
		Vector3 force = (transform.forward * direction.y + transform.right * direction.x) * movementForce * Time.deltaTime;
		
		RaycastHit groundCheck;
		Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector2.down, out groundCheck, 0.4f, ~( (1 << gameObject.layer) + (1 << 9)));
		
		if(groundCheck.collider == null) force *= airSpeedMult;
		else force = Vector3.ProjectOnPlane(force, groundCheck.normal);
		
		AddForceWithSpeedCheck(force);
	}
	
	// -----------------------------------
	// RPCs the client calls
	// -----------------------------------
	
	[ServerRpc]
	public void FireGun_ServerRPC() { equippedGun.Fire(this, lookTransform.position, facingDirection.Value); }
	[ServerRpc]
	public void AddForce_ServerRPC(Vector3 force, float magnitude) => AddForce_ClientRPC(force, magnitude);
	
	// -----------------------------------
	// Only runs server side
	// -----------------------------------
	
	public void Damage(Vector3 direction, float damage, float force)
	{
		if(health.Value <= 0) return;
		health.Value -= damage;
		if(health.Value <= 0) Die();
		Damage_ClientRPC(direction, force);
		health.Value = health.Value < 0 ? 0 : health.Value > maxHealth ? maxHealth : health.Value;
	}
	
	private IEnumerator RespawnCoroutine()
	{
		yield return new WaitForSeconds(5f);
		health.Value = maxHealth;
		Respawn_ClientRPC(spawnPoint.position, spawnPoint.rotation);
		respawnCoroutine = null;
	}
	
	public void Die()
	{
		Die_ClientRPC();
		if(respawnCoroutine == null) respawnCoroutine = StartCoroutine(RespawnCoroutine());
	}
	
	// -----------------------------------
	// RPCs the server calls
	// -----------------------------------
	[ClientRpc] public void AddForce_ClientRPC(Vector3 force, float magnitude) => AddForce(force, magnitude);
	[ClientRpc] 
	public void Respawn_ClientRPC(Vector3 position, Quaternion rotation)
	{
		Teleport(position);
		transform.rotation = rotation;
		rb.freezeRotation = true;
		canMove = true;
	}
	
	[ClientRpc]
	private void Die_ClientRPC()
	{
		rb.freezeRotation = false;
		rb.angularVelocity = Vector3.forward;
		canMove = false;
	}
	
	[ClientRpc]
	private void Damage_ClientRPC(Vector3 direction, float force)
	{
		rb.AddForce(direction * force, ForceMode.Impulse);
	}
	
}
