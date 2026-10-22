using UnityEngine;

[RequireComponent(typeof(Enemy))]
public abstract class EnemyAttack : MonoBehaviour
{
    [SerializeField] protected float damage;
    [SerializeField] protected float contactDistance;
    
    [Header("Attack phase transition Time")] 
    [SerializeField, Tooltip("공격 모션 시작 -> 공격 판정까지 걸리는 시간"), Min(0f)] private float preheatTime = 0.1f;
    [SerializeField, Tooltip("공격 판정 -> 다음 행동까지 걸리는 시간"), Min(0f)] private float recoveryTime = 0.1f;
    
    public float PreheatTime => preheatTime;
    public float RecoveryTime => recoveryTime;

    protected Enemy Self;
    public abstract bool CanAttack(Vector3 targetPos, IDamageable target);
    public abstract void Attack(Vector3 targetPos, IDamageable target);

    public bool IsInContactDistance(Vector3 targetPos)
    {
        var distance = targetPos - Self.Position;
        distance.y = 0;

        return distance.sqrMagnitude <= contactDistance * contactDistance;
    }
    
    protected virtual void Awake()
    {
        Self = GetComponent<Enemy>();
    }
}
