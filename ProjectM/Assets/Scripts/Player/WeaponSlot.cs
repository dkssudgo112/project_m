using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSlot
{
    private Weapon _weapon = null;

    public void SetWeapon(Weapon weapon)
    {
        _weapon = weapon;
    }

    public void SetItemCount(int itemCount)
    {
        _weapon.itemData.itemCount = itemCount;
    }
    
    public Weapon GetWeapon()
    {
        return _weapon;
    }

    public T GetWeapon<T>()
    {
        if (_weapon.TryGetComponent(out T component))
        {
            return component;
        }
        else
        {
            Debug.LogError($"Component of weaponSlot is not found.");
        }

        return default(T);
    }

    public int GetItemCount()
    {
        return _weapon.itemData.itemCount;
    }

    public void ClearWeapon()
    {
        _weapon = null;
    }

    public void AddItemCount(int itemCount)
    {
        _weapon.itemData.itemCount += itemCount;
    }

    public void UseItemCount()
    {
        _weapon.itemData.itemCount--;
    }
}
