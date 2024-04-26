using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Gun : NetworkBehaviour
{
	[SerializeField] private float rpm = 300;
	[SerializeField] private int maxBullets;
	private int loadedBullets;
	private Transform barrel;
	private bool isNextBulletReady = true;
	private Coroutine gunCoroutine;
	private LineRenderer barrelLR;
	
	// -----------------------------------
	// Only runs client side
	// -----------------------------------
	
	private IEnumerator MuzzleFlashCoroutine(Vector3 hitPos)
	{
		barrelLR.enabled = true;
		barrelLR.SetPosition(0, barrel.transform.position);
		barrelLR.SetPosition(1, hitPos);
		StartCoroutine(CycleNextBulletCoroutine());
		yield return new WaitForSeconds(30/rpm);
		barrelLR.enabled = false;
		yield return new WaitForSeconds(30/rpm);
	}
	
	// -----------------------------------
	// Only runs server side
	// -----------------------------------
	
	public void Fire(Vector3 shotOrigin, Vector3 direction)
	{
		if(!isNextBulletReady) return;
		RaycastHit hit;
		Physics.Raycast(shotOrigin, direction, out hit, Mathf.Infinity, ~( (1 << gameObject.layer) + (1 << 9) ));
		Vector3 hitPos = hit.collider != null ? hit.point : shotOrigin + direction * 10f;
		if(hit.collider != null)
		{
			PlayerBody bodyHit = hit.collider.GetComponent<PlayerBody>();
			if(bodyHit != null) bodyHit.player.Damage(direction, 50f, 2f);
		}
			
		MuzzleFlash_ClientRPC(hitPos);
	}
	
	void Awake()
	{
		loadedBullets = maxBullets;
		barrel = transform.GetChild(0).GetChild(0);
		barrelLR = barrel.GetComponent<LineRenderer>();
		gunCoroutine = StartCoroutine(CycleNextBulletCoroutine());
		
	}
	
	private IEnumerator CycleNextBulletCoroutine()
	{
		isNextBulletReady = false;
		yield return new WaitForSeconds(60/rpm);
		isNextBulletReady = true;
	}
	
	// -----------------------------------
	// RPCs the server calls
	// -----------------------------------
	
	[ClientRpc]
	void MuzzleFlash_ClientRPC(Vector3 hitPos)
	{
		StopCoroutine(gunCoroutine);
		gunCoroutine = StartCoroutine(MuzzleFlashCoroutine(hitPos));
	}
	
}
