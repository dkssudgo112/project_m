using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// 22.08.30_jayjeong
/// <br> 아이템의 드랍 알고리즘을 관리하는 매니저 </br>
/// </summary>
public class DropItemManager : MonoBehaviour
{
    private static DropItemManager Instance = null;

    [Header("Weighted Random Sampling 아이템 리스트")]
    [SerializeField]
    private WeightedRandomSampler<ItemType> _dropTypeList = null;
    [Header("Poisson-disc Sampling 조정값")]
    [SerializeField]
    private float _radius = 0.8f;
    [SerializeField]
    private Vector2 _regionSize = new Vector2(5, 5);
    [SerializeField]
    private int _rejectionSamples = 10;
    [SerializeField]
    private float _adjustedPosOffset = -5f;
    [Header("BulletType 아이템 드랍 조정값")]
    [SerializeField]
    private float _bulletDropDistance = 0.8f;
    [SerializeField]
    private int _minBulletDropCount = 10;

    private const string _scopeName = "x스코프";
    private const int _scopeConvertUnit = 2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Weighted Random Sampling 알고리즘을 통해 설정된 무게 값에 비례한 확률로 아이템을 드랍
    /// </summary>
    /// <param name="spawnPos"></param>
    /// <param name="spreadDir"></param>
    public static void DropRandomItem(Vector3 spawnPos, Vector2 spreadDir) =>
        Instance.DropWeightedRandomItem(spawnPos, spreadDir, Quaternion.identity);

    /// <summary>
    /// Weighted Random Sampling 알고리즘을 통해 설정된 무게 값에 비례한 확률로 아이템을 드랍
    /// </summary>
    /// <param name="spawnPos"></param>
    /// <param name="spreadDir"></param>
    /// <param name="rotation"></param>
    public static void DropRandomItem(Vector3 spawnPos, Vector2 spreadDir, Quaternion rotation) =>
        Instance.DropWeightedRandomItem(spawnPos, spreadDir, rotation);

    // WeightedRandomList 클래스로 설정된 무게 값에 비례한 랜덤 확률로 아이템을 드랍하는 메서드 
    private void DropWeightedRandomItem(Vector3 spawnPos, Vector2 spreadDir, Quaternion rotation)
    {
        var randomType = _dropTypeList.GetRandomValue();

        Item itemToDrop = GetRandomItemByType(randomType, BulletType.NULL);

        NetworkManager.Instance.RequestDropItemToMaster(itemToDrop.name, spawnPos, spreadDir);

        if (itemToDrop is WeaponGun)
        {
            Weapon weapon = itemToDrop as WeaponGun;
            DropAmmoByBulletType(weapon.weaponData.bulletType, spawnPos, spreadDir);
        }
    }

    // WeaponGun이 드랍되는 위치 양 옆에 bulletType의 ItemAmmo 드랍하는 메서드  
    private void DropAmmoByBulletType(BulletType bulletType, Vector3 spawnPos, Vector2 spreadDir)
    {
        NetworkManager.Instance.RequestDropItemToMaster(GetRandomItemByType(ItemType.AMMO, bulletType).name,
            new Vector3(spawnPos.x + _bulletDropDistance, spawnPos.y, spawnPos.z), spreadDir);

        NetworkManager.Instance.RequestDropItemToMaster(GetRandomItemByType(ItemType.AMMO, bulletType).name,
            new Vector3(spawnPos.x - _bulletDropDistance, spawnPos.y, spawnPos.z), spreadDir);
    }

    // dropType과 일치하는 아이템들 중 랜덤한 아이템을 반환하는 메서드 
    private Item GetRandomItemByType(ItemType dropType, BulletType bulletType)
    {
        List<Item> dropItemList = null;

        // ItemType이 Ammo인 경우 bulletType으로 한번 더 분류 
        if (dropType == ItemType.AMMO)
        {
            dropItemList = ItemManager.GetItemDic().Where(x => x.Value.itemData.itemType == dropType)
                                                   .Where(x => x.Value.itemData.bulletType == bulletType)
                                                   .Select(x => x.Value).ToList();
        }
        else
        {
            dropItemList = ItemManager.GetItemDic().Where(x => x.Value.itemData.itemType == dropType)
                                                   .Select(x => x.Value).ToList();
        }

        int randomIdx = UnityEngine.Random.Range(0, dropItemList.Count);
        var itemToDrop = dropItemList[randomIdx];

        return itemToDrop;
    }

    /// <summary>
    /// 장비 해제 등을 통해 아이템을 position의 위치에 버리는 메서드 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="position"></param>
    /// <param name="spreadDir"></param>
    public static void DitchItem(Item item, Vector3 position, Vector3 spreadDir)
    {
        int dropCount = Instance.GetDropCount(item.itemData);
        Instance.DitchItemByType(item, dropCount, position, spreadDir);
    }

    /// <summary>
    /// 장비 해제 등을 통해 아이템을 dropCount만큼 position의 위치에 버리는 메서드 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="position"></param>
    /// <param name="spreadDir"></param>
    public static void DitchItem(Item item, int dropCount, Vector3 position, Vector3 spreadDir) =>
        Instance.DitchItemByType(item, dropCount, position, spreadDir);

