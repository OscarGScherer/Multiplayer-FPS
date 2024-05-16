using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DPS1Gun : HitscanGun
{
	
	// -----------------------------------
	// Only runs client side
	// -----------------------------------
	
	public override void Fire(PlayerController shooter, Vector3 shotOrigin, Vector3 direction)
	{
		if(!isNextRoundReady) return;
		RaycastHit hit;
		Physics.Raycast(shotOrigin, direction, out hit, Mathf.Infinity, ~( (1 << gameObject.layer) + (1 << 9) ));
		Vector3 hitPos = hit.collider != null ? hit.point : shotOrigin + direction * 1000f;
		if(hit.collider != null)
		{
			PlayerBody bodyHit = hit.collider.GetComponent<PlayerBody>();
			if(bodyHit != null) bodyHit.player.Damage_ServerRPC(direction, damage, knockback);
			
			ReflectCoin coin = hit.collider.GetComponent<ReflectCoin>();
			if(coin != null && coin.OwnerClientId == OwnerClientId)
			{
				coin.ReflectShot(shooter, damage, rpm, knockback);
			}
		}
		
		StartCoroutine(CycleNextRoundCoroutine());
		MuzzleFlash_ServerRpc(hitPos);
	}
	
	// -----------------------------------
	// Only runs server side
	// -----------------------------------
	
	// -----------------------------------
	// RPCs the server calls
	// -----------------------------------
}
