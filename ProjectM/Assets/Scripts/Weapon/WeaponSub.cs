using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSub : Weapon
{
    float atkSizeX, atkSizeY;

    public override void ParseData()
    {
        base.ParseData();
    }

    public override void Attack(Vector2 direction, Vector2 position)
    {
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(direction + position, new Vector2(atkSizeX, atkSizeY), 0);
        IDamageable target = null;

        foreach (Collider2D collider in collider2Ds)
        {
            if(collider.tag == "Player")
            {
                if (collider.gameObject.TryGetComponent(out target))
                {
                    //target.TakeDamage(damageToPlayer, damageToObject, (Vector2)new Vector2(0, 0));
                }
            }
        }
    }
}
