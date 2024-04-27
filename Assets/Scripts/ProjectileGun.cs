using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileGun : NetworkBehaviour
{
	[SerializeField] private float rpm = 100;
	[SerializeField] private int maxBullets;
	
	public GameObject projectile;
	public float shotForce = 50f;
	
	private int loadedBullets;
	private Transform barrel;
	private bool isNextRoundReady = true;
	private Coroutine gunCoroutine;
	
	// -----------------------------------
	// Only runs client side
	// -----------------------------------
	
	
	// -----------------------------------
	// Only runs server side
	// -----------------------------------
	
	public void Fire(Vector3 shotOrigin, Vector3 direction)
	{
		if(!isNextRoundReady) return;
		RaycastHit hit;
		Physics.Raycast(shotOrigin, direction, out hit, Mathf.Infinity, ~( (1 << gameObject.layer) + (1 << 9) ));
		Vector3 hitPos = hit.collider != null ? hit.point : shotOrigin + direction * 1000f;
		if(hit.collider != null)
		{
			PlayerBody bodyHit = hit.collider.GetComponent<PlayerBody>();
			if(bodyHit != null) bodyHit.player.Damage(direction, 50f, 2f);
		}
		
		StartCoroutine(CycleNextRoundCoroutine());
	}
	
	void Awake()
	{
		loadedBullets = maxBullets;
		barrel = transform.GetChild(0).GetChild(0);
		gunCoroutine = StartCoroutine(CycleNextRoundCoroutine());
		
	}
	
	private IEnumerator CycleNextRoundCoroutine()
	{
		isNextRoundReady = false;
		yield return new WaitForSeconds(60/rpm);
		isNextRoundReady = true;
	}
	
	// -----------------------------------
	// RPCs the server calls
	// -----------------------------------
	
}
