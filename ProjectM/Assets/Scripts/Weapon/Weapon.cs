using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item
{
    public WeaponData weaponData;

    // 공격 속도
    [SerializeField]
    public float attackLatency;
    public bool canAuto;
    public float damageToPlayer;
    public float damageToObject;
    public float attackRange;
    public float fallOff;
    public bool isFire;
    public float equipSpeedPenalty;
    public float atkSpeedPenalty;
    public float loadSpeedPenalty;

    public override void ParseData()
    {
        base.ParseData();

        attackLatency = weaponData.atkLatency;
        canAuto = weaponData.canAuto;
        damageToPlayer = weaponData.damageToPlayer; 
        damageToObject = weaponData.damageToObject;
        attackRange = weaponData.attackRange;

        fallOff = weaponData.fallOff;
        equipSpeedPenalty = weaponData.equipSpeedPenalty;
        atkSpeedPenalty = weaponData.atkSpeedPenalty;
        loadSpeedPenalty = weaponData.loadSpeedPenalty;
    }

    public virtual void Attack(Vector2 direction, Vector2 position)
    {
    }
}
