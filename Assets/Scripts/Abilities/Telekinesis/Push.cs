using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Push : Ability
{
	public override bool CanCast(PlayerController caster, PlayerController target, Transform origin, Vector3 direction)
    {
        return target != null;
    }
	protected override void Cast(PlayerController caster, PlayerController target, Transform origin, Vector3 direction)
	{
		target.AddForce((target.transform.position - caster.transform.position).normalized, 15);
	}
}
