using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// 22.08.30_jayjeong
/// <br> �������� ��� �˰����� �����ϴ� �Ŵ��� </br>
/// </summary>
public class DropItemManager : MonoBehaviour
{
    private static DropItemManager Instance = null;

    [Header("Weighted Random Sampling ������ ����Ʈ")]
    [SerializeField]
    private WeightedRandomSampler<ItemType> _dropTypeList = null;
    [Header("Poisson-disc Sampling ������")]
    [SerializeField]
    private float _radius = 0.8f;
    [SerializeField]
    private Vector2 _regionSize = new Vector2(5, 5);
    [SerializeField]
    private int _rejectionSamples = 10;
    [SerializeField]
    private float _adjustedPosOffset = -5f;
    [Header("BulletType ������ ��� ������")]
    [SerializeField]
    private float _bulletDropDistance = 0.8f;
    [SerializeField]
    private int _minBulletDropCount = 10;

    private const string _scopeName = "x������";
    private const int _scopeConvertUnit = 2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Weighted Random Sampling �˰����� ���� ������ ���� ���� ����� Ȯ���� �������� ���
    /// </summary>
    /// <param name="spawnPos"></param>
    /// <param name="spreadDir"></param>
    public static void DropRandomItem(Vector3 spawnPos, Vector2 spreadDir) =>
        Instance.DropWeightedRandomItem(spawnPos, spreadDir, Quaternion.identity);

    /// <summary>
    /// Weighted Random Sampling �˰����� ���� ������ ���� ���� ����� Ȯ���� �������� ���
    /// </summary>
    /// <param name="spawnPos"></param>
    /// <param name="spreadDir"></param>
    /// <param name="rotation"></param>
    public static void DropRandomItem(Vector3 spawnPos, Vector2 spreadDir, Quaternion rotation) =>
        Instance.DropWeightedRandomItem(spawnPos, spreadDir, rotation);

    // WeightedRandomList Ŭ������ ������ ���� ���� ����� ���� Ȯ���� �������� ����ϴ� �޼��� 
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

    // WeaponGun�� ����Ǵ� ��ġ �� ���� bulletType�� ItemAmmo ����ϴ� �޼���  
    private void DropAmmoByBulletType(BulletType bulletType, Vector3 spawnPos, Vector2 spreadDir)
    {
        NetworkManager.Instance.RequestDropItemToMaster(GetRandomItemByType(ItemType.AMMO, bulletType).name,
            new Vector3(spawnPos.x + _bulletDropDistance, spawnPos.y, spawnPos.z), spreadDir);

        NetworkManager.Instance.RequestDropItemToMaster(GetRandomItemByType(ItemType.AMMO, bulletType).name,
            new Vector3(spawnPos.x - _bulletDropDistance, spawnPos.y, spawnPos.z), spreadDir);
    }

    // dropType�� ��ġ�ϴ� �����۵� �� ������ �������� ��ȯ�ϴ� �޼��� 
    private Item GetRandomItemByType(ItemType dropType, BulletType bulletType)
    {
        List<Item> dropItemList = null;

        // ItemType�� Ammo�� ��� bulletType���� �ѹ� �� �з� 
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
    /// ��� ���� ���� ���� �������� position�� ��ġ�� ������ �޼��� 
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
    /// ��� ���� ���� ���� �������� dropCount��ŭ position�� ��ġ�� ������ �޼��� 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="position"></param>
    /// <param name="spreadDir"></param>
    public static void DitchItem(Item item, int dropCount, Vector3 position, Vector3 spreadDir) =>
        Instance.DitchItemByType(item, dropCount, position, spreadDir);

    // �������� Ÿ�Կ� ���� �������� dropCount��ŭ position�� ��ġ�� ������ �޼��� 
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

            // ������ ������ ��� 
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
    /// Poisson-disc Sampling �˰����� ���� ����� �÷��̾��� �������� ������ �������� ����ϴ� �޼��� 
    /// </summary>
    /// <param name="playerInfo"></param>
    public static void DropItemsOfDeadPlayer(PlayerInfo playerInfo)
    {
        // Drop�ؾ� �ϴ� ��� Item�� dropItems�� ��� �Ѳ����� ó�� 
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

        // WeaponThrow (itemCount Ȯ�� �ʿ�)
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

        // ItemRecovery (itemCount Ȯ�� �ʿ�) 
        foreach (var recoverItem in playerInfo._recoverySlot)
        {
            if (recoverItem != null && recoverItem.itemData.itemCount > 0)
            {
                dropItems.Add(recoverItem);
            }
        }

        // ItemAmmo (itemCount Ȯ�� �ʿ�) 
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

    // Poisson-disc Sampling �˰����� ���� �������� ������ �������� ��� 
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

            // �Ѿ��� ��� ����ؼ� �ݾ� �ɰ��� ��� 
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

    // ����� �������� ������ ����ؼ� ��ȯ�ϴ� �޼���(�������� ������ �ּҵ������ �ʰ� �� �ݰ�) 
    private int GetDropCount(ItemData itemData)
    {
        int minDropCount = itemData.itemType == ItemType.AMMO ? _minBulletDropCount : 1;
        int dropCount = itemData.itemCount > minDropCount ? itemData.itemCount / 2 : itemData.itemCount;

        return dropCount;
    }
}
