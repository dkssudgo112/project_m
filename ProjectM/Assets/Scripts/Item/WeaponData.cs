using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 22.07.21_jayjeong
/// <br> Weapon Ŭ������ ����� ��� ��ü ������ </br>
/// </summary>
[System.Serializable]
public class WeaponData 
{
    [Header("Weapon ����")]
    public float equipSpeedPenalty = 0f;
    public float atkSpeedPenalty = 0f;
    public float loadSpeedPenalty = 0f;
    public float atkLatency = 0f;
    public bool canAuto = false;
    public float damageToPlayer = 0f;
    public float damageToObject = 0f;
    public float attackRange = 0f;
    public float fallOff = 0f;

    [Header("WeaponGun ����")]
    public BulletType bulletType = BulletType.NULL;
    public int maxBullet = 0;
    public float loadedTime = 0f;
    public int costBullet = 0;
    public bool isBomb = false;

    [Header("WeaponThrow ����")]
    public ThrowType throwType = ThrowType.NULL;
}
