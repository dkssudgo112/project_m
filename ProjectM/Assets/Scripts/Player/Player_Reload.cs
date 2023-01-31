using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public partial class Player
{
    // Reloading() ~ 
    //���� ������ �������� �˻� �Լ�
    private bool CanReloadingGun()
    {
        if (_info.IsGunSlot(_curSlot) == false || _state != State.LIVE)
        {
            return false;
        }

        bool isNotZeroBullet = (_info.GetHavingBullet(_info.CastIntBulletType(_curSlot)) > 0);
        bool isNotMaxBullet = (_info.GetLoadedBullet(_curSlot) < _info.GetWeaponInSlot(_curSlot).GetComponent<WeaponGun>().weaponData.maxBullet);

        if (isNotMaxBullet == false)
        {
            _info.HUD.CreateInformText(_info.GetCantReloadingMessage());
        }

        return (isNotZeroBullet && isNotMaxBullet);
    }

    private void ReloadGun()
    {
        WeaponGun gun = _info.GetWeaponGunInSlot(_curSlot);

        int havingBullet = _info.GetHavingBullet(_info.CastIntBulletType(_curSlot));

        if(havingBullet > 0)
        {
            int reloadBulletCount = gun.weaponData.maxBullet - _info.GetLoadedBullet(_curSlot);
            if(havingBullet >= reloadBulletCount)
            {
                _info.SetLoadedBullet(_curSlot, gun.weaponData.maxBullet);
            }
            else
            {
                reloadBulletCount = havingBullet;
                _info.AddLoadedBulletValue(_curSlot, reloadBulletCount);
            }
            _info.AddBulletSlotValue(_info.CastIntBulletType(_curSlot), -reloadBulletCount);
            HUDRefreshSlotBullet();
            _state = State.LIVE;
        }
        else
        {
            Debug.Log("You don't have bullet");
        }
    }

    //���� ������ �Ѿ��� ����, �ش� �Ѿ��� ������ �ִٸ� �ڵ����� ����
    private void AutoReloading()
    {
        bool isZeroLoadedBullet = (_info.GetLoadedBullet(_curSlot) == 0);
        bool isNotZeroBullet = (_info.GetHavingBullet(_info.CastIntBulletType(_curSlot)) > 0);

        if (isZeroLoadedBullet && isNotZeroBullet)
        {
            StartCoroutine("ReloadingGun");
        }
    }

    //���� ��Ÿ�� ����
    private IEnumerator ReloadingGun()
    {
        _info.HUD.PlayActionViewOn("���� ��...", _info.GetWeaponGunInSlot(_curSlot).weaponData.loadedTime);
        _state = State.RELOADING;
        yield return new WaitForSeconds(_info.GetWeaponGunInSlot(_curSlot).weaponData.loadedTime);
        ReloadGun();
        _info.HUD.PlayActionViewOff();
    }
    // ~ Reloading()
}
