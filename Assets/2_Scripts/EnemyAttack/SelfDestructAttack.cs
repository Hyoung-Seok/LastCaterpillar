using UnityEngine;

public class SelfDestructAttack : EnemyAttack
{
    public override bool CanAttack(Vector3 targetPos, IDamageable target)
    {
        var distance = targetPos - Self.Position;
        distance.y = 0;

        return distance.sqrMagnitude <= contactDistance * contactDistance;
    }

    public override void Attack(IDamageable target)
    {
        target.TakeDamage(damage);
        Self.TakeDamage(float.MaxValue);
    }
}
