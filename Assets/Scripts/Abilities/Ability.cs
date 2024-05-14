using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public abstract class Ability : NetworkBehaviour
{
	public Image ui;
	public float cooldown;
	[HideInInspector] public bool onCooldown = false;
	protected abstract void Cast(PlayerController caster, PlayerController target, Transform origin, Vector3 direction);
	public abstract bool CanCast(PlayerController caster, PlayerController target, Transform origin, Vector3 direction);
	public void TryCast(PlayerController caster, PlayerController target, Transform origin, Vector3 direction)
	{
		if(!onCooldown && CanCast(caster, target, origin, direction))	
		{
			Cast(caster, target, origin, direction);
			StartCoroutine(Cooldown());
		}
	}
	protected IEnumerator Cooldown()
	{
		onCooldown = true;
		float timer = 0;
		while(timer < cooldown)
		{
			ui.fillAmount = Mathf.Lerp(0,1, timer/cooldown);
			yield return null;
			timer+= Time.deltaTime;
		}
		onCooldown = false;
	}
}
