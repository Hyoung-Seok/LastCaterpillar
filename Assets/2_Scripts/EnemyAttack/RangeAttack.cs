using System;
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
        obj.Throw(handTf.position, targetPos, minAttackDistance, contactDistance);
        
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

    private void OnValidate()
    {
        if (minAttackDistance > contactDistance)
            minAttackDistance = contactDistance;
    }
}
