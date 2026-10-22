using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyAttackController : MonoBehaviour
{
    [SerializeField] private EnemyAttack[] attacks;

    private Enemy _source;
    private AttackPhase _curPhase = AttackPhase.Approach;
    private EnemyAttack _curSelectedAttack;
    private float _nextTransitionTime;

    public void Tick(Vector3 targetPos, IDamageable target)
    {
        switch (_curPhase)
        {
            case AttackPhase.Approach:
                // 순회해서 쓸 수 있는 공격이 없을 때
                if(!TryEnterPreheat(targetPos, target))
                {
                    // 첫 공격의 사거리 안이지만, 지금 쓸 수 있는 공격이 하나도 없다
                    if (attacks[0].IsInContactDistance(targetPos))
                        _curPhase = AttackPhase.StandBy;
                }
                break;
            
            case AttackPhase.StandBy:
                // 순회해서 쓸 수 있는 공격이 없을 때
                if(!TryEnterPreheat(targetPos, target))
                {
                    // 첫 공격이 사거리 밖을 벗어났다면 접근
                    if (!attacks[0].IsInContactDistance(targetPos))
                        _curPhase = AttackPhase.Approach;
                }
                break;
            
            case AttackPhase.Preheat:
                if (_nextTransitionTime <= Time.time)
                {
                    _nextTransitionTime = Time.time + _curSelectedAttack.RecoveryTime;
                    _curSelectedAttack.Attack(targetPos, target);
                    
                    _curPhase = AttackPhase.Recovery;
                }
                break;
            
            case AttackPhase.Recovery:
                if (_nextTransitionTime <= Time.time)
                {
                    _curPhase = AttackPhase.StandBy;
                }
                break;
        }
    }

    private bool TryEnterPreheat(Vector3 targetPos, IDamageable target)
    {
        // 순회해서 쓸 수 있는 공격이 있는지 확인
        if (!TryFindCanAttackComponent(targetPos, target, out _curSelectedAttack))
            return false;
        
        // 있다면 예열 상태로
        _nextTransitionTime = Time.time + _curSelectedAttack.PreheatTime;
        _curPhase = AttackPhase.Preheat;

        return true;
    }
    
    private bool TryFindCanAttackComponent(Vector3 targetPos, IDamageable target, out EnemyAttack attack)
    {
        foreach (var atk in attacks)
        {
            if (!atk.CanAttack(targetPos, target))
                continue;

            attack = atk;
            return true;
        }

        attack = null;
        return false;
    }

    private void OnValidate()
    {
        if (attacks == null || attacks.Length == 0 || attacks[0] == null)
        {
            Debug.LogError($"{this}의 attacks는 비어있을 수 없습니다.");
        }
    }
}
