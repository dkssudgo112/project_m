using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public UIManager HUD;

    public WeaponSlot[] _slotWeapon = new WeaponSlot[ConstNums.numberOfWeapon];
    
    public ItemDefensive[] _defSlot = new ItemDefensive[ConstNums.numberOfItemDefensive] { null, null, null }; //[0] Helmet [1] Armor [2] Bag
    public ItemRecovery[] _recoverySlot = new ItemRecovery[ConstNums.numberOfItemRecovery] { null, null, null, null }; // [0] 붕대, [1] 구급상자, [2] 소다, [3] 알약
    public ItemAmmo[] _bulletSlot = new ItemAmmo[ConstNums.numberOfItemAmmo] { null, null, null }; // [0] ammoRed, [1] ammoGreen, [2] ammoBlue

    [SerializeField]
    private WeaponSub _fist = null;

    private float _maxHP = 1000;
    private float _currentHP = 1000;
    private float _maxSP = 100;
    private float _currentSP = 0;
    private float _defensive = 0;
    private int _basicSpeed = 300;
    private int _eventSpeed = 0;
    private int _curWeaponThrowIndex = noneThrowIndex; // 0 : 수류탄, 1 : 연막탄
    private int[] _loadedBullet = new int[slotNumLoadedBullet] { 0, 0 };
    private readonly int[,] _maxWeaponThrow = new int[numberOfBag, ConstNums.numberOfWeaponThrow] { { 3, 3 }, { 6, 6 }, { 9, 9 }, { 12, 12 } }; // [_bag][수류탄]
    private readonly int[,] _maxHavingBullet = new int[numberOfBag, ConstNums.numberOfItemAmmo] { { 15, 90, 90 }, { 30, 180, 180 }, { 60, 240, 240 }, { 90, 300, 300 } }; // [_bag][ammo]
    private readonly int[,] _maxRecoverItem = new int[numberOfBag, ConstNums.numberOfItemRecovery] { { 5, 1, 2, 1 }, { 10, 2, 5, 2 }, { 15, 3, 10, 3 }, { 30, 4, 15, 4 } }; // [_bag][]
    private int _scope = 1;
    private int _curEyesight = 10;
    private int _basicEyesight = 10;

    #region NotiMessage
    private string _equipAlreadyHaveMessage = "아이템을 이미 보유하고 있습니다.";
    private string _equipWornBetterMessage = "이미 장비한 장비가 같거나 더 좋습니다.";
    private string _fullInvenMessage = "충분한 공간이 없습니다!";
    private string _cantReloadingMessage = "탄창에 총알이 가득합니다.";
    private string _cantRecoveryMessage = "더 이상 회복할 체력이 없습니다.";
    #endregion

    private bool _useStamina = false;

    #region ConstNumGroup
    private const int slotNumLoadedBullet = 2;
    private const int numberOfBag = 4;
    private const int increaseSpeedBySP = 50;
    private const int noneThrowIndex = -1;
    private const float decreaseSPOnEvent = 0.4f;
    private const float increaseHPBySP = 10.0f;
    private const float timeIntervalSP = 0.5f;
    #endregion

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

    #region Get()Method
    public float GetMaxHP()
    {
        return _maxHP;
    }

    public float GetCurrentHP()
    {
        return _currentHP;
    }

    public float GetMaxSP()
    {
        return _maxSP;
    }

    public float GetCurrentSP()
    {
        return _currentSP;
    }

    public float GetDefensive()
    {
        return _defensive;
    }

    public float GetPlayerSpeed()
    {
        return _basicSpeed + _eventSpeed;
    }

    public Weapon GetWeaponInSlot(int slotIndex)
    {
        return _slotWeapon[slotIndex].GetWeapon();
    }

    public WeaponGun GetWeaponGunInSlot(int slotIndex)
    {
        return _slotWeapon[slotIndex].GetWeapon().GetComponent<WeaponGun>();
    }

    public int GetCurWeaponThrowIndex()
    {
        return _curWeaponThrowIndex;
    }

    public WeaponSub GetFist()
    {
        return _fist;
    }

    public int GetHavingBullet(int bulletType)
    {
        if (_bulletSlot[bulletType] == null)
        {
            return 0;
        }
        return _bulletSlot[bulletType].itemData.itemCount;
    }

    public float GetHelmetDefensive()
    {
        int slotIndex = (int)DefensiveType.HELMET;
        if (_defSlot[slotIndex] == null)
            return 0;
        else
            return _defSlot[slotIndex].itemData.defensive;
    }

    public float GetArmorDefensive()
    {
        int slotIndex = (int)DefensiveType.ARMOR;
        if (_defSlot[slotIndex] == null)
            return 0;
        else
            return _defSlot[slotIndex].itemData.defensive;
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
        return _defSlot[slotIndex].itemData.level;
    }

    public int GetCurEyeSight()
    {
        return _curEyesight;
    }

    public int GetBasicEyeSight()
    {
        return _basicEyesight;
    }

    public string GetEquipAlreadyHaveMessage()
    {
        return _equipAlreadyHaveMessage;
    }

    public string GetEquipWornBetterMessage()
    {
        return _equipWornBetterMessage;
    }

    public string GetFullInvenMessage()
    {
        return _fullInvenMessage;
    }

    public string GetCantReloadingMessage()
    {
        return _cantReloadingMessage;
    }

    public string GetCantRecoveryMessage()
    {
        return _cantRecoveryMessage;
    }

    public int GetLoadedBullet(int slotIndex)
    {
        if (slotIndex > slotNumLoadedBullet)
        {
            return 0;
        }

        return _loadedBullet[slotIndex];
    }
    #endregion

    #region Set()Method
    public void SetLoadedBullet(int slotIndex, int value)
    {
        _loadedBullet[slotIndex] = value;
    }

    public void SetWeaponInSlot(int slotIndex, Weapon weapon)
    {
        _slotWeapon[slotIndex].SetWeapon(weapon);
    }
    #endregion

    public void ClearWeaponInSlot(int slotIndex)
    {
        _slotWeapon[slotIndex].ClearWeapon();
    }

    #region Is()Method
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

    public bool IsBasicSubWeapon()
    {
        if (GetWeaponInSlot(ConstNums.subWeaponIndex) == _fist)
        {
            return true;
        }
        return false;
    }
    public bool IsBetterScope(ItemScope scope)
    {
        if (_curEyesight < _basicEyesight + scope.itemData.lensPower)
        {
            return true;
        }
        return false;
    }

    public bool IsMaxHP()
    {
        if (_currentHP < _maxHP)
        {
            return false;
        }
        return true;
    }
    #endregion

    #region IsNull()Method
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

    public bool IsNullWeaponThrowSlot()
    {
        for (int i = 0; i < ConstNums.numberOfWeaponThrow; i++)
        {
            if (_slotWeapon[ConstNums.weaponThrowIndex + i].GetWeapon() != null)
            {
                if (_slotWeapon[ConstNums.weaponThrowIndex + i].GetWeapon().itemData.itemCount != 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool IsNullOtherWeaponThrow(int slotIndex)
    {
        int throwIndex = slotIndex;
        throwIndex++;
        if (throwIndex == ConstNums.numberOfWeapon)
        {
            throwIndex = ConstNums.weaponThrowIndex;
        }

        if (_slotWeapon[throwIndex].GetWeapon() == null || _slotWeapon[throwIndex].GetItemCount() == 0)
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

    public bool IsNullRecoverySlot(int slotIndex)
    {
        if (_recoverySlot[slotIndex] == null)
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

    #region AddValue()Method
    public void AddEventSpeedValue(int value)
    {
        _eventSpeed += value;
    }

    public void AddScopeValue(int value)
    {
        _scope += value;
    }

    public void AddLoadedBulletValue(int slotIndex, int value)
    {
        _loadedBullet[slotIndex] += value;
    }

    public void AddCurrentHPValue(float value)
    {
        _currentHP += value;
        if (_currentHP > _maxHP)
        {
            _currentHP = _maxHP;
        }
        HUD.RefreshHPBar();
    }

    public void AddCurrentSPValue(float value)
    {
        _currentSP += value;

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

    public void AddBulletSlotValue(int bulletType, int bulletCount)
    {
        if (IsNullBulletSlot(bulletType) == true)
        {
            Debug.Log($"{_bulletSlot[bulletType]} is null");
            return;
        }

        _bulletSlot[bulletType].itemData.itemCount += bulletCount;

        if (_bulletSlot[bulletType].itemData.itemCount > _maxHavingBullet[GetBagLevel(), bulletType])
        {
            int dropCount = _bulletSlot[bulletType].itemData.itemCount - _maxHavingBullet[GetBagLevel(), bulletType];

            DropItemManager.DitchItem(_bulletSlot[bulletType], dropCount, transform.position, -HUD.GetDitchItemDirection());

            HUD.CreateInformText(GetFullInvenMessage());
        }

        //총알 정보 변경에 따른 HUD 반영
        HUD.RefreshSlotAmmo(bulletType, _bulletSlot[bulletType].itemData.itemCount);
    }
    #endregion

    #region ApplyItemDataInSlot()Method
    public void ApplyWeaponDataInSlot(int slotIndex, Weapon weapon)
    {
        _slotWeapon[slotIndex].SetWeapon(weapon);
        HUD.RefreshSlotWeapon(slotIndex, weapon);
    }

    public void ApplyWeaponThrowItemDataInSlot(WeaponThrow thr)
    {
        int throwType = CastIntThrowType(thr);
        int throwIndex = ConstNums.weaponThrowIndex + throwType;

        if (_curWeaponThrowIndex < 0)
        {
            CastCurWeaponThrowIndex(throwIndex);
        }

        if (IsNullWeaponSlot(throwIndex) == true)
        {
            var obj = ItemManager.GetItemInstanceByType<WeaponThrow>(ItemType.WEAPONTHROW);
            obj.DeepCopy(thr);

            obj.gameObject.transform.SetParent(HUD.gameObject.transform);
            _slotWeapon[throwIndex].SetWeapon(obj);
        }
        else
        {
            _slotWeapon[throwIndex].AddItemCount(thr.itemData.itemCount);

            if (_slotWeapon[throwIndex].GetItemCount() > _maxWeaponThrow[GetBagLevel(), throwType])
            {
                int dropCount = _slotWeapon[throwIndex].GetItemCount() - _maxWeaponThrow[GetBagLevel(), throwType];

                DropItemManager.DitchItem(_slotWeapon[throwIndex].GetWeapon(), dropCount, transform.position, -HUD.GetDitchItemDirection());

                HUD.CreateInformText(GetFullInvenMessage());
            }
        }

        HUD.RefreshSlotWeaponThrow(_slotWeapon[ConstNums.weaponThrowIndex + _curWeaponThrowIndex].GetWeapon().GetComponent<WeaponThrow>());
    }

    public void ApplyDefensiveDataInSlot(ItemDefensive def)
    {
        int slotIndex = CastIntDefensiveType(def);

        if (def.itemData.defensiveType != DefensiveType.BAG)
        {
            float calcDef = 0f;
            if (def.itemData.defensiveType == DefensiveType.HELMET)
            {
                calcDef = GetArmorDefensive() + def.itemData.defensive;
            }
            else if (def.itemData.defensiveType == DefensiveType.ARMOR)
            {
                calcDef = GetHelmetDefensive() + def.itemData.defensive;
            }
            _defensive = calcDef;
        }
        _defSlot[slotIndex] = def;

        HUD.RefreshSlotDef(slotIndex, _defSlot[slotIndex]);
    }

    //bulletType에 맞는 Slot에 들어오는 Value값을 더해준다.
    public void ApplyAmmoDataInSlot(int bulletType, ItemAmmo ammo)
    {
        if (IsNullBulletSlot(bulletType) == true)
        {
            var obj = ItemManager.GetItemInstanceByType<ItemAmmo>(ItemType.AMMO);
            obj.DeepCopy(ammo);

            obj.gameObject.transform.SetParent(HUD.gameObject.transform);
            _bulletSlot[bulletType] = obj;
        }
        else
        {
            AddBulletSlotValue(bulletType, ammo.itemData.itemCount);
        }

        HUD.RefreshSlotAmmo(bulletType, _bulletSlot[bulletType].itemData.itemCount);
    }

    public void ApplyRecoveryItemDataInSlot(ItemRecovery rec)
    {
        int slotIndex = CastIntRecoverType(rec);
        if (IsNullRecoverySlot(slotIndex) == true)
        {
            var obj = ItemManager.GetItemInstanceByType<ItemRecovery>(ItemType.RECOVERY);
            obj.DeepCopy(rec);

            obj.gameObject.transform.SetParent(HUD.gameObject.transform);
            _recoverySlot[slotIndex] = obj;
        }
        else
        {
            _recoverySlot[slotIndex].itemData.itemCount += rec.itemData.itemCount;

            if (_recoverySlot[slotIndex].itemData.itemCount > _maxRecoverItem[GetBagLevel(), slotIndex])
            {
                // 소지 개수 한도 초과 시 아이템 뱉기 
                int dropCount = _recoverySlot[slotIndex].itemData.itemCount - _maxRecoverItem[GetBagLevel(), slotIndex];

                DropItemManager.DitchItem(_recoverySlot[slotIndex], dropCount, transform.position, -HUD.GetDitchItemDirection());

                HUD.CreateInformText(GetFullInvenMessage());
            }
        }

        HUD.RefreshSlotRecovery(_recoverySlot[slotIndex]);
    }
    #endregion

    //시야 변환 함수. 들어오는 값에 따라 늘리거나 줄여준다.
    public void ChangeScope(int lensPower)
    {
        int eyesight = _basicEyesight + lensPower;
        Camera.main.orthographicSize = eyesight;
        _curEyesight = eyesight;
        HUD.RefreshSlotScope(_curEyesight);
    }

    #region UseItem()Method
    public void UseWeaponThrow(int slotIndex)
    {
        _slotWeapon[slotIndex].UseItemCount();
        if (_slotWeapon[slotIndex].GetItemCount() == 0)
        {
            if (IsNullOtherWeaponThrow(slotIndex) == true)
            {
                _curWeaponThrowIndex = noneThrowIndex;
            }
        }
        HUD.RefreshSlotWeaponThrow(_slotWeapon[slotIndex].GetWeapon().GetComponent<WeaponThrow>());
    }

    public void UseRecoveryItem(int slotIndex)
    {
        RecoverType recoveryType = _recoverySlot[slotIndex].itemData.recoverType;

        _recoverySlot[slotIndex].itemData.itemCount--;
        if (_recoverySlot[slotIndex].itemData.itemCount < 0)
        {
            _recoverySlot[slotIndex].itemData.itemCount = 0;
        }

        if (recoveryType == RecoverType.BAND || recoveryType == RecoverType.KIT)
        {
            AddCurrentHPValue(_recoverySlot[slotIndex].itemData.recoverValue);
        }
        else if (recoveryType == RecoverType.SODA || recoveryType == RecoverType.DRUG)
        {
            AddCurrentSPValue(_recoverySlot[slotIndex].itemData.recoverValue);
        }

        HUD.RefreshSlotRecovery(_recoverySlot[slotIndex]);
        HUD.PlayActionViewOff();
    }
    #endregion

    public void DecreseCurrentSPValue(float value)
    {
        _currentSP -= value;
        if (_currentSP < 0)
        {
            _currentSP = 0;
            _useStamina = false;
            _eventSpeed -= increaseSpeedBySP;
            StopCoroutine(StaminaEvent());
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
            DecreseCurrentSPValue(decreaseSPOnEvent);
            AddCurrentHPValue(increaseHPBySP);
            yield return new WaitForSeconds(timeIntervalSP);
        }
    }
    // ~ Recovery()

    public void CastCurWeaponThrowIndex(int slotIndex)
    {
        _curWeaponThrowIndex = slotIndex - ConstNums.weaponThrowIndex;
    }
    public void ClearCurWeaponThrowIndex()
    {
        _curWeaponThrowIndex = noneThrowIndex;
    }

    #region CanAcquire()Method
    public bool CanAcquireItemDefensive(ItemDefensive def)
    {
        int slotIndex = CastIntDefensiveType(def);
        if (IsNullDefensiveSlot(slotIndex) == true)
        {
            return true;
        }
        if (_defSlot[slotIndex].itemData.level < def.itemData.level)
        {
            return true;
        }
        return false;
    }

    public bool CanAcquireItemScope(ItemScope scope)
    {
        //비트연산을 통해 현재 획득하려는 스코프를 보유하고 있는지 확인한다.
        int lensPower = scope.itemData.lensPower & _scope;

        if (lensPower == scope.itemData.lensPower)
        {
            return false;
        }
        return true;
    }
    #endregion

    public int ChangeThrowIndex(int slotIndex)
    {
        int throwIndex = slotIndex;
        throwIndex++;
        if (throwIndex == ConstNums.numberOfWeapon)
        {
            throwIndex = ConstNums.weaponThrowIndex;
        }
        _curWeaponThrowIndex = throwIndex - ConstNums.weaponThrowIndex;

        HUD.RefreshSlotWeaponThrow(_slotWeapon[throwIndex].GetWeapon().GetComponent<WeaponThrow>());

        return throwIndex;
    }

    #region CastIntType()Method
    public int CastIntBulletType(int curSlot)
    {
        return (int)GetWeaponInSlot(curSlot).GetComponent<WeaponGun>().weaponData.bulletType;
    }

    public int CastIntRecoverType(ItemRecovery recovery)
    {
        return (int)recovery.itemData.recoverType;
    }

    public int CastIntRecoverType(RecoverType recoverType)
    {
        return (int)recoverType;
    }

    public int CastIntDefensiveType(ItemDefensive defensive)
    {
        return (int)defensive.itemData.defensiveType;
    }

    public int CastIntAmmoType(ItemAmmo ammo)
    {
        return (int)ammo.itemData.bulletType;
    }

    public int CastIntThrowType(WeaponThrow thr)
    {
        return (int)thr.weaponData.throwType;
    }
    #endregion
}
