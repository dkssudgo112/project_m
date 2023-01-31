using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 22.07.21_jayjeong
/// <br> Item 클래스를 상속한 모든 객체 데이터 </br>
/// </summary>
[System.Serializable]
public class ItemData
{
    [Header("Item 공통 정보")]
    public int id = 0;
    public int viewID = 0;
    public string itemName = "";
    public ItemType itemType = ItemType.NULL;
    public string spritePath = "";
    public string toolTip = "";
    public int poolSize = 0;
    public int itemCount = 0;

    [Header("ItemAmmo 관련 데이터")]
    public BulletType bulletType = BulletType.NULL;

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

    public void DeepCopy(ItemData itemData)
    {
        id = itemData.id;
        viewID = itemData.viewID;
        itemName = itemData.itemName;
        itemType = itemData.itemType;
        spritePath = itemData.spritePath;
        toolTip = itemData.toolTip;
        poolSize = itemData.poolSize;
        itemCount = itemData.itemCount;

        bulletType = itemData.bulletType;

        recoverType = itemData.recoverType;
        recoverValue = itemData.recoverValue;
        usingLatency = itemData.usingLatency;

        defensiveType = itemData.defensiveType;
        defensive = itemData.defensive;
        level = itemData.level;

        lensPower = itemData.lensPower;
    }
}
