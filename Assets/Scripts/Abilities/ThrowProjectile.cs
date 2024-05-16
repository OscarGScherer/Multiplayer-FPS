using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ThrowProjectile : Ability
{
	public GameObject projectilePrefab;
	public float throwForce = 5f;
	public float throwVerticalForce = 1f;
	public Vector3 angularVelocity;
	private GameObject previous;
	
	public override bool CanCast(PlayerController caster, PlayerController target, Transform origin, Vector3 direction)
	{
		return true;
	}

	protected override void Cast(PlayerController caster, PlayerController target, Transform origin, Vector3 direction)
	{
		Throw_ServerRPC(caster.rb.velocity, caster.transform.rotation, origin.position, direction, caster.OwnerClientId);
	}

	// -----------------------------------
	// Only runs server side
	// -----------------------------------
	[ServerRpc]
	protected void Throw_ServerRPC(Vector3 initialVelocity, Quaternion shotRotataion, Vector3 shotOrigin, Vector3 direction, ulong ownerId)
	{	
		Rigidbody projectile = GameObject.Instantiate(projectilePrefab, shotOrigin, shotRotataion).GetComponent<Rigidbody>();
		
		if(previous != null) Destroy(previous);
		previous = projectile.gameObject;
		
		projectile.GetComponent<NetworkObject>().SpawnWithOwnership(ownerId);
		projectile.GetComponent<Throwable>().SetPos_ClientRpc(shotOrigin);
		projectile.GetComponent<Throwable>().AddForce_ClientRPC(throwForce*direction + Vector3.up * throwVerticalForce, initialVelocity, angularVelocity);
	}
	
	public override void OnDestroy()
	{
		if(IsServer && previous != null) Destroy(previous);
	}
}
