using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ThrowProjectile : Ability
{
	public GameObject projectilePrefab;
	public float throwForce = 5f;
	public float throwVerticalForce = 1f;
	
	public override bool CanCast(PlayerController caster, PlayerController target, Transform origin, Vector3 direction)
	{
		return true;
	}

	protected override void Cast(PlayerController caster, PlayerController target, Transform origin, Vector3 direction)
	{
		Cast_ServerRPC(caster, origin.position, direction, caster.OwnerClientId);
	}
	
	// -----------------------------------
	// Only runs server side
	// -----------------------------------
	[ServerRpc]
	protected void Cast_ServerRPC(PlayerController shooter, Vector3 shotOrigin, Vector3 direction, ulong ownerId)
	{	
		Rigidbody projectile = GameObject.Instantiate(projectilePrefab, shotOrigin, shooter.transform.rotation).GetComponent<Rigidbody>();
		projectile.GetComponent<NetworkObject>().SpawnWithOwnership(ownerId);
		projectile.isKinematic = false;
		projectile.velocity = shooter.rb.velocity;
		projectile.AddForce(throwForce*direction + Vector3.up * throwVerticalForce, ForceMode.Impulse);
	}
}
