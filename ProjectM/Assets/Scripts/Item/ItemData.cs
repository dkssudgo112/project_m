using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 22.07.21_jayjeong
/// <br> Weapon ���� ��� ������ ������ </br>
/// </summary>
[System.Serializable]
public class ItemData
{
    [Header("ItemAmmo ���� ������")]
    public BulletType bulletType = BulletType.NULL;
    public int bulletCount = 0;

    [Header("ItemRecovery ���� ������")]
    public RecoverType recoverType = RecoverType.NULL;
    public float recoverValue = 0f;
    public float usingLatency = 0f;

    [Header("ItemDefensive ���� ������")]
    public DefensiveType defensiveType = DefensiveType.NULL;
    public float defensive = 0f;
    public int level = 0;

    [Header("ItemEyesight ���� ������")]
    public int lensPower = 0;
}
