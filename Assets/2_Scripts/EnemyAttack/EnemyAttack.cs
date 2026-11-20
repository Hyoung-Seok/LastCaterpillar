using UnityEngine;

[RequireComponent(typeof(Enemy))]
public abstract class EnemyAttack : MonoBehaviour
{
    public float ContactDistance => contactDistance;
    
    [Header("Attack Config")]
    [SerializeField] protected float damage;
    [SerializeField] protected float contactDistance;
    [SerializeField, Min(0)] protected float coolDown;

    [Header("Animation Config")] 
    [SerializeField] private bool hasAttackAnimation;
    // EnemyAtkCustomInspector에서 그림
    [SerializeField, HideInInspector] private string atkStateName;
    [SerializeField, HideInInspector, Min(0)] private float transitionTime;
    [SerializeField, HideInInspector, Min(0)] private float preheatTime = 0.1f;
    [SerializeField, HideInInspector, Min(0), 
     Tooltip("던지기 몇 초 전에 조준을 고정할지")] private float aimLockdownTime = 0f;
    [SerializeField, HideInInspector, Min(0)] private float recoveryTime = 0.1f;
    
    public float PreheatTime => preheatTime;
    public float AimLockdownTime => aimLockdownTime;
    public float RecoveryTime => recoveryTime;

    protected Enemy Self;

    private float _nextAttackTime;
    private int _atkStateHash;
    private bool _canPlayAtkAnim;
        
    public abstract bool CanAttack(Vector3 targetPos, IAttackTarget target);
    public abstract void Attack(Vector3 targetPos, IAttackTarget target);

    public virtual void OnPreheatEnter()
    {
        if (!_canPlayAtkAnim) return;
        
        Self.PlayAnimationClipByHashCode(_atkStateHash, transitionTime);
    }

    public virtual bool IsInAttackPosition(Vector3 targetPos)
    {
        var distance = targetPos - Self.Position;
        distance.y = 0;

        return distance.sqrMagnitude <= contactDistance * contactDistance;
    }

    /// <summary>
    /// 사거리 기즈모. 소유 적의 OnDrawGizmosSelected에서 호출 (높이를 적 쪽 기준에 맞추기 위해)
    /// </summary>
    public virtual void DrawRangeGizmo(Vector3 center)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, contactDistance);
    }

    protected bool IsCoolDownReady() => Time.time >= _nextAttackTime;
    protected void StartCoolDown() => _nextAttackTime = Time.time + coolDown;
    
    protected virtual void Awake()
    {
        Self = GetComponent<Enemy>();
        _nextAttackTime = 0f;
        
        if(hasAttackAnimation)
            _atkStateHash = Animator.StringToHash(atkStateName);
    }

    protected virtual void Start()
    {
        if (hasAttackAnimation && !Self.HasAnimState(_atkStateHash))
        {
            Debug.LogWarning($"{gameObject.name} : {atkStateName} 상태가 Animator에 없음!", this);
            _canPlayAtkAnim = false;
        }
        else
        {
            _canPlayAtkAnim = hasAttackAnimation;
        }
    }
}
