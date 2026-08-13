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
    
    protected override (Vector3 dir, float speedScale) GetDesiredMove(Vector3 pos, FlowField f)
    {
        if(_enemyGroup.State == EGroupState.Chase)
            return base.GetDesiredMove(pos, f);

        return _enemyGroup.GetMoveFor(_delayTime, Time.time);
    }
}
