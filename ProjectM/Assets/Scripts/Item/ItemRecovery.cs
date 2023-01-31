using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRecovery : Item
{
    public RecoverType recoverType;
    public float recoverValue;
    public float usingLatency;

    public override void ParseData()
    {
        base.ParseData();
        recoverValue = itemData.recoverValue;
        usingLatency = itemData.usingLatency;
        recoverType = itemData.recoverType; 
    }

    public override void DeepCopy(Item item)
    {
        base.DeepCopy(item);

        ItemRecovery itemRecovery = null;

        if (item is ItemRecovery)
        {
            itemRecovery = item as ItemRecovery;

            recoverType = itemRecovery.recoverType;
            recoverValue = itemRecovery.recoverValue;
            usingLatency = itemRecovery.usingLatency;
        }
        else
        {
            Debug.LogError($"Item {item.name} type for deepCopy is incorrect.");
        }
    }
}
