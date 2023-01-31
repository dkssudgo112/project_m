using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public float _maxHP = 1000;
    public float _currentHP = 1000;
    public float _maxSP = 100;
    public float _currentSP = 0;
    public float _defensive = 0;
    public int _basicSpeed = 500;
    public int _eventSpeed = 0;
    public WeaponSlot[] _slotWeapon = new WeaponSlot[ConstNums.numberOfWeapon];
    public int _curThrowWeapon = 1000;
    public WeaponSub _fist = null;
    public ItemDefensive[] _defSlot = new ItemDefensive[ConstNums.numberOfItemDefensive] { null, null, null }; //[0] Helmet [1] Armor [2] Bag
    public readonly int[,] _maxThrowingWeapon = new int[numberOfBag, ConstNums.numberOfWeaponThrow] { { 3, 3 }, { 6, 6 }, { 9, 9 }, { 12, 12 } }; // [_bag][수류탄]
    public ItemAmmo[] _bulletSlot = new ItemAmmo[ConstNums.numberOfItemAmmo] { null, null, null }; // [0] ammoRed, [1] ammoGreen, [2] ammoBlue
    public readonly int[,] _maxHavingBullet = new int[numberOfBag, ConstNums.numberOfItemAmmo] { { 15, 90, 90 }, { 30, 180, 180 }, { 60, 240, 240 }, { 90, 300, 300 } }; // [_bag][ammo]
    public ItemRecovery[] _recoverSlot = new ItemRecovery[ConstNums.numberOfItemRecovery] { null, null, null, null }; // [0] 붕대, [1] 구급상자, [2] 소다, [3] 알약
    public readonly int[,] _maxRecoverItem = new int[numberOfBag, ConstNums.numberOfItemRecovery] { { 5, 1, 2, 1 }, { 10, 2, 5, 2 }, { 15, 3, 10, 3 }, { 30, 4, 15, 4 } }; // [_bag][]
    public int[] _loadedBullet = new int[2] { 0, 0 };
    public int _scope = 1;
    public int _curEyesight = 10;
    public UIManager HUD;
    public int _basicEyesight = 10;

    private bool _useStamina = false;

    private const int numberOfBag = 4;
    private const int increaseSpeedBySP = 50;
    private const float decreaseSPOnEvent = 0.4f;
    private const float increaseHPBySP = 10.0f;
    private const float timeIntervalSP = 0.5f;

    public void Start()
    {
        for (int i = 0; i < _slotWeapon.Length; i++)
        {
            if (_slotWeapon[i] == null)
            {
                _slotWeapon[i] = new WeaponSlot();
            }
        }

        _slotWeapon[ConstNums.subWeaponIndex].SetWeapon(_fist);
    }

    public int GetHavingBullet(int bulletType)
    {
        if (_bulletSlot[bulletType] == null)
        {
            return 0;
        }
        return _bulletSlot[bulletType].itemCount;
    }

    public void ApplyThrowWeaponData(WeaponThrow thr)
    {
        int throwType = CastIntThrowType(thr);
        int throwIndex = ConstNums.throwWeaponIndex + throwType;

        if(_curThrowWeapon > ConstNums.numberOfWeaponThrow)
        {
            _curThrowWeapon = throwType;
        }

        if (IsNullWeaponSlot(throwIndex) == true)
        {
            _slotWeapon[throwIndex].SetWeapon(thr);
        }
        else
        {
            _slotWeapon[throwIndex].AddItemCount(thr.itemCount);
            if (_slotWeapon[throwIndex].GetItemCount() > _maxThrowingWeapon[GetBagLevel(), throwType])
            {
                _slotWeapon[throwIndex].SetItemCount(_maxThrowingWeapon[GetBagLevel(), throwType]);
            }
        }

        HUD.RefreshSlotThrowWeapon(_slotWeapon[ConstNums.throwWeaponIndex + _curThrowWeapon].GetWeapon());
    }

    //bulletType에 맞는 Slot에 들어오는 Value값을 더해준다.
    public void ApplyAmmoData(int bulletType, ItemAmmo ammo)
    {
        if (IsNullBulletSlot(bulletType) == true)
        {
            var obj = ItemManager.GetInstanceByType<ItemAmmo>(ItemType.AMMO);
            obj.name = ammo.name;
            obj.gameObject.transform.SetParent(HUD.gameObject.transform);
            obj.DeepCopy(ammo);
            _bulletSlot[bulletType] = obj;
        }
        else
        {
            BulletSlotAddValue(bulletType, ammo.itemCount);
        }

        HUD.RefreshSlotAmmo(bulletType, _bulletSlot[bulletType].itemCount);
    }

    //시야 변환 함수. 들어오는 값에 따라 늘리거나 줄여준다.
    public void ChangeScope(int lensPower)
    {
        int eyesight = _basicEyesight + lensPower;
        Camera.main.orthographicSize = eyesight;
        _curEyesight = eyesight;
        HUD.RefreshSlotScope(_curEyesight);
    }

    public void BulletSlotAddValue(int bulletType, int bulletCount)
    {
        if (IsNullBulletSlot(bulletType) == true)
        {
            Debug.Log($"{_bulletSlot[bulletType]} is null");
            return;
        }

        _bulletSlot[bulletType].itemCount += bulletCount;

        if (_bulletSlot[bulletType].itemCount > _maxHavingBullet[GetBagLevel(), bulletType])
        {
            Debug.LogError("bullet Count 전달 함수 필요");
            ItemManager.DitchItem(_bulletSlot[bulletType], transform.position, -HUD.GetDitchItemDirection());

            _bulletSlot[bulletType].itemCount = _maxHavingBullet[GetBagLevel(), bulletType];
        }
        
        //총알 정보 변경에 따른 HUD 반영
        HUD.RefreshSlotAmmo(bulletType, _bulletSlot[bulletType].itemCount);
    }

    public void AddRecoveryItem(ItemRecovery rec)
    {
        int slotIndex = CastIntRecoverType(rec);
        if (IsNullRecoverSlot(slotIndex) == true)
        {
            var obj = ItemManager.GetInstanceByType<ItemRecovery>(ItemType.RECOVERY);
            obj.name = rec.name;
            obj.gameObject.transform.SetParent(HUD.gameObject.transform);
            obj.DeepCopy(rec);
            _recoverSlot[slotIndex] = obj;
        }
        else
        {
            _recoverSlot[slotIndex].itemCount += rec.itemCount;

            if (_recoverSlot[slotIndex].itemCount > _maxRecoverItem[GetBagLevel(), slotIndex])
            {
                // 소지 개수 한도 초과 시 아이템 뱉기 
                int dropCount = _recoverSlot[slotIndex].itemCount - _maxRecoverItem[GetBagLevel(), slotIndex];
                _recoverSlot[slotIndex].itemCount = _maxRecoverItem[GetBagLevel(), slotIndex];

                ItemManager.DitchItem(rec, dropCount, transform.position, -HUD.GetDitchItemDirection());
            }
        }

        HUD.RefreshSlotRecovery(_recoverSlot[slotIndex]);
    }

    public void UsingRecoveryItem(int slotIndex)
    {
        RecoverType recoverType = _recoverSlot[slotIndex].recoverType;
        if (recoverType == RecoverType.BAND || recoverType == RecoverType.KIT)
        {
            RecoverHP(_recoverSlot[slotIndex]);
        }
        else if (recoverType == RecoverType.SODA || recoverType == RecoverType.DRUG)
        {
            RecoverSP(_recoverSlot[slotIndex]);
        }

        HUD.RefreshSlotRecovery(_recoverSlot[slotIndex]);
        HUD.SetActiveRecoveringView(false);
    }

    public void AddWeaponThrow(WeaponThrow thr)
    {
        int slotIndex = CastIntThrowType(thr);
    }

    // Recovery() ~
    public void AddValueForHP(float value)
    {
        _currentHP += value;
        if (_currentHP > _maxHP)
        {
            _currentHP = _maxHP;
        }
        HUD.RefreshHPBar();
    }

    public void RecoverHP(ItemRecovery recovery)
    {
        _recoverSlot[CastIntRecoverType(recovery)].itemCount--;
        _currentHP += recovery.recoverValue;
        if (_currentHP > _maxHP)
        {
            _currentHP = _maxHP;
        }
        HUD.RefreshHPBar();
    }

    public void UseSP(float recoverValue)
    {
        _currentSP -= recoverValue;
        if (_currentSP < 0)
        {
            _currentSP = 0;
            _useStamina = false;
            _eventSpeed -= increaseSpeedBySP;
            StopCoroutine(StaminaEvent());
        }
        HUD.RefreshSPBar();
    }

    public void RecoverSP(ItemRecovery recovery)
    {
        _recoverSlot[CastIntRecoverType(recovery)].itemCount--;
        _currentSP += recovery.recoverValue;
        if (_currentSP > _maxSP)
        {
            _currentSP = _maxSP;
        }
        if (_useStamina == false)
        {
            _useStamina = true;
            _eventSpeed += increaseSpeedBySP;
            StartCoroutine(StaminaEvent());
        }
        HUD.RefreshSPBar();
    }

    private IEnumerator StaminaEvent()
    {
        for (; ; )
        {
            if (_useStamina == false)
            {
                yield break;
            }
            UseSP(decreaseSPOnEvent);
            AddValueForHP(increaseHPBySP);
            yield return new WaitForSeconds(timeIntervalSP);
        }
    }
    // ~ Recovery()

    public float GetHelmetDefensive()
    {
        int slotIndex = (int)DefensiveType.HELMET;
        if (_defSlot[slotIndex] == null)
            return 0;
        else
            return _defSlot[slotIndex].defensive;
    }

    public float GetArmorDefensive()
    {
        int slotIndex = (int)DefensiveType.ARMOR;
        if (_defSlot[slotIndex] == null)
            return 0;
        else
            return _defSlot[slotIndex].defensive;
    }

    public int GetNullWeaponGunIndex()
    {
        for (int i = 0; i < ConstNums.numberOfWeaponGun; i++)
        {
            if (_slotWeapon[i].GetWeapon() == null)
            {
                return i;
            }
        }

        return ConstNums.numberOfWeaponGun;
    }

    public int GetBagLevel()
    {
        int slotIndex = (int)DefensiveType.BAG;
        if (_defSlot[slotIndex] == null)
            return 0;
        return _defSlot[slotIndex].level;
    }

    public Weapon GetWeaponInSlot(int slotIndex)
    {
        return _slotWeapon[slotIndex].GetWeapon();
    }

    public WeaponGun GetWeaponGunInSlot(int slotIndex)
    {
        return _slotWeapon[slotIndex].GetWeapon().GetComponent<WeaponGun>();
    }

    public void ApplyWeaponInSlot(int slotIndex, Weapon weapon)
    {
        _slotWeapon[slotIndex].SetWeapon(weapon);
        HUD.RefreshSlotWeapon(slotIndex, weapon);
    }

    public void UseThrowWeapon(int slotIndex)
    {
        _slotWeapon[slotIndex].UseItemCount();
        if (_slotWeapon[slotIndex].GetItemCount() == 0)
        {
            _slotWeapon[slotIndex].ClearWeapon();
            if(IsNullOtherThrowWeapon(slotIndex) == true)
            {
                _curThrowWeapon = 1000;
            }
        }
        HUD.RefreshSlotThrowWeapon(_slotWeapon[slotIndex].GetWeapon());
    }

    public bool IsGunSlot(int slotIndex)
    {
        if (slotIndex < ConstNums.subWeaponIndex)
        {
            return true;
        }
        return false;
    }

    public bool IsSubSlot(int slotIndex)
    {
        if (slotIndex == ConstNums.subWeaponIndex)
        {
            return true;
        }
        return false;
    }

    public bool IsThrowSlot(int slotIndex)
    {
        if(slotIndex >= ConstNums.throwWeaponIndex)
        {
            return true;
        }
        return false;
    }

    public bool IsBasicSubWeapon()
    {
        if (GetWeaponInSlot(ConstNums.subWeaponIndex) == _fist)
        {
            return true;
        }
        return false;
    }

    public bool CanAcquireDefensive(ItemDefensive def)
    {
        int slotIndex = CastIntDefensiveType(def);
        if (IsNullDefensiveSlot(slotIndex) == true)
        {
            return true;
        }
        if (_defSlot[slotIndex].level < def.level)
        {
            return true;
        }
        return false;
    }

    public bool CanAcquireScope(ItemScope scope)
    {
        //비트연산을 통해 현재 획득하려는 스코프를 보유하고 있는지 확인한다.
        int lensPower = scope.lensPower & _scope;

        if (lensPower == scope.lensPower)
        {
            return false;
        }
        return true;
    }

    public void ApplyDefensive(ItemDefensive def)
    {
        int slotIndex = CastIntDefensiveType(def);
        
        if (def.defensiveType != DefensiveType.BAG)
        {
            float calcDef = 0f;
            if (def.defensiveType == DefensiveType.HELMET)
            {
                calcDef = GetArmorDefensive() + def.defensive;
            }
            else if (def.defensiveType == DefensiveType.ARMOR)
            {
                calcDef = GetHelmetDefensive() + def.defensive;
            }
            _defensive = calcDef;
        }
        _defSlot[slotIndex] = def;

        HUD.RefreshSlotDef(slotIndex, _defSlot[slotIndex]);
    }

    public int ChangeThrowIndex(int slotIndex)
    {
        int throwIndex = slotIndex;
        throwIndex++;
        if (throwIndex == ConstNums.numberOfWeapon)
        {
            throwIndex = ConstNums.throwWeaponIndex;
        }
        _curThrowWeapon = throwIndex - ConstNums.throwWeaponIndex;


        HUD.RefreshSlotThrowWeapon(_slotWeapon[throwIndex].GetWeapon());

        return throwIndex;
    }

    public bool IsBetterScope(ItemScope scope)
    {
        if (_curEyesight < _basicEyesight + scope.lensPower)
        {
            return true;
        }
        return false;
    }

    public bool IsMaxHP()
    {
        if(_currentHP < _maxHP)
        {
            return false;
        }
        return true;
    }

    #region IsNullMethod()
    public bool IsNullWeaponGunSlot()
    {
        for (int i = 0; i < ConstNums.numberOfWeaponGun; i++)
        {
            if (_slotWeapon[i].GetWeapon() == null)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsNullWeaponSlot(int slotIndex)
    {
        if (_slotWeapon[slotIndex].GetWeapon() == null)
        {
            return true;
        }
        return false;
    }

    public bool IsNullThrowWeaponSlot()
    {
        for(int i = 0; i < ConstNums.numberOfWeaponThrow; i++)
        {
            if (_slotWeapon[ConstNums.throwWeaponIndex + i].GetWeapon() != null)
            {
                if (_slotWeapon[ConstNums.throwWeaponIndex + i].GetWeapon().itemCount != 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool IsNullOtherThrowWeapon(int slotIndex)
    {
        int throwIndex = slotIndex;
        throwIndex++;
        if(throwIndex == ConstNums.numberOfWeapon)
        {
            throwIndex = ConstNums.throwWeaponIndex;
        }

        if (_slotWeapon[throwIndex].GetWeapon() == null)
        {
            return true;
        }
        return false;
    }

    public bool IsNullBulletSlot(int bulletType)
    {
        if (_bulletSlot[bulletType] == null)
        {
            return true;
        }
        return false;
    }

    public bool IsNullRecoverSlot(int slotIndex)
    {
        if (_recoverSlot[slotIndex] == null)
        {
            return true;
        }
        return false;
    }

    public bool IsNullDefensiveSlot(int slotIndex)
    {
        if (_defSlot[slotIndex] == null)
        {
            return true;
        }
        return false;
    }
    #endregion

    #region CastIntType
    public int CastIntBulletType(int curSlot)
    {
        return (int)GetWeaponInSlot(curSlot).GetComponent<WeaponGun>().bulletType;
    }

    public int CastIntRecoverType(ItemRecovery recovery)
    {
        return (int)recovery.recoverType;
    }

    public int CastIntRecoverType(RecoverType recoverType)
    {
        return (int)recoverType;
    }

    public int CastIntDefensiveType(ItemDefensive defensive)
    {
        return (int)defensive.defensiveType;
    }

    public int CastIntAmmoType(ItemAmmo ammo)
    {
        return (int)ammo.bulletType;
    }

    public int CastIntThrowType(WeaponThrow thr)
    {
        return (int)thr.throwType;
    }
    #endregion
}
