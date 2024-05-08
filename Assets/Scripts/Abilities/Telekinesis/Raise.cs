using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raise : Ability
{
	public override bool CanCast(PlayerController caster, PlayerController target, Transform origin, Vector3 direction)
    {
        return target != null;
    }
	protected override void Cast(PlayerController caster, PlayerController target, Transform origin, Vector3 direction)
	{
		target.AddForce_ServerRPC(Vector3.up, 15);
	}
}
