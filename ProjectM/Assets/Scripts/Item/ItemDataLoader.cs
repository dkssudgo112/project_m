using UnityEngine;
using System;
using System.IO;

/// <summary>
/// 22.07.21_jayjeong
/// <br> Item 관련 Json 데이터 파싱하는 클래스 </br>
/// </summary>
public static class ItemDataLoader
{
    /// <summary>
    /// ItemData Json 파일을 받아 ItemData 객체를 생성해 ItemDataDictionary에 저장 
    /// </summary>
    /// <param name="dataPath"></param>
    public static void ParseItemData(string dataPath)
    {
        string itemStringData = File.ReadAllText(dataPath);
        var itemDataArray = JsonHelper.FromJson<ItemData>(itemStringData);

        foreach (var itemData in itemDataArray)
        {
            ItemData data = new ItemData();

            data.DeepCopy(itemData);
            ItemManager.AddItemDataToDic(data.id, data);
        }
    }

    /// <summary>
    /// WeaponData Json 파일을 받아 WeaponData 객체를 생성해 WeaponDataDictionary에 저장 
    /// </summary>
    /// <param name="dataPath"></param>
    public static void ParseWeaponData(string dataPath)
    {
        string weaponStringData = File.ReadAllText(dataPath);
        var weaponDataArray = JsonHelper.FromJson<WeaponData>(weaponStringData);

        foreach (var weaponData in weaponDataArray)
        {
            WeaponData data = new WeaponData();

            data.DeepCopy(weaponData);
            ItemManager.AddWeaponDataToDic(data.id, data);
        }
    }

    // 매개변수로 넘겨받은 data를 T 타입으로 변환해서 반환 
    private static T GetDataByType<T>(string data)
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
    public static Sprite GetSpriteByPath(string path)
    {
        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogError($"Resource path of {path} is not valid");
        }

        return sprite;
    }
}
