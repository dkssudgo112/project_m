using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WeaponGun : Weapon
{
    [SerializeField]
    private GameObject bulletPrefab;
    private float weaponLength;

    public override void Attack(Vector2 direction, Vector2 position)
    {
    }
}

