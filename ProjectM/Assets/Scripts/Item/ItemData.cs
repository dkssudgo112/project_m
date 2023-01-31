using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 22.07.21_jayjeong
/// <br> Weapon 제외 모든 아이템 데이터 </br>
/// </summary>
[System.Serializable]
public class ItemData
{
    [Header("ItemAmmo 관련 데이터")]
    public BulletType bulletType = BulletType.NULL;
    public int bulletCount = 0;

    [Header("ItemRecovery 관련 데이터")]
    public RecoverType recoverType = RecoverType.NULL;
    public float recoverValue = 0f;
    public float usingLatency = 0f;

    [Header("ItemDefensive 관련 데이터")]
    public DefensiveType defensiveType = DefensiveType.NULL;
    public float defensive = 0f;
    public int level = 0;

    [Header("ItemEyesight 관련 데이터")]
    public int lensPower = 0;
}
