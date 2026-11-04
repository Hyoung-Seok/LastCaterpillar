using UnityEngine;

public class SelfDestructAttack : EnemyAttack
{
    public override bool CanAttack(Vector3 targetPos, IDamageable target)
    {
        return IsInAttackPosition(targetPos);
    }

    public override void Attack(Vector3 targetPos, IDamageable target)
    {
        target.TakeDamage(damage);
        Self.TakeDamage(float.MaxValue);
    }
}
