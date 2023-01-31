using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public partial class Player
{
    // Item Acquire() ~
    private void OnTriggerStay2D(Collider2D collision) // rigid -> never sleep option
    {
        if (collision.CompareTag("River"))
        {
            _tickTimerWaterEffect += Time.deltaTime;
            if (_tickTimerWaterEffect > timerWaterEffect)
            {
                _tickTimerWaterEffect = 0.0f;
                ObjectPoolManager.AllocObject("WaterEffect", transform.position);
            }
        }

        if(collision.CompareTag("Item") == false)
        {
            return;
        }

        Item target = null;
        if (collision.TryGetComponent<Item>(out target) == false)
        {
            //Debug.Log($"Item {target} is null");
            return;
        }

        if (photonView.IsMine == true)
        {
            _info.HUD.ActiveOnPressFView(target.itemData.itemName);
        }

        if (_input.acquire == true)
        {
            if (photonView.IsMine == true)
            {
                ClassifyAcquireByItemType(target);
            }
        }
    }

    //Item Ÿ�Կ� ���� �Լ� ȣ��
    public void ClassifyAcquireByItemType(Item item)
    {
        switch (item.itemData.itemType)
        {
            case ItemType.WEAPONGUN:
                {
                    AcquireGun(item);
                    break;
                }
            case ItemType.WEAPONSUB:
                {
                    AcquireSub(item);
                    break;
                }
            case ItemType.WEAPONTHROW:
                {
                    AcquireThrow(item);
                    break;
                }
            case ItemType.RECOVERY:
                {
                    AcquireRecovrey(item);
                    break;
                }
            case ItemType.DEFENSIVE:
                {
                    AcquireDefensive(item);
                    break;
                }
            case ItemType.SCOPE:
                {
                    AcquireScope(item);
                    break;
                }
            case ItemType.AMMO: //Bullet
                {
                    AcquireAmmo(item);
                    break;
                }
            case ItemType.NULL:
            default:
                {
                    Debug.Log("This Item don't have type");
                    break;
                }
        }
    }

    private void AcquireGun(Item item)
    {
        WeaponGun gun = null;
        if (item.TryGetComponent(out gun) == false)
        {
            Debug.LogError($"WeaponGun {gun} is null");
            return;
        }

        int slotIndex = _curSlot;
        if (_info.IsNullWeaponGunSlot() == true)
        {
            slotIndex = _info.GetNullWeaponGunIndex();
            if (slotIndex == ConstNums.numberOfWeaponGun)
            {
                Debug.LogError("This Function has error!");
            }
        }

        //���� �ֿ� �� �ִ� ���¶��, slotIndex�� ��ȿ�� ������ ����.
        if (_info.IsGunSlot(slotIndex) == true)
        {
            //���� ������ �̹� ���Ⱑ �ִٸ�,
            if (_info.IsNullWeaponSlot(slotIndex) == false)
            {
                // Ŀ�� �ٲٱ�
                if (gun.itemData.itemName == "rocketlauncher")
                {
                    _info.HUD.ChangeActiveCursor(true);
                }
                else
                {
                    _info.HUD.ChangeActiveCursor(false);
                }

                ReplaceGun(slotIndex);
            }
            //WeaponSlot�� ȹ���� �������� ������ �Է��Ѵ�.
            _info.ApplyWeaponDataInSlot(slotIndex, gun);

            NetworkManager.Instance.DisableItemByViewID(gun.itemData.viewID);

            if (_info.IsGunSlot(_curSlot) == true)
            {
                HUDRefreshSlotBullet();
            }
            else if (_curSlot == ConstNums.subWeaponIndex)
            {
                TryChangeSlot(slotIndex);
            }
        }
    }

    private void ReplaceGun(int slotIndex)
    {
        StopAction();
        //�����Ǿ��ִ� �Ѿ��� �����ϰ�, ���� ���� ������, �ش��ϴ� HUD�� Refresh���ش�.
        int preBulletType = _info.CastIntBulletType(slotIndex);
        _info.AddBulletSlotValue(preBulletType, _info.GetLoadedBullet(slotIndex));
        _info.SetLoadedBullet(slotIndex, 0);
        DropItemManager.DitchItem(_info.GetWeaponInSlot(slotIndex), transform.position, -CalcAttackDir());

        AutoReloading();
    }

    private void AcquireSub(Item item)
    {
        WeaponSub sub = null;
        if (item.TryGetComponent(out sub) == false)
        {
            Debug.LogError($"WeaponSub {sub} is null");
            return;
        }

        //������ ��� �ִ� �������⸦ (�ָ��� �ƴ϶��) �������Ѵ�.
        if (_info.IsBasicSubWeapon() == false)
        {
            DropItemManager.DitchItem(_info.GetWeaponInSlot(ConstNums.subWeaponIndex), transform.position, -CalcAttackDir());
        }
        //WeaponSlot�� ȹ���� �������� ������ �Է��Ѵ�.
        _info.ApplyWeaponDataInSlot(ConstNums.subWeaponIndex, sub);

        if (_info.IsSubSlot(_curSlot) == true)
        {
            photonView.RPC("RPC_ChangeAnim", RpcTarget.AllBuffered, sub.itemData.itemName);
        }

        NetworkManager.Instance.DisableItemByViewID(sub.itemData.viewID);
    }

    private void AcquireThrow(Item item)
    {
        WeaponThrow thr = null;
        if (item.TryGetComponent(out thr) == false)
        {
            Debug.LogError($"WeaponThrow {thr} is null");
            return;
        }

        _info.ApplyWeaponThrowItemDataInSlot(thr);
        NetworkManager.Instance.DisableItemByViewID(thr.itemData.viewID);
    }

    private void AcquireRecovrey(Item item)
    {
        ItemRecovery recovery = null;
        if (item.TryGetComponent(out recovery) == false)
        {
            Debug.LogError($"ItemRecovery {recovery} is null");
            return;
        }

        //RecoverSlot�� ȹ���� �������� ������ �Է��Ѵ�.
        _info.ApplyRecoveryItemDataInSlot(recovery);
        NetworkManager.Instance.DisableItemByViewID(recovery.itemData.viewID);
    }

    private void AcquireDefensive(Item item)
    {
        ItemDefensive defensive = null;
        if (item.TryGetComponent(out defensive) == false)
        {
            Debug.LogError($"ItemDefensive {defensive} is null");
            return;
        }

        // ���� �� ������ ����ų�, ȹ���ϴ� �������� �� ���� �������̶�� ȹ��
        if (_info.CanAcquireItemDefensive(defensive))
        {
            //DefensiveSlot�� ȹ���� �������� ������ �Է��Ѵ�.
            int slotIndex = _info.CastIntDefensiveType(defensive);
            if(_info._defSlot[slotIndex] != null)
            {
                DropItemManager.DitchItem(_info._defSlot[slotIndex], transform.position, -CalcAttackDir());
            }

            _info.ApplyDefensiveDataInSlot(defensive);
            NetworkManager.Instance.DisableItemByViewID(defensive.itemData.viewID);
        }
        else
        {
            _info.HUD.CreateInformText(_info.GetEquipWornBetterMessage());
        }
    }

    private void AcquireScope(Item item)
    {
        ItemScope scope = null;
        if (item.TryGetComponent(out scope) == false)
        {
            Debug.LogError($"ItemScope {scope} is null");
            return;
        }

        // ���� ���� ���� Scope��� ȹ��
        if (_info.CanAcquireItemScope(scope) == true)
        {
            //������ ȹ�� _ ��Ʈ������ ���� _info.scope�� ���� �־��ش�.
            _info.AddScopeValue(scope.itemData.lensPower);

            if (_info.IsBetterScope(scope))
            {
                _info.ChangeScope(scope.itemData.lensPower);
            }
            
            NetworkManager.Instance.DisableItemByViewID(scope.itemData.viewID);

            //ó�� �Ծ��⶧���� Create�� ������Ѵ�.
            _info.HUD.CreateSlotScope(scope.itemData.lensPower, _info.GetCurEyeSight() - _info.GetBasicEyeSight());
        }
        else
        {
            _info.HUD.CreateInformText(_info.GetEquipAlreadyHaveMessage());
        }
    }

    private void AcquireAmmo(Item item)
    {
        ItemAmmo ammo = null;
        if (item.TryGetComponent(out ammo) == false)
        {
            Debug.LogError($"ItemAmmo {ammo} is null");
            return;
        }

        int bulletType = _info.CastIntAmmoType(ammo);

        _info.ApplyAmmoDataInSlot(bulletType, ammo);
        NetworkManager.Instance.DisableItemByViewID(ammo.itemData.viewID);

        if (_curSlot < ConstNums.subWeaponIndex)
        {
            //���� ���� ��� �ִٸ� HUD �Ʒ����Կ��� ���� �����ؾߵ� ���� �ֱ� ������ Ȯ�����ش�.
            HUDRefreshSlotBullet();
            AutoReloading();
        }
    }
    // ~ Item Acquire ()
}
