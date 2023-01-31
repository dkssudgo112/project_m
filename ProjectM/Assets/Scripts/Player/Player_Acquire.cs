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
            _info.HUD.ActiveOnPressFView(target.itemName);
        }

        if (_input.acquire == true)
        {
            if (photonView.IsMine == true)
            {
                SortAcquireType(target);
            }
        }
    }

    //Item 타입에 따라 함수 호출
    public void SortAcquireType(Item item)
    {
        switch (item.type)
        {
            case ItemType.WEAPONGUN:
                {
                    WeaponGun gun = null;
                    if (item.TryGetComponent(out gun) == false)
                    {
                        Debug.LogError($"WeaponGun {gun} is null");
                        break;
                    }

                    Acquire(gun);
                    break;
                }
            case ItemType.WEAPONSUB:
                {
                    WeaponSub sub = null;
                    if (item.TryGetComponent(out sub) == false)
                    {
                        Debug.LogError($"WeaponSub {sub} is null");
                        break;
                    }

                    Acquire(sub);
                    break;
                }
            case ItemType.WEAPONTHROW:
                {
                    WeaponThrow thr = null;
                    if (item.TryGetComponent(out thr) == false)
                    {
                        Debug.LogError($"WeaponThrow {thr} is null");
                        break;
                    }

                    Acquire(thr);
                    break;
                }
            case ItemType.RECOVERY:
                {
                    ItemRecovery recovery = null;
                    if (item.TryGetComponent(out recovery) == false)
                    {
                        Debug.LogError($"ItemRecovery {recovery} is null");
                        break;
                    }

                    Acquire(recovery);
                    break;
                }
            case ItemType.DEFENSIVE:
                {
                    ItemDefensive defensive = null;
                    if (item.TryGetComponent(out defensive) == false)
                    {
                        Debug.LogError($"ItemDefensive {defensive} is null");
                        break;
                    }

                    Acquire(defensive);
                    break;
                }
            case ItemType.SCOPE:
                {
                    ItemScope scope = null;
                    if (item.TryGetComponent(out scope) == false)
                    {
                        Debug.LogError($"ItemScope {scope} is null");
                        break;
                    }

                    Acquire(scope);
                    break;
                }
            case ItemType.AMMO: //Bullet
                {
                    ItemAmmo ammo = null;
                    if (item.TryGetComponent(out ammo) == false)
                    {
                        Debug.LogError($"ItemAmmo {ammo} is null");
                        break;
                    }

                    Acquire(ammo);
                    break;
                }
            case ItemType.NULL:
                {
                    Debug.Log("ItemType is null");
                }
                break;
            default:
                {
                    Debug.Log("This Item don't have type");
                    break;
                }

        }
    }

    private void Acquire(WeaponGun gun)
    {
        int slotIndex = _curSlot;
        if (_info.IsNullWeaponGunSlot() == true)
        {
            slotIndex = _info.GetNullWeaponGunIndex();
            if (slotIndex == ConstNums.numberOfWeaponGun)
            {
                Debug.LogError("This Function has error!");
            }
        }

        //총을 주울 수 있는 상태라면, slotIndex가 유효한 값으로 들어간다.
        if (_info.IsGunSlot(slotIndex) == true)
        {
            

            //현재 슬롯이 이미 무기가 있다면,
            if (_info.IsNullWeaponSlot(slotIndex) == false)
            {
                // 커서 바꾸기
                if (gun.itemName == "rocketlauncher")
                {
                    Cursor.visible = false;
                    _info.HUD.ChangeActiveCursor(true);
                }
                else
                {
                    Cursor.visible = true;
                    _info.HUD.ChangeActiveCursor(false);
                }

                ReplaceGun(slotIndex);
            }
            //WeaponSlot에 획득한 아이템의 정보를 입력한다.
            _info.ApplyWeaponInSlot(slotIndex, gun);

            

            DisableItemByViewID(gun.viewID);

            if (_info.IsGunSlot(_curSlot) == true)
            {
                int loadedBullet = _info._loadedBullet[_curSlot];
                int havingBullet = _info._bulletSlot[_info.CastIntBulletType(slotIndex)].itemCount;
                
                _info.HUD.RefreshSlotBullet(loadedBullet, havingBullet);
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
        //장전되어있는 총알을 제거하고, 현재 총을 버리고, 해당하는 HUD를 Refresh해준다.
        int preBulletType = _info.CastIntBulletType(slotIndex);
        _info.BulletSlotAddValue(preBulletType, _info._loadedBullet[slotIndex]);
        _info._loadedBullet[slotIndex] = 0;
        ItemManager.DitchItem(_info.GetWeaponInSlot(slotIndex), transform.position, -CalcAttackDir());

        AutoReloading();
    }

    private void Acquire(WeaponSub sub)
    {
        //기존에 들고 있는 근접무기를 (주먹이 아니라면) 버려야한다.
        if (_info.IsBasicSubWeapon() == false)
        {
            ItemManager.DitchItem(_info.GetWeaponInSlot(ConstNums.subWeaponIndex), transform.position, -CalcAttackDir());
        }
        //WeaponSlot에 획득한 아이템의 정보를 입력한다.
        _info.ApplyWeaponInSlot(ConstNums.subWeaponIndex, sub);

        if (_info.IsSubSlot(_curSlot) == true)
        {
            photonView.RPC("RPC_ChangeAnim", RpcTarget.AllBuffered, sub.itemName);
        }
        DisableItemByViewID(sub.viewID);
    }

    private void Acquire(WeaponThrow thr)
    {
        Debug.LogWarning("ThrowWeapon : Acquire(thr)");
        _info.ApplyThrowWeaponData(thr);
        DisableItemByViewID(thr.viewID);
    }

    private void Acquire(ItemRecovery rec)
    {
        //RecoverSlot에 획득한 아이템의 정보를 입력한다.
        _info.AddRecoveryItem(rec);
        DisableItemByViewID(rec.viewID);
    }

    private void Acquire(ItemDefensive def)
    {
        // 현재 방어구 슬롯이 비었거나, 획득하는 아이템이 더 좋은 아이템이라면 획득
        if (_info.CanAcquireDefensive(def))
        {
            //DefensiveSlot에 획득한 아이템의 정보를 입력한다.
            int slotIndex = _info.CastIntDefensiveType(def);
            ItemManager.DitchItem(_info._defSlot[slotIndex], transform.position, -CalcAttackDir());

            _info.ApplyDefensive(def);
            DisableItemByViewID(def.viewID);
        }
        else
        {
            Debug.Log("Your equipment worn is better");
        }
    }

    private void Acquire(ItemScope scope)
    {
        // 갖고 있지 않은 Scope라면 획득
        if (_info.CanAcquireScope(scope) == true)
        {
            //스코프 획득 _ 비트연산을 통해 _info.scope에 값을 넣어준다.
            _info._scope += scope.lensPower;
            if (_info.IsBetterScope(scope))
            {
                _info.ChangeScope(scope.lensPower);
            }
            DisableItemByViewID(scope.viewID);
            //처음 먹었기때문에 Create를 해줘야한다.
            _info.HUD.CreateSlotScope(scope.lensPower, _info._curEyesight - _info._basicEyesight);
        }
    }

    private void Acquire(ItemAmmo ammo)
    {
        int bulletType = _info.CastIntAmmoType(ammo);

        _info.ApplyAmmoData(bulletType, ammo);
        DisableItemByViewID(ammo.viewID);
        if (_curSlot < ConstNums.subWeaponIndex)
        {
            //현재 총을 들고 있다면 HUD 아래슬롯에도 같이 적용해야될 수도 있기 때문에 확인해준다.
            HUDUpdate_slotBullet();
            AutoReloading();
        }
    }
    // ~ Item Acquire ()
}
