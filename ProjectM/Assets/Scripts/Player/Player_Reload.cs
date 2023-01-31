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
        bool isNotMaxBullet = (_info._loadedBullet[_curSlot] < _info.GetWeaponInSlot(_curSlot).GetComponent<WeaponGun>().maxBullet);

        return (isNotZeroBullet && isNotMaxBullet);
    }

    private void ReloadGun()
    {
        WeaponGun gun = _info.GetWeaponGunInSlot(_curSlot);

        int havingBullet = _info.GetHavingBullet(_info.CastIntBulletType(_curSlot));

        if(havingBullet > 0)
        {
            int reloadBulletCount = gun.maxBullet - _info._loadedBullet[_curSlot];
            if(havingBullet >= reloadBulletCount)
            {
                _info._loadedBullet[_curSlot] = gun.maxBullet;
            }
            else
            {
                reloadBulletCount = havingBullet;
                _info._loadedBullet[_curSlot] += havingBullet;
            }
            _info.BulletSlotAddValue(_info.CastIntBulletType(_curSlot), -reloadBulletCount);
            HUDUpdate_slotBullet();
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
        bool isZeroLoadedBullet = (_info._loadedBullet[_curSlot] == 0);
        bool isNotZeroBullet = (_info.GetHavingBullet(_info.CastIntBulletType(_curSlot)) > 0);

        if (isZeroLoadedBullet && isNotZeroBullet)
        {
            StartCoroutine("ReloadingGun");
        }
    }

    //���� ��Ÿ�� ����
    private IEnumerator ReloadingGun()
    {
        _info.HUD.SetActiveReloadingView(true);
        _state = State.RELOADING;
        yield return new WaitForSeconds(_info.GetWeaponGunInSlot(_curSlot).loadedTime);
        ReloadGun();
        _info.HUD.SetActiveReloadingView(false);
    }
    // ~ Reloading()
}
