using UnityEngine;

[RequireComponent(typeof(Enemy))]
public abstract class EnemyAttack : MonoBehaviour
{
    [SerializeField] protected float damage;
    [SerializeField] protected float contactDistance;

    [Header("Animation Config")] 
    [SerializeField] private bool hasAttackAnimation;
    // EnemyAtkCustomInspector에서 그림
    [SerializeField, HideInInspector] private string atkStateName;
    [SerializeField, HideInInspector, Min(0)] private float transitionTime;
    [SerializeField, HideInInspector, Min(0)] private float preheatTime = 0.1f;
    [SerializeField, HideInInspector, Min(0)] private float recoveryTime = 0.1f;
    
    public float PreheatTime => preheatTime;
    public float RecoveryTime => recoveryTime;

    protected Enemy Self;
    private int _atkStateHash;
        
    public abstract bool CanAttack(Vector3 targetPos, IDamageable target);
    public abstract void Attack(Vector3 targetPos, IDamageable target);

    public virtual void OnPreheatEnter()
    {
        if (!hasAttackAnimation) return;
        
        Self.PlayAnimationClipByHashCode(_atkStateHash, transitionTime);
    }

    public bool IsInContactDistance(Vector3 targetPos)
    {
        var distance = targetPos - Self.Position;
        distance.y = 0;

        return distance.sqrMagnitude <= contactDistance * contactDistance;
    }
    
    protected virtual void Awake()
    {
        Self = GetComponent<Enemy>();
        
        if(hasAttackAnimation)
            _atkStateHash = Animator.StringToHash(atkStateName);
    }
}
