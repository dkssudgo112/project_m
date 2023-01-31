using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAmmo : Item
{
    public BulletType bulletType;

    public override void ParseData()
    {
        base.ParseData();
        bulletType = itemData.bulletType;
    }

    public override void DeepCopy(Item item)
    {
        base.DeepCopy(item);

        ItemAmmo itemAmmo = null;

        if (item is ItemAmmo)
        {
            itemAmmo = item as ItemAmmo;

            bulletType = itemAmmo.bulletType;
        }
        else
        {
            Debug.LogError($"Item {item.name} type for deepCopy is incorrect.");
        }
    }
}
