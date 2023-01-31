using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public partial class Player
{
    // Reloading() ~ 
    //장전 가능한 상태인지 검사 함수
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

    //현재 장전된 총알이 없고, 해당 총알을 가지고 있다면 자동으로 장전
    private void AutoReloading()
    {
        bool isZeroLoadedBullet = (_info._loadedBullet[_curSlot] == 0);
        bool isNotZeroBullet = (_info.GetHavingBullet(_info.CastIntBulletType(_curSlot)) > 0);

        if (isZeroLoadedBullet && isNotZeroBullet)
        {
            StartCoroutine("ReloadingGun");
        }
    }

    //장전 쿨타임 적용
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
