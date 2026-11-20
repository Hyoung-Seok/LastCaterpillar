using UnityEngine;

public class RangeAttack : EnemyAttack
{
    [Header("Range Attack Config")] 
    [SerializeField] private EnemyProjectile projectile;
    [SerializeField] private Transform handTf;
    [SerializeField, Min(0)] private float minAttackDistance;
    
    public override bool CanAttack(Vector3 targetPos, IAttackTarget target)
    {
        return IsCoolDownReady() && IsInAttackPosition(targetPos);
    }

    public override void Attack(Vector3 targetPos, IAttackTarget target)
    {
        var obj = Instantiate(projectile, handTf.position, Quaternion.identity);
        obj.Throw(target, Self, handTf.position, targetPos, minAttackDistance, contactDistance, damage);
        
        StartCoolDown();
    }

    public override bool IsInAttackPosition(Vector3 targetPos)
    {
        var distance = targetPos - Self.Position;
        distance.y = 0;
        
        var sqrDistance = distance.sqrMagnitude;
        var isInRange = sqrDistance <= contactDistance * contactDistance
                        && sqrDistance > minAttackDistance * minAttackDistance;

        return isInRange && FlowField.Instance.HasLineOfSight(Self.Position, targetPos);
    }

    public override void DrawRangeGizmo(Vector3 center)
    {
        base.DrawRangeGizmo(center);

        // 최소 사거리 (이 안에선 이 부품으로 공격 불가)
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(center, minAttackDistance);
    }

    private void OnValidate()
    {
        if (minAttackDistance > contactDistance)
            minAttackDistance = contactDistance;
    }
}
