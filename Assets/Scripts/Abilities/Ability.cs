using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
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
		yield return new WaitForSeconds(cooldown);
		onCooldown = false;
	}
}
