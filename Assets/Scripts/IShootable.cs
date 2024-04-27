using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShootable
{
    public abstract void Shoot(CharacterController shooter, Vector3 shotOrigin, Vector3 direction);
}
