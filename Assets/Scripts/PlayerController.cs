using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
	public const float MAX_HEALTH = 200;
	public const float MAX_SPEED = 10;
	
	// Network variables
	public NetworkVariable<Vector3> facingDirection = new NetworkVariable<Vector3>(Vector3.one, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	public NetworkVariable<float> health = new NetworkVariable<float>(MAX_HEALTH, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	public NetworkVariable<int> team = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	
	// Othe references
	public Transform spawnPoint;
	public bool canMove = true;
	public float lookSensitivity = 1f;
	private Rigidbody rb;
	private Transform lookTransform;
	private Gun equippedGun;
	private Transform footCheck;
	private bool canJump = true;
	
	private Coroutine respawnCoroutine;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		if(!IsOwner) rb.isKinematic = true;
		lookTransform = transform.GetChild(0).GetChild(0).GetChild(0);
		equippedGun = GetComponentInChildren<Gun>();
		footCheck = transform.GetChild(2);
	}

	// -----------------------------------
	// Only runs client side
	// -----------------------------------
	public void EnableCamera()
	{
		lookTransform.GetComponent<Camera>().enabled = true;
		lookTransform.GetComponent<AudioListener>().enabled = true;
	}
	
	public void Teleport(Vector3 to)
	{
		rb.isKinematic = true;
		rb.position = to;
		rb.isKinematic = false;
	}
	
	public bool IsTouchingGround()
	{
		return Physics.OverlapSphere(footCheck.position, 0.3f, ~(1 << gameObject.layer)).Length > 0;
	}
	
	public void Jump(float magnitude)
	{
		if(IsTouchingGround() && canJump)
		{
			StartCoroutine(JumpCooldown());
			AddForce(Vector3.up, magnitude);
		}
	}
	
	private IEnumerator JumpCooldown()
	{
		canJump = false;
		yield return new WaitForSeconds(0.5f);
		canJump = true;
	}
	
	private void AddForce(Vector3 force, float magnitude)
	{
		rb.AddForce(force.normalized * magnitude, ForceMode.Impulse);
	}
	
	public void LookAt(Vector2 mouseRotation)
	{
		float yRot = transform.eulerAngles.y + mouseRotation.x * lookSensitivity;
		transform.eulerAngles = new Vector3(0, yRot, 0);
		
		float xRot = lookTransform.localEulerAngles.x - mouseRotation.y * lookSensitivity;
		lookTransform.localEulerAngles = new Vector3(xRot, 0, 0);
		
		facingDirection.Value = lookTransform.forward;
		
		equippedGun.transform.localEulerAngles = new Vector3(xRot, 0, 0);
	}
	
	public void AddHorizontalForce(Vector2 direction, float magnitude)
	{
		if(rb.velocity.magnitude > MAX_SPEED) return;
		Vector3 force = (transform.forward * direction.y + transform.right * direction.x) * magnitude * Time.deltaTime;
		rb.AddForce(force);
	}
	
	// -----------------------------------
	// RPCs the client calls
	// -----------------------------------
	
	[ServerRpc]
	public void FireGun_ServerRPC() { equippedGun.Fire(lookTransform.position, facingDirection.Value); }
	
	// -----------------------------------
	// Only runs server side
	// -----------------------------------
	
	public void Damage(Vector3 direction, float damage, float force)
	{
		if(health.Value <= 0) return;
		health.Value -= damage;
		if(health.Value <= 0) Die();
		Damage_ClientRPC(direction, force);
		health.Value = health.Value < 0 ? 0 : health.Value > MAX_HEALTH ? MAX_HEALTH : health.Value;
	}
	
	private IEnumerator RespawnCoroutine()
	{
		yield return new WaitForSeconds(5f);
		health.Value = MAX_HEALTH;
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
		canMove = false;
	}
	
	[ClientRpc]
	private void Damage_ClientRPC(Vector3 direction, float force)
	{
		rb.AddForce(direction * force, ForceMode.Impulse);
	}
	
}
