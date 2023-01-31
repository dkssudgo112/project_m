using UnityEngine;

public class Weapon : Item
{
    public WeaponData weaponData;

    public override void DeepCopy(Item item)
    {
        base.DeepCopy(item);

        Weapon weapon = null;

        if (item is Weapon)
        {
            weapon = item as Weapon;

            WeaponData weaopnData = new WeaponData();
            weaopnData.DeepCopy(weapon.weaponData);
            this.weaponData = weaopnData;
        }
        else
        {
            Debug.LogError($"Item {item.name} type for deepCopy is incorrect.");
        }
    }

    public virtual void Attack(Vector2 direction, Vector2 position)
    {
    }
}
