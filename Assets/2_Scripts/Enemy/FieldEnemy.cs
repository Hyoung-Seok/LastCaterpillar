using UnityEngine;

public class FieldEnemy : SwarmEnemy
{
    private EnemyGroup _enemyGroup;
    private float _delayTime;
    
    private readonly int _isMoveAnim = Animator.StringToHash("IsMove");

    public void Init(EnemyGroup enemyGroup, float delayTime)
    {
        _enemyGroup = enemyGroup;
        _delayTime = delayTime;
    }

    public override void OnHeardNoise(Vector3 pos, float now)
    {
        if (IsDead) return;
        
        _enemyGroup.OnHeardNoise(now);
    }

    protected override (Vector3 dir, float speedScale) GetDesiredMove(Vector3 pos, FlowField f)
    {
        _enemyGroup.ReportPosition(pos);
        
        var result = (Vector3.zero, 0f);
        var isMove = false;

        switch (_enemyGroup.EffectiveStateFor(_delayTime, Time.time))
        {
            case EGroupState.Idle:
                break;
            
            case  EGroupState.Move:
                isMove = true;
                result = (_enemyGroup.MoveDir, _enemyGroup.MoveSpeedScale);
                break;
            
            case EGroupState.Chase:
                isMove = true;
                result = base.GetDesiredMove(pos, f);
                break;
        }
        
        animator.SetBool(_isMoveAnim, isMove);
        return result;
    }
}
