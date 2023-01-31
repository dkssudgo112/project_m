using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Item : MonoBehaviourPunCallbacks
{
    public ItemData itemData;

    public override void OnDisable()
    {
        base.OnDisable();   

        ObjectPoolManager.FreeObjectToPool(gameObject);
        CancelInvoke();
    }

    public virtual void DeepCopy(Item item)
    {
        ItemData itemData = new ItemData();
        itemData.DeepCopy(item.itemData);
        this.itemData = itemData;

        this.gameObject.name = item.itemData.itemName;
        this.GetComponent<SpriteRenderer>().sprite = item.GetComponent<SpriteRenderer>().sprite;
    }
}
