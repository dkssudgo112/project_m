using UnityEngine;

public interface IDamageable
{
    void TakeDamage(object[] damageInfo, float damageToPlayer, float damageToObject, Vector2 attackPoint, bool isAttacker);
}
