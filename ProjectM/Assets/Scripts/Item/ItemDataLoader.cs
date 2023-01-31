using UnityEngine;
using System;
using System.IO;

/// <summary>
/// 22.07.21_jayjeong
/// <br> Item ���� Json ������ �Ľ��ϴ� Ŭ���� </br>
/// </summary>
public static class ItemDataLoader
{
    /// <summary>
    /// ItemData Json ������ �޾� ItemData ��ü�� ������ ItemDataDictionary�� ���� 
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
    /// WeaponData Json ������ �޾� WeaponData ��ü�� ������ WeaponDataDictionary�� ���� 
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

    // �Ű������� �Ѱܹ��� data�� T Ÿ������ ��ȯ�ؼ� ��ȯ 
    private static T GetDataByType<T>(string data)
    {
        if (string.IsNullOrEmpty(data) == true)
        {
            return default(T);
        }
        else
        {
            // Object Ÿ���� �� Ÿ���� T�� ��� �ڽ�/��ڽ��� �̷�����Ƿ� ����� �����غ���. 
            return (T)Convert.ChangeType(data, typeof(T));
        }
    }

    // �Ű������� �Ѱܹ��� path�� ��ο� �ִ� Sprite ��ȯ 
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
