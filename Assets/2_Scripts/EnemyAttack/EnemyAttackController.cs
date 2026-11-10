using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyAttackController : MonoBehaviour
{
    /// <summary>
    /// 위치 판정 기준 공격(attacks[0]). 기즈모 표시용, 비어 있으면 null
    /// </summary>
    public EnemyAttack PrimaryAttack => attacks is { Length: > 0 } ? attacks[0] : null;

    [SerializeField] private EnemyAttack[] attacks;

    private IAttackTarget _atkTarget;
    private AttackPhase _curPhase = AttackPhase.Approach;
    private EnemyAttack _curSelectedAttack;
    private Vector3 _targetPos;
    private float _nextTransitionTime;
    private bool _isAimLocked;

    public void SetEnemyAtkTarget(IAttackTarget target)
        => _atkTarget = target;

    public bool Tick()
    {
        if(!_isAimLocked)
            _targetPos = _atkTarget.GetSurfacePoint(transform.position);
        
        switch (_curPhase)
        {
            case AttackPhase.Approach:
                // 순회해서 쓸 수 있는 공격이 없을 때
                if(!TryEnterPreheat(_targetPos, _atkTarget))
                {
                    // 위치는 맞지만 지금 쓸 수 있는 공격이 없음
                    if (IsAnyAttackInPosition(_targetPos))
                    {
                        // 쿨타임이 차는동안 대기할 수 있도록
                        _curPhase = AttackPhase.StandBy;
                    }
                }
                break;
            
            case AttackPhase.StandBy:
                // 순회해서 쓸 수 있는 공격이 없을 때
                if(!TryEnterPreheat(_targetPos, _atkTarget))
                {
                    // 모든 공격이 사거리 밖을 벗어났다면 접근
                    if (!IsAnyAttackInPosition(_targetPos))
                        _curPhase = AttackPhase.Approach;
                }
                break;
            
            case AttackPhase.Preheat:
                if (!_isAimLocked && _nextTransitionTime - _curSelectedAttack.AimLockdownTime <= Time.time)
                {
                    _isAimLocked = true;
                }
                if (_nextTransitionTime <= Time.time)
                {
                    _nextTransitionTime = Time.time + _curSelectedAttack.RecoveryTime;
                    _curSelectedAttack.Attack(_targetPos, _atkTarget);
                    
                    _curPhase = AttackPhase.Recovery;
                    _isAimLocked = false;
                }
                break;
            
            case AttackPhase.Recovery:
                if (_nextTransitionTime <= Time.time)
                {
                    _curPhase = AttackPhase.StandBy;
                }
                break;
        }

        return _curPhase == AttackPhase.Approach;
    }

    public bool TryGetLookDirection(out Vector3 lookDirection)
    {
        if (_curPhase is AttackPhase.Approach or AttackPhase.Recovery)
        {
            lookDirection = Vector3.zero;
            return false;
        }

        lookDirection = _targetPos - transform.position;
        lookDirection.y = 0;
        lookDirection.Normalize();

        return true;
    }

    private bool TryEnterPreheat(Vector3 targetPos, IAttackTarget target)
    {
        _isAimLocked = false;
        
        // 순회해서 쓸 수 있는 공격이 있는지 확인
        if (!TryFindCanAttackComponent(targetPos, target, out _curSelectedAttack))
            return false;
        
        // 있다면 예열 상태로
        _nextTransitionTime = Time.time + _curSelectedAttack.PreheatTime;
        _curPhase = AttackPhase.Preheat;
        _curSelectedAttack.OnPreheatEnter();

        return true;
    }
    
    private bool TryFindCanAttackComponent(Vector3 targetPos, IAttackTarget target, out EnemyAttack attack)
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

    private bool IsAnyAttackInPosition(Vector3 targetPos)
    {
        foreach (var atk in attacks)
        {
            if (atk.IsInAttackPosition(targetPos))
                return true;
        }

        return false;
    }

    private void OnValidate()
    {
        if (attacks == null || attacks.Length == 0)
        {
            Debug.LogError($"{this}의 attacks는 비어있을 수 없습니다.");
        }
        else
        {
            for (var i = 0; i < attacks.Length; i++)
            {
                if(attacks[i] == null)
                    Debug.LogError($"{this}의 {i}번 attack이 비어있습니다.");
            }
        }
    }
}
