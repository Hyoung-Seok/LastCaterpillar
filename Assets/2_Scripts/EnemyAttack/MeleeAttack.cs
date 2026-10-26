using UnityEngine;

public class MeleeAttack : EnemyAttack
{
    [SerializeField] private string atkStateName;
    [SerializeField, Min(0)] private float transitionTime = 0;
    
    [SerializeField] private float coolDown;
    private float _nextAttackTime;

    private int _hash;

    protected override void Awake()
    {
        base.Awake();

        _nextAttackTime = 0;
        _hash = Animator.StringToHash(atkStateName);
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

    public override void OnPreheatEnter()
    {
        Self.PlayAnimationClipByHashCode(_hash, transitionTime);
    }
}
