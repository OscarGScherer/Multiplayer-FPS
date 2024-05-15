using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class ReflectCoin : NetworkBehaviour
{
	private LineRenderer lr;
	private Rigidbody rb;
	
	void Awake()
	{
		lr = GetComponent<LineRenderer>();
		rb =  GetComponent<Rigidbody>();
	}
	
	// -----------------------------------
	// Only runs client side
	// -----------------------------------
	protected IEnumerator ReflectFlashCoroutine(Vector3 hitPos, float rpm)
	{
		lr.enabled = true;
		lr.SetPosition(0, transform.position);
		lr.SetPosition(1, hitPos);
		yield return new WaitForSeconds(30/rpm);
		lr.enabled = false;
		yield return new WaitForSeconds(30/rpm);
	}
	// -----------------------------------
	// Only runs server side
	// -----------------------------------
	
	void Start()
	{
		if(IsServer) rb.angularVelocity = new Vector3(30,0,0);
	}
	
	// void FixedUpdate()
	// {
	// 	if(!IsServer) return;
	// 	Quaternion deltaRotation = Quaternion.Euler(new Vector3(360,0,0) * Time.fixedDeltaTime);
	// 	rb.MoveRotation(rb.rotation * deltaRotation);
	// }
	
	public void ReflectShot(PlayerController shooter, float damage, float rpm, float force)
	{
		PlayerController closestEnemy = GetClosestVisibleEnemy(shooter);
		if(closestEnemy != null && rb.velocity.magnitude > 0.1f)
		{
			ReflectFlash_ClientRPC(closestEnemy.transform.GetChild(0).position, rpm);
			closestEnemy.Damage(closestEnemy.transform.position - transform.position, damage*2, force*2);
		}
	}
	
	public PlayerController GetClosestVisibleEnemy(PlayerController shooter)
	{
		PlayerController closestReachableEnemy = null;
		float closestDistance = Mathf.Infinity;
		foreach(GameObject p in MatchInfo.playersCharacters)
		{
			if(p == null) continue;
			PlayerController otherPlayer = p.GetComponent<PlayerController>();
			if(otherPlayer == null || otherPlayer == shooter) continue;
			if(otherPlayer.team == shooter.team) continue;
			
			Vector3 dir = otherPlayer.transform.GetChild(0).position - transform.position;
			
			RaycastHit hit;
			Physics.Raycast(transform.position, dir, out hit, Mathf.Infinity, ~((1 << 10) + (1 << shooter.gameObject.layer) + (1 << 9)));
			
			if(hit.collider == null) continue;
			
			PlayerBody bodyPart = hit.collider.GetComponent<PlayerBody>();
			if(bodyPart != null && bodyPart.player == otherPlayer && hit.distance < closestDistance)
			{
				closestReachableEnemy = otherPlayer;
				closestDistance = hit.distance;
				//ReflectFlash_ClientRPC(closestReachableEnemy.transform.position + Vector3.up, 500);
			}
		}
		return closestReachableEnemy;
	}
	// -----------------------------------
	// RPCs the server calls
	// -----------------------------------
	[ClientRpc]
	public void ReflectFlash_ClientRPC(Vector3 hitPos, float rpm)
	{
		StartCoroutine(ReflectFlashCoroutine(hitPos, rpm));
	}
}
