using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
	public NetworkVariable<Vector3> facingDirection = new NetworkVariable<Vector3>(Vector3.one, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	public NetworkVariable<bool> isAlive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	
	public bool canMove = true;
	public float movementSpeed = 3f, lookSensitivity = 1f;
	private Rigidbody rb;
	private Transform lookTransform;
	private Gun equippedGun;
	private Transform footCheck;
	private bool canJump = true;

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		isAlive.OnValueChanged += OnIsAliveChanged;
	}
	
	private void OnIsAliveChanged(bool wasAlive, bool isAlive)
	{
		//if(wasAlive && !isAlive)
	}

	[ClientRpc]
	public void Hit_ClientRPC(Vector3 direction)
	{
		Debug.Log("Got hit");
	}
	
	[ServerRpc]
	public void Die_ServerRPC()
	{
		
	}
	
	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		lookTransform = transform.GetChild(0).GetChild(0).GetChild(0);
		equippedGun = GetComponentInChildren<Gun>();
		footCheck = transform.GetChild(2);
	
	}
	
	public void DisableCamera()
	{
		lookTransform.GetComponent<Camera>().enabled = false;
		lookTransform.GetComponent<AudioListener>().enabled = false;
	}
	
	public void FireGun() { equippedGun.Fire(lookTransform.position, facingDirection.Value); }
	
	public void SetHorizontalVelocity(Vector2 velocity)
	{
		Vector3 horizontalVelocity = (transform.forward * velocity.y + transform.right * velocity.x) * movementSpeed;
		rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
	}
	
	// void OnDrawGizmos()
	// {
	// 	Gizmos.DrawSphere(footCheck.position, 0.3f);
	// }
	
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
	
	
}
