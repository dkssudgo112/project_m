using UnityEngine;

public class WeaponThrow : Weapon
{
    public override void DeepCopy(Item item)
    {
        base.DeepCopy(item);
    }

    public override void Attack(Vector2 direction, Vector2 position)
    {

    }

    public int GetItemCount()
    {
        return itemData.itemCount;
    }
}
