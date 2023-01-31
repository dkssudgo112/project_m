using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// 22.07.22_jayjeong
/// <br> ��� ������ ���� �� ������ ����� �����ϴ� Ŭ���� </br>
/// </summary>
public class ItemManager : MonoBehaviour
{
    private static ItemManager Instance = null;
    private JsonDataLoader _dataLoader = new JsonDataLoader();

    // "name"�� Key, Item Ŭ������ Value�� ���� ��� ������ ��ųʸ� 
    private Dictionary<string, Item> _itemDictionary = new Dictionary<string, Item>();

    // ���Կ� ����� ������ ���� �����ϴ� ��� ������ ����Ʈ 
    [SerializeField]
    private WeightedRandomList<ItemType> _dropTypeList = null;

    [SerializeField]
    private float _itemSpawnPos = 500f;
    [SerializeField]
    private float _bulletDropDistance = 0f;
    [SerializeField]
    private int _minBulletDropCount = 10;
    // ��� �������� �⺻Ʋ�� �Ǵ� ������ 
    public GameObject _itemPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _dataLoader.ParsingItemData();
        _dataLoader.ParsingWeaponData();
        ObjectPoolManager.CreateItemPoolsOnStart();
    }

    private void Update()
    {
        // �׽�Ʈ�� Ű��ǲ 
        //if (Input.GetKeyDown(KeyCode.V))
        //{
        //    DropGunPreferredItem(transform.position);
        //}
    }

    /// <summary>
    /// �Ű����� type�� ������Ʈ�� ���� GameObject�� ������ �ش� ������Ʈ Ÿ������ ��ȯ
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static T GetInstanceByType<T>(ItemType type) where T : class
    {
        GameObject obj = Instantiate(Instance._itemPrefab);
        obj.transform.SetParent(Instance.transform);
        obj.transform.position = new Vector2(Instance._itemSpawnPos, Instance._itemSpawnPos);

        switch (type)
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

    // WeightedRandomList Ŭ������ ���� ������ ���� ���� ����� �ѱ� ������ ������ �������� ��� 
    public static void DropRandomItem(Vector3 spawnPos, Vector2 startVector) =>
    Instance.DropWeightedRandomItem(spawnPos, startVector, Quaternion.identity);

    public static void DropRandomItem(Vector3 spawnPos, Vector2 startVector, Quaternion rotation) =>
        Instance.DropWeightedRandomItem(spawnPos, startVector, rotation);

    /// <summary>
    /// �̸� ������ ���� ���� ����� ���� Ȯ���� �������� ����ϴ� �޼��� 
    /// </summary>
    /// <param name="spawnPos"></param>
    /// <param name="startVector"></param>
    /// <param name="rotation"></param>
    private void DropWeightedRandomItem(Vector3 spawnPos, Vector2 startVector, Quaternion rotation)
    {
        var randomType = _dropTypeList.GetRandomValue();

        // randomType�� ��ġ�ϴ� itemType�� ������ ����Ʈ�� ������ �� �� �ϳ� ��� 
        var dropTypeList = GetItemDic().Where(x => x.Value.type == randomType).ToList();

        int randomIdx = UnityEngine.Random.Range(0, dropTypeList.Count);
        var itemToDrop = dropTypeList[randomIdx].Value;

        NetworkManager.DropItem(itemToDrop.name, spawnPos, startVector);

        if (itemToDrop is WeaponGun)
        {
            Weapon weapon = itemToDrop as WeaponGun;
            DropAmmoByBulletType(weapon.weaponData.bulletType, spawnPos, startVector);
        }
    }

    // ���� ����� ��ġ �� ���� bulletType�� ItemAmmo ����ϱ� 
    private void DropAmmoByBulletType(BulletType bulletType, Vector3 spawnPos, Vector2 startVector)
    {
        var dropTypeList = GetItemDic().Where(x => x.Value.type == ItemType.AMMO)
                                       .Where(x => x.Value.itemData.bulletType == bulletType).ToList();

        int randomIdx = UnityEngine.Random.Range(0, dropTypeList.Count);

        NetworkManager.DropItem(dropTypeList[randomIdx].Key,
                                 new Vector3(spawnPos.x + _bulletDropDistance, spawnPos.y, spawnPos.z), startVector);

        randomIdx = UnityEngine.Random.Range(0, dropTypeList.Count);

        NetworkManager.DropItem(dropTypeList[randomIdx].Key,
                         new Vector3(spawnPos.x - _bulletDropDistance, spawnPos.y, spawnPos.z), startVector);
    }

    // ���� �÷��̾��� �� ���Կ� ������ ������ ��� 
    public static void DropItemsOfDeadPlayer(PlayerInfo playerInfo)
    {
        // WeaponGunSlot�� ��� loadedBullet�� bulletSlot�� ��ȯ�ϴ� ���� 
        // DitchItem �Լ����� ó������ �ʰ� HUDScript���� �ϱ� ������ �������� ó�� �ʿ� 
        for (int i = 0; i < playerInfo._slotWeapon.Count(); i++)
        {
            if (playerInfo._slotWeapon[i] == null)
            {
                continue;
            }

            DitchItem(playerInfo, playerInfo._slotWeapon[i].GetWeapon());

            if (playerInfo._loadedBullet[i] > 0)
            {
                playerInfo._bulletSlot[Convert.ToInt32(playerInfo._slotWeapon[i].GetWeapon().GetComponent<WeaponGun>().bulletType)].itemCount
                    += playerInfo._loadedBullet[i];
            }
        }

        //DitchItem(playerInfo, playerInfo._weaponSubSlot);

        foreach (var defItem in playerInfo._defSlot)
        {
            DitchItem(playerInfo, defItem);
        }

        foreach (var recoverItem in playerInfo._recoverSlot)
        {
            DitchItem(playerInfo, recoverItem);
        }

        foreach (var bulletItem in playerInfo._bulletSlot)
        {
            DitchItem(playerInfo, bulletItem);
        }

        // WeaponThrow, Scope �߰��ؾ� ��. 
    }

    /// <summary>
    /// ��� ���� �� ������ ������ �޾� �ش� ������ ������ 
    /// </summary>
    /// <param name="item"></param>
    public static void DitchItem(PlayerInfo playerInfo, Item item)
    {
        if (item == null)
        {
            Debug.LogError($"The item to ditch is null");
            return;
        }

        switch (item.type)
        {
            case ItemType.WEAPONGUN:
            case ItemType.WEAPONSUB:
            case ItemType.DEFENSIVE:
                NetworkManager.DropItem(item.itemName, playerInfo.transform.position, Vector3.zero);
                break;

            case ItemType.WEAPONTHROW:
                // ���� �� �ۼ� 
                break;

            case ItemType.RECOVERY:
                ItemRecovery recovery = ObjectPoolManager.AllocItem<ItemRecovery>(item.itemName, playerInfo.transform.position);
                recovery.itemCount = item.GetComponent<ItemRecovery>().itemCount;
                break;

            case ItemType.SCOPE:
                // ���� �� �ۼ� 
                break;

            case ItemType.AMMO:
                ItemAmmo bullet = ObjectPoolManager.AllocItem<ItemAmmo>(item.itemName, playerInfo.transform.position);
                bullet.itemCount = item.GetComponent<ItemAmmo>().itemCount;
                break;

            default:
                Debug.LogError($"The type of item {item.name} is missing in the case.");
                break;
        }
    }

    public static void DitchItem(Item item, Vector3 position, Vector3 spreadDir)
    {
        if (item == null)
        {
            Debug.LogError($"The item to ditch is null");
            return;
        }

        switch (item.type)
        {
            // 1���� ��� 
            case ItemType.WEAPONGUN:
            case ItemType.WEAPONSUB:
            case ItemType.DEFENSIVE:
            case ItemType.SCOPE:
                NetworkManager.DropItem(item.itemName, position, spreadDir);
                break;
            
            // 1�� �̻� ��� 
            case ItemType.WEAPONTHROW:  
            case ItemType.RECOVERY:
            case ItemType.AMMO:
            // ������ ������ �ּҵ������ �ʰ� �� �ݰ��� ������ 
                int minDropCount = item.type == ItemType.AMMO ? Instance._minBulletDropCount : 1;
                int dropCount = item.itemCount > minDropCount ? item.itemCount / 2 : item.itemCount;
                item.itemCount -= dropCount;
                NetworkManager.DropItem(item.itemName, dropCount, position, spreadDir);
                break;

            default:
                Debug.LogError($"The type of item {item.name} is missing in the case.");
                break;
        }
    }

    public static void DitchItem(Item item, int dropCount, Vector3 position, Vector3 spreadDir)
    {
        if (item == null)
        {
            Debug.LogError($"The item to ditch is null");
            return;
        }

        switch (item.type)
        {
            case ItemType.WEAPONTHROW:
            case ItemType.RECOVERY:
            case ItemType.AMMO:
                NetworkManager.DropItem(item.itemName, dropCount, position, spreadDir);
                break;

            default:
                Debug.LogError($"The type of item {item.name} is missing in the case.");
                break;
        }
    }

    public static Dictionary<string, Item> GetItemDic() 
    { 
        return Instance._itemDictionary; 
    }

    public static Item GetItemByName(string name)
    {
        return Instance._itemDictionary.ContainsKey(name) ? Instance._itemDictionary[name] : null;
    }

    public static T GetItemByName<T>(string name)
    {
        if (Instance._itemDictionary.ContainsKey(name))
        {
            GameObject obj = Instance._itemDictionary[name].gameObject;

            if (obj.TryGetComponent(out T component))
            {
                return component;
            }
            else
            {
                throw new Exception($"Component of Item is not found");
            }
        }

        return default(T);
    }
}
