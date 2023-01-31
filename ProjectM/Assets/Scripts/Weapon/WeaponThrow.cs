using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponThrow : Weapon
{
    public ThrowType throwType;
    public override void ParseData()
    {
        base.ParseData();
        throwType = weaponData.throwType;
    }

    public override void Attack(Vector2 direction, Vector2 position)
    {

    }
}
