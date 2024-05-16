// using System.Collections;
// using System.Collections.Generic;
// using Unity.Netcode;
// using UnityEditor;
// using UnityEngine;

// public class Bomb : NetworkBehaviour
// {
// 	private Rigidbody rb;
	
// 	void Awake()
// 	{
// 		rb =  GetComponent<Rigidbody>();
// 	}
	
// 	// -----------------------------------
// 	// Only runs client side
// 	// -----------------------------------
	
// 	// -----------------------------------
// 	// Only runs server side
// 	// -----------------------------------
	
// 	void Start()
// 	{
// 	}
	
// 	public void BlowUp(PlayerController shooter, float damage, float force)
// 	{
// 		PlayerController closestEnemy = GetClosestVisibleEnemy(shooter);
// 		if(closestEnemy != null && rb.velocity.magnitude > 0.1f)
// 		{
// 			ReflectFlash_ClientRPC(closestEnemy.transform.GetChild(0).position, rpm);
// 			closestEnemy.Damage(closestEnemy.transform.position - transform.position, damage*2, force*2);
// 		}
// 	}
	
// 	private IEnumerator BlowUpCoroutine()
// 	{
// 		transform.GetChild(0).gameObject
// 	}
	
// 	// -----------------------------------
// 	// RPCs the server calls
// 	// -----------------------------------
// 	[ClientRpc]
// 	public void ReflectFlash_ClientRPC(Vector3 hitPos, float rpm)
// 	{
// 		StartCoroutine(ReflectFlashCoroutine(hitPos, rpm));
// 	}
// }
