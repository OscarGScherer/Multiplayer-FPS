using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Gun : NetworkBehaviour
{
	public float rpm = 300;
	public int roundCapacity = 5;
	protected int loadedRounds;
	protected Transform barrel;
	protected bool isNextRoundReady = true;
	
	protected virtual void Awake()
	{
		loadedRounds = roundCapacity;
		barrel = transform.GetChild(0).GetChild(0);
		StartCoroutine(CycleNextRoundCoroutine());
	}
	
	// -----------------------------------
	// Only runs client side
	// -----------------------------------
	
	// -----------------------------------
	// Only runs server side
	// -----------------------------------
	
	public virtual void Fire(PlayerController shooter, Vector3 shotOrigin, Vector3 direction)
	{
	}
	
	protected IEnumerator CycleNextRoundCoroutine()
	{
		isNextRoundReady = false;
		yield return new WaitForSeconds(60/rpm);
		isNextRoundReady = true;
	}
	
	// -----------------------------------
	// RPCs the server calls
	// -----------------------------------
	
}
