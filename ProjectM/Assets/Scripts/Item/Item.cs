using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Item : MonoBehaviourPunCallbacks
{
    [Header("Item 공통 정보")]
    public int id = 0;
    public int viewID = 0;
    public string itemName = "";
    public ItemType type = ItemType.NULL;
    public string spritePath = "";
    public string toolTip = "";
    public int stackSize = 0;
    public int itemCount = 0;

    public ItemData itemData;

    public virtual void ParseData()
    {

    }

    public override void OnDisable()
    {
        base.OnDisable();   

        ObjectPoolManager.FreeObjectToPool(gameObject);
        CancelInvoke();
    }

    public virtual void DeepCopy(Item item)
    {
        id = item.id;
        itemName = item.itemName;
        type = item.type;
        spritePath = item.spritePath;
        toolTip = item.toolTip;
        stackSize = item.stackSize;
        itemCount = item.itemCount;
    }
}
