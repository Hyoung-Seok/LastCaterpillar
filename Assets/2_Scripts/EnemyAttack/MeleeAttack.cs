using UnityEngine;

public class MeleeAttack : EnemyAttack
{
    public override bool CanAttack(Vector3 targetPos, IDamageable target)
    {
        return IsCoolDownReady() && IsInAttackPosition(targetPos);
    }

    public override void Attack(Vector3 targetPos, IDamageable target)
    {
        if(IsInAttackPosition(targetPos))
            target.TakeDamage(damage);
        
        StartCoolDown();
    }
}
