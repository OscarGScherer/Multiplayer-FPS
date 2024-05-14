using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HealingHitscanGun : HitscanGun
{
	public float healing;
	// -----------------------------------
	// Only runs client side
	// -----------------------------------
	
	// -----------------------------------
	// Only runs server side
	// -----------------------------------
	
	public override void Fire(PlayerController shooter, Vector3 shotOrigin, Vector3 direction)
	{
		if(!isNextRoundReady) return;
		RaycastHit hit;
		Physics.Raycast(shotOrigin, direction, out hit, Mathf.Infinity, ~(1 << 9));
		Vector3 hitPos = hit.collider != null ? hit.point : shotOrigin + direction * 1000f;
		if(hit.collider != null)
		{
			PlayerBody bodyHit = hit.collider.GetComponent<PlayerBody>();
			if(bodyHit != null)
			{
				if(bodyHit.player.team == shooter.team)
					bodyHit.player.Damage(direction, -healing, 2f);
				else
					bodyHit.player.Damage(direction, damage, 2f);
			}
		}
		
		StartCoroutine(CycleNextRoundCoroutine());
		MuzzleFlash_ClientRPC(hitPos);
	}
	
	// -----------------------------------
	// RPCs the server calls
	// -----------------------------------
}
