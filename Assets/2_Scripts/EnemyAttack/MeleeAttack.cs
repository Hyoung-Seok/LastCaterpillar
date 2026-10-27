using UnityEngine;

public class MeleeAttack : EnemyAttack
{
    [SerializeField] private float coolDown;
    
    private float _nextAttackTime;

    protected override void Awake()
    {
        base.Awake();

        _nextAttackTime = 0;
    }
    
    public override bool CanAttack(Vector3 targetPos, IDamageable target)
    {
        return Time.time >= _nextAttackTime
               && IsInContactDistance(targetPos);
    }

    public override void Attack(Vector3 targetPos, IDamageable target)
    {
        if(IsInContactDistance(targetPos))
            target.TakeDamage(damage);
        
        _nextAttackTime = Time.time + coolDown;
    }
}
