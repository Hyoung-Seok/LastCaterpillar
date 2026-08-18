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

        return _enemyGroup.EffectiveStateFor(_delayTime, Time.time) switch
        {
            EGroupState.Idle => (Vector3.zero, 0f),
            EGroupState.Move => (_enemyGroup.MoveDir, _enemyGroup.MoveSpeedScale),
            EGroupState.Chase => base.GetDesiredMove(pos, f)
        };
    }
}
