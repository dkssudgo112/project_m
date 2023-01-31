using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButton : MonoBehaviour
{
    private ItemType _itemType;
    private int _slotIndex = 0;

    public void EnterInitialValue(ItemType itemType, int slotIndex)
    {
        _itemType = itemType;  
        _slotIndex = slotIndex;
    }

    public ItemType GetItemType()
    {
        return _itemType;
    }

    public int GetSlotIndex()
    {
        return _slotIndex;
    }
}
