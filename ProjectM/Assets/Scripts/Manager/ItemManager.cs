using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// 22.07.22_jayjeong
/// <br> 아이템 데이터 관리 및 데이터를 기반으로 아이템 객체를 만들어 관리하는 매니저 </br>
/// </summary>
public class ItemManager : MonoBehaviour
{
    private static ItemManager Instance = null;

    // ItemID를 Key로 가지는 ItemData 딕셔너리 
    private Dictionary<int, ItemData> _itemDataDictionary = new Dictionary<int, ItemData>();
    // ItemID를 Key로 가지는 WeaponData 딕셔너리 
    private Dictionary<int, WeaponData> _weaponDataDictionary = new Dictionary<int, WeaponData>();
    // ItemName을 Key, Item 객체를 Value로 가지는 아이템 딕셔너리 
    private Dictionary<string, Item> _itemDictionary = new Dictionary<string, Item>();

    // 모든 아이템의 기본틀이 되는 프리팹 
    public GameObject _itemPrefab;

    [SerializeField] 
    private string _itemDataPath = "/ItemData.json";
    [SerializeField] 
    private string _weaponDataPath = "/WeaponData.json";
   // Weapon 클래스를 상속받는 아이템의 첫 번째 ItemID 값  
    [SerializeField] 
    private int _weaponFirstID = 100;
    [SerializeField] 
    private float _itemSpawnPos = 500f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        ItemDataLoader.ParseItemData(Application.streamingAssetsPath + _itemDataPath);
        ItemDataLoader.ParseWeaponData(Application.streamingAssetsPath + _weaponDataPath);
        CreateItemsToDic();
        ObjectPoolManager.CreateItemPoolsOnStart();
    }

    /// <summary>
    /// 딕셔너리에 저장된 ItemData를 기반으로 아이템 인스턴스(프로토타입)를 만들어 itemDictionary에 보관 
    /// </summary>
    public static void CreateItemsToDic()
    {
        // 아이템 데이터를 기반으로 아이템 인스턴스를 만들어 itemDictionary에 보관 

        foreach(var data in Instance._itemDataDictionary.Values)
        {
            Item item = null;

            if (data.id < Instance._weaponFirstID)
            {
                item = GetItemInstanceByType<Item>(data.itemType);
            }
            else
            {
                item = GetItemInstanceByType<Weapon>(data.itemType);

                Weapon weapon = item as Weapon;
                WeaponData weaponData = new WeaponData();
                weaponData.DeepCopy(GetWeaponDataByID(data.id));
                weapon.weaponData = weaponData;
            }

            ItemData itemData = new ItemData();
            itemData.DeepCopy(GetItemDataByID(data.id));
            item.itemData = itemData;
            item.gameObject.name = itemData.itemName;
            item.GetComponent<SpriteRenderer>().sprite = ItemDataLoader.GetSpriteByPath(itemData.spritePath);
            AddItemInstanceToDic(itemData.itemName, item);
        }
    }

    /// <summary>
    /// 매개변수 itemType에 맞는 컴포넌트를 가진 GameObject를 생성해 해당 컴포넌트 타입으로 반환
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="itemType"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static T GetItemInstanceByType<T>(ItemType itemType) where T : class
    {
        var obj = Instantiate(Instance._itemPrefab);
        obj.transform.SetParent(Instance.transform);
        obj.transform.position = new Vector2(Instance._itemSpawnPos, Instance._itemSpawnPos);

        switch (itemType)
        {
            case ItemType.WEAPONGUN:
                return obj.AddComponent<WeaponGun>() as T;
            case ItemType.WEAPONSUB:
                return obj.AddComponent<WeaponSub>() as T;
            case ItemType.WEAPONTHROW:
                return obj.AddComponent<WeaponThrow>() as T;
            case ItemType.RECOVERY:
                return obj.AddComponent<ItemRecovery>() as T;
            case ItemType.DEFENSIVE:
                return obj.AddComponent<ItemDefensive>() as T;
            case ItemType.SCOPE:
                return obj.AddComponent<ItemScope>() as T;
            case ItemType.AMMO:
                return obj.AddComponent<ItemAmmo>() as T;
        }

        return null;
    }

    public static Dictionary<int, ItemData> GetItemDataDic()
    {
        return Instance._itemDataDictionary;
    }

    public static Dictionary<int, WeaponData> GetWeaponDataDic()
    {
        return Instance._weaponDataDictionary;
    }

    public static Dictionary<string, Item> GetItemDic()
    {
        return Instance._itemDictionary;
    }

    public static void AddItemDataToDic(int itemID, ItemData itemData) =>
        GetItemDataDic().Add(itemID, itemData);

    public static void AddWeaponDataToDic(int itemID, WeaponData weaponData) =>
        GetWeaponDataDic().Add(itemID, weaponData);

    public static void AddItemInstanceToDic(string itemName, Item item) =>
        GetItemDic().Add(itemName, item);

    public static ItemData GetItemDataByID(int id)
    {
        ItemData itemData = Instance._itemDataDictionary[id];

        if (itemData == null)
        {
            Debug.LogError($"ItemData with key {id} is not found.");
        }

        return itemData;
    }

    public static WeaponData GetWeaponDataByID(int id)
    {
        WeaponData weaponData = Instance._weaponDataDictionary[id];

        if (weaponData == null)
        {
            Debug.LogError($"WeaponData with key {id} is not found.");
        }

        return weaponData;
    }

    public static Item GetItemByName(string name)
    {
        Item item = Instance._itemDictionary[name];

        if (item == null)
        {
            Debug.LogError($"Item with key {name} is not found.");
        }

        return item;
    }

    public static T GetItemByName<T>(string name)
    {
        Item item = null;
        
        if (GetItemDic().TryGetValue(name, out item))
        {
            if (item.gameObject.TryGetComponent(out T component))
            {
                return component;
            }
            else
            {
                Debug.LogError($"Component of Item is not found");
            }
        }
        else
        {
            Debug.LogError($"Item with key {name} is not found.");
        }

        return default(T);
    }
}
