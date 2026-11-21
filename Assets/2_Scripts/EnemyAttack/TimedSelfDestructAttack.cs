using System;
using UnityEngine;

public class TimedSelfDestructAttack : EnemyAttack, IPursuitAttack
{
    [SerializeField, Min(0.001f)] private float blastRadius = 2f;
    [SerializeField, Min(0)] private float pursuitDuration = 0;
    [SerializeField, Min(0)] private float pursuitMoveScale = 1f;
    [SerializeField] private string pursuitAnimName;
    [SerializeField, Min(0)] private float pursuitAnimTransitionTime = 0;
    
    public float PursuitDuration => pursuitDuration;
    public float PursuitMoveScale => pursuitMoveScale;
    
    private int _pursuitAnimHash;

    protected override void Awake()
    {
        base.Awake();

        _pursuitAnimHash = Animator.StringToHash(pursuitAnimName);
    }
    
    public override bool CanAttack(Vector3 targetPos, IAttackTarget target)
    {
        return IsInAttackPosition(targetPos);
    }
    
    public override void Attack(Vector3 targetPos, IAttackTarget target)
    {
        if(IsInBlastRange(targetPos))
            target.TakeDamage(damage);
        
        Self.TakeDamage(100000);
    }

    public void OnPursuitEnter()
    {
        Self.PlayAnimationClipByHashCode(_pursuitAnimHash, pursuitAnimTransitionTime);
    }

    public bool ShouldFireEarly(Vector3 targetPos)
    {
        return IsInBlastRange(targetPos);
    }

    private bool IsInBlastRange(Vector3 targetPos)
    {
        var distance = targetPos - Self.Position;
        distance.y = 0f;
        
        return distance.sqrMagnitude <= blastRadius * blastRadius;
    }
}
