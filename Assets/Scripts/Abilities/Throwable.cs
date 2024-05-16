using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Throwable : NetworkBehaviour
{
	[ClientRpc]
	public void AddForce_ClientRPC(Vector3 force, Vector3 initialVelocity, Vector3 initialAngularVelocity)
	{
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.velocity = initialVelocity;
		rb.angularVelocity = initialAngularVelocity;
		
		rb.AddForce(force, ForceMode.Impulse);
	}
	
	[ClientRpc]
	public void SetPos_ClientRpc(Vector3 position)
	{
		if(!IsOwner) return;
		
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.position = position;
		rb.isKinematic = false;
	}
}
