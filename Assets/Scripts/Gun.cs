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
	
	[ClientRpc]
	void MuzzleFlash_ClientRPC(Vector3 hitPos)
	{
		StopCoroutine(gunCoroutine);
		gunCoroutine = StartCoroutine(MuzzleFlashCoroutine(hitPos));
	}
	
	public void Fire(Vector3 shotOrigin, Vector3 direction)
	{
		if(!isNextBulletReady) return;
		RaycastHit hit;
		Physics.Raycast(shotOrigin, direction, out hit, 10f, ~(1 << gameObject.layer));
		Vector3 hitPos = hit.collider != null ? hit.point : shotOrigin + direction * 10f;
		if(hit.collider != null)
		{
			Debug.Log(hit.collider.name);
			if(hit.collider.GetComponent<PlayerController>() != null)
				hit.collider.GetComponent<PlayerController>().Hit_ClientRPC(direction);
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
	
	private IEnumerator CycleNextBulletCoroutine()
	{
		isNextBulletReady = false;
		yield return new WaitForSeconds(60/rpm);
		isNextBulletReady = true;
	}
}
