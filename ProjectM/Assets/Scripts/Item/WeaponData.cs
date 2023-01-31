using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 22.07.21_jayjeong
/// <br> Weapon 클래스를 상속한 모든 객체 데이터 </br>
/// </summary>
[System.Serializable]
public class WeaponData 
{
    [Header("Weapon 정보")]
    public int id = 0;
    public float equipSpeedPenalty = 0f;
    public float atkSpeedPenalty = 0f;
    public float loadSpeedPenalty = 0f;
    public float atkLatency = 0f;
    public bool canAuto = false;
    public float damageToPlayer = 0f;
    public float damageToObject = 0f;
    public float attackRange = 0f;
    public float fallOff = 0f;

    [Header("WeaponGun 정보")]
    public BulletType bulletType = BulletType.NULL;
    public int maxBullet = 0;
    public float loadedTime = 0f;
    public int costBullet = 0;
    public bool isBomb = false;

    [Header("WeaponThrow 정보")]
    public ThrowType throwType = ThrowType.NULL;

    public void DeepCopy(WeaponData weaponData)
    {
        id = weaponData.id;
        equipSpeedPenalty = weaponData.equipSpeedPenalty;
        atkSpeedPenalty = weaponData.atkSpeedPenalty;
        loadSpeedPenalty = weaponData.loadSpeedPenalty;
        atkLatency = weaponData.atkLatency;
        canAuto = weaponData.canAuto;
        damageToPlayer = weaponData.damageToPlayer;
        damageToObject = weaponData.damageToObject;
        attackRange = weaponData.attackRange;
        fallOff = weaponData.fallOff;

        bulletType = weaponData.bulletType;
        maxBullet = weaponData.maxBullet;
        loadedTime = weaponData.loadedTime;
        costBullet = weaponData.costBullet;
        isBomb = weaponData.isBomb;

        throwType = weaponData.throwType;
    }
}
