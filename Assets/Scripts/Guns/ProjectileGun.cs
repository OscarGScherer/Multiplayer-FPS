using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileGun : Gun
{
	public GameObject projectilePrefab;
	public float shotForce = 50f;
	
	
	// -----------------------------------
	// Only runs client side
	// -----------------------------------
	
	
	// -----------------------------------
	// Only runs server side
	// -----------------------------------
	
	public override void Fire(PlayerController shooter, Vector3 shotOrigin, Vector3 direction)
	{
		if(!isNextRoundReady) return;
		
		Rigidbody projectile = GameObject.Instantiate(projectilePrefab, shotOrigin, shooter.transform.rotation).GetComponent<Rigidbody>();
		projectile.GetComponent<NetworkObject>().Spawn();
		projectile.isKinematic = false;
		projectile.velocity = shooter.rb.velocity;
		projectile.AddForce(shotForce*direction, ForceMode.Impulse);
		
		StartCoroutine(CycleNextRoundCoroutine());
	}
	
	// -----------------------------------
	// RPCs the server calls
	// -----------------------------------
	
}