    // 아이템의 타입에 따라 아이템을 dropCount만큼 position의 위치에 버리는 메서드 
    private void DitchItemByType(Item item, int dropCount, Vector3 position, Vector3 spreadDir)
    {
        if (item == null)
        {
            Debug.LogError($"The item to ditch is null");
            return;
        }

        switch (item.itemData.itemType)
        {
            case ItemType.WEAPONGUN:
            case ItemType.WEAPONSUB:
            case ItemType.DEFENSIVE:
            case ItemType.SCOPE:
                NetworkManager.Instance.RequestDropItemToMaster(item.itemData.itemName, dropCount, position, spreadDir);
                break;

            // 복수의 아이템 드랍 
            case ItemType.WEAPONTHROW:
            case ItemType.RECOVERY:
            case ItemType.AMMO:
                item.itemData.itemCount -= dropCount;
                NetworkManager.Instance.RequestDropItemToMaster(item.itemData.itemName, dropCount, position, spreadDir);
                break;

            default:
                Debug.LogError($"The type of item {item.name} is missing in the case.");
                break;
        }
    }

    /// <summary>
    /// Poisson-disc Sampling 알고리즘을 통해 사망한 플레이어의 아이템을 일정한 간격으로 드랍하는 메서드 
    /// </summary>
    /// <param name="playerInfo"></param>
    public static void DropItemsOfDeadPlayer(PlayerInfo playerInfo)
    {
        // Drop해야 하는 모든 Item을 dropItems에 담아 한꺼번에 처리 
        List<Item> dropItems = new List<Item>();

        // WeaponGun
        for (int i = 0; i < ConstNums.numberOfWeaponGun; i++)
        {
            if (playerInfo.IsNullWeaponSlot(i) == true)
            {
                continue;
            }

            if (playerInfo.GetLoadedBullet(i) > 0)
            {
                playerInfo._bulletSlot[Convert.ToInt32(playerInfo.GetWeaponGunInSlot(i).weaponData.bulletType)].itemData.itemCount
                    += playerInfo.GetLoadedBullet(i);
            }

            dropItems.Add(playerInfo._slotWeapon[i].GetWeapon());
        }

        // WeaponThrow (itemCount 확인 필요)
        for (int i = ConstNums.weaponThrowIndex; i < ConstNums.weaponThrowIndex + ConstNums.numberOfWeaponThrow; i++)
        {
            if (playerInfo._slotWeapon[i].GetWeapon() != null && playerInfo._slotWeapon[i].GetWeapon().itemData.itemCount > 0)
            {
                dropItems.Add(playerInfo._slotWeapon[i].GetWeapon());
            }
        }

        // WeaponSub
        if (playerInfo.IsBasicSubWeapon() == false)
        {
            dropItems.Add(playerInfo._slotWeapon[ConstNums.subWeaponIndex].GetWeapon());
        }

        // ItemDefensive
        foreach (var defItem in playerInfo._defSlot)
        {
            if (defItem != null)
            {
                dropItems.Add(defItem);
            }
        }

        // ItemRecovery (itemCount 확인 필요) 
        foreach (var recoverItem in playerInfo._recoverySlot)
        {
            if (recoverItem != null && recoverItem.itemData.itemCount > 0)
            {
                dropItems.Add(recoverItem);
            }
        }

        // ItemAmmo (itemCount 확인 필요) 
        foreach (var bulletItem in playerInfo._bulletSlot)
        {
            if (bulletItem != null && bulletItem.itemData.itemCount > 0)
            {
                dropItems.Add(bulletItem);
            }
        }

        // ItemScope
        for (int i = 1; i < ConstNums.numberOfItemScope; i++)
        {
            if (playerInfo.HUD.IsScopeSlotActive(i))
            {
                string itemName = String.Format($"{i * _scopeConvertUnit}{_scopeName}");
                dropItems.Add(ItemManager.GetItemByName(itemName));
            }
        }

        Instance.DropItemsAtRegularDistances(dropItems, playerInfo.transform.position);
    }

    // Poisson-disc Sampling 알고리즘을 통해 아이템을 일정한 간격으로 드랍 
    private void DropItemsAtRegularDistances(List<Item> dropItemList, Vector3 playerPos)
    {
        List<Vector2> samplePoints = PoissonDiscSampler.GeneratePoints(_radius, _regionSize, _rejectionSamples);
        Vector2 adjustedPos = new Vector2(playerPos.x + _adjustedPosOffset, playerPos.y + _adjustedPosOffset);
        int index = 0;

        foreach (var point in samplePoints)
        {
            if (index >= dropItemList.Count)
            {
                break;
            }

            Vector2 dropPos = new Vector2(adjustedPos.x + point.x, adjustedPos.y + point.y);
            Vector3 dir = dropPos - adjustedPos;

            // 총알의 경우 계속해서 반씩 쪼개서 드랍 
            if (dropItemList[index].itemData.itemType == ItemType.AMMO)
            {
                DitchItem(dropItemList[index], dropPos, dir);

                if (dropItemList[index].itemData.itemCount > 0)
                {
                    dropItemList.Add(dropItemList[index]);
                }
            }
            else
            {
                DitchItem(dropItemList[index], dropItemList[index].itemData.itemCount, dropPos, dir);
            }

            index++;
        }
    }

    // 드랍할 아이템의 개수를 계산해서 반환하는 메서드(아이템의 개수가 최소드랍단위 초과 시 반개) 
    private int GetDropCount(ItemData itemData)
    {
        int minDropCount = itemData.itemType == ItemType.AMMO ? _minBulletDropCount : 1;
        int dropCount = itemData.itemCount > minDropCount ? itemData.itemCount / 2 : itemData.itemCount;

        return dropCount;
    }
}
