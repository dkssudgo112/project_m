using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public partial class Player
{
    public void DropWeaponGun(int slotIndex)
    {
        WeaponGun gun = _info.GetWeaponGunInSlot(slotIndex);
        if (gun == null)
        {
            Debug.LogError($"{gun} is null");
            return;
        }

        StopAction();

        int gunBulletType = _info.CastIntBulletType(slotIndex);
        if (_info.IsNullBulletSlot(gunBulletType) == false)
        {
            _info.AddBulletSlotValue(gunBulletType, _info.GetLoadedBullet(slotIndex));
            _info.SetLoadedBullet(slotIndex, 0);

            _info.HUD.RefreshSlotAmmo(gunBulletType, _info._bulletSlot[gunBulletType].itemData.itemCount);
        }

        DropItemManager.DitchItem(gun, transform.position, -(CalcAttackDir()));
        _info.ClearWeaponInSlot(slotIndex);

        _info.HUD.RefreshSlotWeapon(slotIndex, _info.GetWeaponInSlot(slotIndex));

        if (slotIndex == _curSlot)
        {
            TryChangeSlot(ConstNums.subWeaponIndex);
        }
    }

    public void DropWeaponSub(int slotIndex)
    {
        if (_info.IsBasicSubWeapon() == true)
        {
            return;
        }

        WeaponSub sub = _info.GetWeaponInSlot(slotIndex).GetComponent<WeaponSub>();

        StopAction();

        DropItemManager.DitchItem(sub, transform.position, -(CalcAttackDir()));
        _info.SetWeaponInSlot(slotIndex, _info.GetFist());
        _info.HUD.RefreshSlotWeapon(slotIndex, _info.GetFist());

        ChangeAnimFist();
    }

    public void DropWeaponThrow(int slotIndex)
    {
        int throwSlotIndex = slotIndex + _info.GetCurWeaponThrowIndex();
        if (throwSlotIndex >= ConstNums.numberOfWeapon)
        {
            return;
        }

        WeaponThrow thr = _info.GetWeaponInSlot(throwSlotIndex).GetComponent<WeaponThrow>();
        if (thr == null)
        {
            return;
        }

        if (thr.GetItemCount() <= 0)
        {
            return;
        }

        StopAction();

        DropItemManager.DitchItem(thr, transform.position, -(CalcAttackDir()));

        //투척무기가 비었으면
        if (thr.GetItemCount() == 0)
        {
            //다른 투척무기도 없으면
            if (_info.IsNullOtherWeaponThrow(throwSlotIndex) == true)
            {
                _info.ClearCurWeaponThrowIndex();
                if (throwSlotIndex == _curSlot)
                {
                    TryChangeSlot(ConstNums.subWeaponIndex);
                }
            }
            else
            {
                if (_curSlot >= ConstNums.weaponThrowIndex)
                {
                    TryChangeSlot(_curSlot);
                    thr = _info.GetWeaponInSlot(_curSlot).GetComponent<WeaponThrow>();
                }
                else
                {
                    thr = _info.GetWeaponInSlot(_info.ChangeThrowIndex(throwSlotIndex)).GetComponent<WeaponThrow>();
                }
            }
        }
        _info.HUD.RefreshSlotWeaponThrow(thr);
    }

    public void DropItemRecovery(int slotIndex)
    {
        if (_info.IsNullRecoverySlot(slotIndex) == true)
        {
            return;
        }

        if (_info._recoverySlot[slotIndex].itemData.itemCount <= 0)
        {
            return;
        }

        StopAction();

        DropItemManager.DitchItem(_info._recoverySlot[slotIndex], transform.position, -(CalcAttackDir()));
        _info.HUD.RefreshSlotRecovery(_info._recoverySlot[slotIndex]);
    }

    public void DropItemAmmo(int slotIndex)
    {
        if (_info.IsNullBulletSlot(slotIndex) == true)
        {
            return;
        }

        if (_info.GetHavingBullet(slotIndex) <= 0)
        {
            return;
        }

        StopAction();

        DropItemManager.DitchItem(_info._bulletSlot[slotIndex], transform.position, -(CalcAttackDir()));
        _info.HUD.RefreshSlotAmmo(slotIndex, _info.GetHavingBullet(slotIndex));

        if (_curSlot < ConstNums.numberOfWeaponGun)
        {
            HUDRefreshSlotBullet();
        }
    }
}
