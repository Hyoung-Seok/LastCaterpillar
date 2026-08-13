using UnityEngine;

public class EnemyGroup
{
    public EGroupState State { get; private set; }

    private readonly GroupConfig _groupConfig;
    private float _transitionTime;
    private float _nextTransitionAt;
    private EGroupState _prevState;
    private Vector3 _moveDir;

    public EnemyGroup(GroupConfig groupConfig, float time)
    {
        _groupConfig = groupConfig;
        
        State = Random.value < 0.5f ? EGroupState.Idle : EGroupState.Move;
        EnterState(time);
    }
    
    public void Tick(float now)
    {
        if (State == EGroupState.Chase)
            return;

        if (now < _nextTransitionAt) return;

        _prevState = State;
        State = State == EGroupState.Idle ? EGroupState.Move : EGroupState.Idle;
        
        EnterState(now);
    }

    public (Vector3 dir, float speedScale) GetMoveFor(float delay, float now)
    {
        var s = (now >= _transitionTime + delay) ? State : _prevState;
        return s == EGroupState.Idle ? (Vector3.zero, 0f) : (_moveDir, _groupConfig.MoveSpeedScale);
    }

    public float GetRelayDelay(int index, int count)
    {
        return count > 1 ? Mathf.Lerp(_groupConfig.MinDelayTime, _groupConfig.MaxDelayTime, (float)index / (count - 1))
                : 0f;
    }

    private void PickMoveDir()
    {
        var angle = Random.Range(0f, Mathf.PI * 2f);
        _moveDir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
    }

    private void EnterState(float now)
    {
        var rnd = State == EGroupState.Idle ? Random.Range(_groupConfig.IdleDurationMinTime, _groupConfig.IdleDurationMaxTime) : 
            Random.Range(_groupConfig.MoveDurationMinTime, _groupConfig.MoveDurationMaxTime);
        
        if(State ==  EGroupState.Move)
            PickMoveDir();
        
        _transitionTime = now;
        _nextTransitionAt = _transitionTime + rnd;
    }
    
}
