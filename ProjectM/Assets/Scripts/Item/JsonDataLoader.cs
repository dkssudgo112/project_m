using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using LitJson;
using UnityEngine.Networking;

/// <summary>
/// 22.07.21_jayjeong
/// <br> Item 관련 Json 데이터 파싱 </br>
/// </summary>
[System.Serializable]
public class JsonDataLoader
{
    static string _streamingAssetsPath = Application.streamingAssetsPath;
    private string _itemDataPath = "/ItemData.json";
    private string _weaponDataPath = "/WeaponData.json";
    private int _commonDataCnt = 7;
    private int _itemTypeColumn = 2;

    // Weapon 제외 Item 클래스를 상속한 모든 객체 데이터 파싱 
    public void ParsingItemData()
    {
        string jsonString;
        string path = _streamingAssetsPath + _itemDataPath;

        jsonString = File.ReadAllText(path);
        JsonData jData = JsonMapper.ToObject(jsonString);

        for (int i = 0; i < jData.Count; i++)
        {
            var item = ItemManager.GetInstanceByType<Item>(GetItemType(jData[i][_itemTypeColumn].ToString()));

            if (item == null)
            {
                throw new Exception($"Item instance got by type is null");
            }

            ItemData itemData = new ItemData();
            ApplyJsonDataToCommonItemData(jData[i], item);
            ApplyJsonDataToItemData(jData[i], itemData);

            item.itemData = itemData;
            item.ParseData();
            item.gameObject.name = item.itemName;
            item.GetComponent<SpriteRenderer>().sprite = GetSpriteByPath(item.spritePath);
            ItemManager.GetItemDic().Add(item.itemName, item);
        }
    }

    // Weapon 클래스를 상속한 모든 객체 데이터 파싱 
    public void ParsingWeaponData()
    {
        string jsonString;
        string path = _streamingAssetsPath + _weaponDataPath;

        jsonString = File.ReadAllText(path);
        JsonData jData = JsonMapper.ToObject(jsonString);

        for (int i = 0; i < jData.Count; i++)
        {
            var weapon = ItemManager.GetInstanceByType<Weapon>(GetItemType(jData[i][_itemTypeColumn].ToString()));

            if (weapon == null)
            {
                throw new Exception($"Weapon instance got by type is null");
            }

            WeaponData weaponData = new WeaponData();
            ApplyJsonDataToCommonItemData(jData[i], weapon);
            ApplyJsonDataToWeaponData(jData[i], weaponData);

            weapon.weaponData = weaponData;
            weapon.ParseData();
            weapon.gameObject.name = weapon.itemName;
            weapon.GetComponent<SpriteRenderer>().sprite = GetSpriteByPath(weapon.spritePath);
            ItemManager.GetItemDic().Add(weapon.itemName, weapon);
        }
    }

    private void ApplyJsonDataToCommonItemData(JsonData jData, Item item)
    {
        string tempData = null;
        int index = 0;

        item.id = GetDataByType<int>(jData[index++].ToString());

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            item.itemName = tempData;
        }

