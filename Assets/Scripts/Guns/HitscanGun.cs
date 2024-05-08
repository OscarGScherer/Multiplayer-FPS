using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HitscanGun : Gun
{
	private bool isNextBulletReady = true;
	private LineRenderer lr;
	
	protected override void Awake()
	{
		base.Awake();
		lr = barrel.GetComponent<LineRenderer>();
	}
	
	// -----------------------------------
	// Only runs client side
	// -----------------------------------
	
	private IEnumerator MuzzleFlashCoroutine(Vector3 hitPos)
	{
		lr.enabled = true;
		lr.SetPosition(0, barrel.transform.position);
		lr.SetPosition(1, hitPos);
		yield return new WaitForSeconds(30/rpm);
		lr.enabled = false;
		yield return new WaitForSeconds(30/rpm);
	}
	
	// -----------------------------------
	// Only runs server side
	// -----------------------------------
	
	public override void Fire(PlayerController shooter, Vector3 shotOrigin, Vector3 direction)
	{
		if(!isNextBulletReady) return;
		RaycastHit hit;
		Physics.Raycast(shotOrigin, direction, out hit, Mathf.Infinity, ~( (1 << gameObject.layer) + (1 << 9) ));
		Vector3 hitPos = hit.collider != null ? hit.point : shotOrigin + direction * 1000f;
		if(hit.collider != null)
		{
			PlayerBody bodyHit = hit.collider.GetComponent<PlayerBody>();
			if(bodyHit != null) bodyHit.player.Damage(direction, 50f, 2f);
		}
		
		StartCoroutine(CycleNextRoundCoroutine());
		MuzzleFlash_ClientRPC(hitPos);
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
