using UnityEngine;

public class FieldEnemy : SwarmEnemy
{
    private EnemyGroup _enemyGroup;
    private float _delayTime;

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

        switch (_enemyGroup.EffectiveStateFor(_delayTime, Time.time))
        {
            case EGroupState.Idle:
                break;
            
            case  EGroupState.Move:
                result = (_enemyGroup.MoveDir, _enemyGroup.MoveSpeedScale);
                break;
            
            case EGroupState.Chase:
                result = base.GetDesiredMove(pos, f);
                break;
        }
        
        return result;
    }

    protected override void OnDead()
    {
        base.OnDead();
        _enemyGroup.OnFieldEnemyDead();
    }
}