        tempData = jData[index++].ToString();
        item.type = ItemType.NULL;
        if (string.IsNullOrEmpty(tempData) == false)
        {
            Enum.TryParse(tempData, out item.type);
        }

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            item.spritePath = tempData;
        }

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            item.toolTip = tempData;
        }

        item.stackSize = GetDataByType<int>(jData[index++].ToString());

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            item.itemCount = GetDataByType<int>(tempData.ToString());
        }
    }

    private void ApplyJsonDataToItemData(JsonData jData, ItemData itemData)
    {
        string tempData = null;
        int index = _commonDataCnt;

        // ======================= ItemAmmo 데이터 ======================= //

        tempData = jData[index++].ToString();
        itemData.bulletType = BulletType.NULL;
        if (string.IsNullOrEmpty(tempData) == false)
        {
            Enum.TryParse(tempData, out itemData.bulletType);
        }

        itemData.bulletCount = GetDataByType<int>(jData[index++].ToString());

        // ======================= ItemRecovery 데이터 ======================= //

        tempData = jData[index++].ToString();
        itemData.recoverType = RecoverType.NULL;
        if (string.IsNullOrEmpty(tempData) == false)
        {
            Enum.TryParse(tempData, out itemData.recoverType);
        }

        itemData.recoverValue = GetDataByType<float>(jData[index++].ToString());
        itemData.usingLatency = GetDataByType<float>(jData[index++].ToString());

        // ======================= ItemDefensive 데이터 ======================= //

        tempData = jData[index++].ToString();
        itemData.defensiveType = DefensiveType.NULL;
        if (string.IsNullOrEmpty(tempData) == false)
        {
            Enum.TryParse(tempData, out itemData.defensiveType);
        }

        itemData.defensive = GetDataByType<float>(jData[index++].ToString());
        itemData.level = GetDataByType<int>(jData[index++].ToString());

        // ======================= ItemScope 데이터 ======================= //

        itemData.lensPower = GetDataByType<int>(jData[index++].ToString());
    }

    private void ApplyJsonDataToWeaponData(JsonData jData, WeaponData weaponData)
    {
        string tempData = null;
        int index = _commonDataCnt;

        // ======================= Weapon 공통 데이터 ======================= //

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            weaponData.equipSpeedPenalty = GetDataByType<float>(tempData.ToString());
        }

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            weaponData.atkSpeedPenalty = GetDataByType<float>(tempData.ToString());
        }

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            weaponData.loadSpeedPenalty = GetDataByType<float>(tempData.ToString());
        }

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            weaponData.atkLatency = GetDataByType<float>(tempData.ToString());
        }

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            weaponData.canAuto = GetDataByType<bool>(tempData.ToString());
        }

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            weaponData.damageToPlayer = GetDataByType<float>(tempData.ToString());
        }

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            weaponData.damageToObject = GetDataByType<float>(tempData.ToString());
        }

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            weaponData.attackRange = GetDataByType<float>(tempData.ToString());
        }

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            weaponData.fallOff = GetDataByType<float>(tempData.ToString());
        }

        // ======================= WeaponGun 데이터 ======================= //

        tempData = jData[index++].ToString();
        weaponData.bulletType = BulletType.NULL;
        if (string.IsNullOrEmpty(tempData) == false)
        {
            Enum.TryParse(tempData, out weaponData.bulletType);
        }

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            weaponData.maxBullet = GetDataByType<int>(tempData.ToString());
        }

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            weaponData.loadedTime = GetDataByType<float>(tempData.ToString());
        }

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            weaponData.costBullet = GetDataByType<int>(tempData.ToString());
        }

        tempData = jData[index++].ToString();
        if (string.IsNullOrEmpty(tempData) == false)
        {
            weaponData.isBomb = GetDataByType<bool>(tempData.ToString());
        }

        //weaponData.maxBullet = GetDataByType<int>(jData[index++].ToString());
        //weaponData.loadedTime = GetDataByType<float>(jData[index++].ToString());
        //weaponData.costBullet = GetDataByType<int>(jData[index++].ToString());
        //weaponData.isBomb = GetDataByType<bool>(jData[index++].ToString());

        // ======================= WeaponThrow 데이터 ======================= //

        tempData = jData[index++].ToString();
        weaponData.throwType = ThrowType.NULL;
        if (string.IsNullOrEmpty(tempData) == false)
        {
            Enum.TryParse(tempData, out weaponData.throwType);
        }
    }

    private ItemType GetItemType(string data)
    {
        ItemType itemType = ItemType.NULL;

        Enum.TryParse(data, out itemType);

        return itemType;
    }

    private T GetDataByType<T>(string data)
    {
        if (string.IsNullOrEmpty(data) == true)
        {
            return default(T);
        }
        else
        {
            // Object 타입이 값 타입인 T인 경우 박싱/언박싱이 이루어지므로 대안을 생각해보자. 
            return (T)Convert.ChangeType(data, typeof(T));
        }
    }

    // 매개변수로 넘겨받은 path의 경로에 있는 Sprite 반환 
    public Sprite GetSpriteByPath(string path)
    {
        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogError($"Resource path of {path} is not valid");
        }

        return sprite;
    }
}
