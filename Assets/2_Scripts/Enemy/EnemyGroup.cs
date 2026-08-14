using UnityEngine;

public class EnemyGroup
{
    public EGroupState State { get; private set; }
    public void ReportPosition(Vector3 pos) => _lastKnownPos = pos;

    private readonly GroupConfig _groupConfig;
    
    private readonly Vector3 _homeCenter;
    private Vector3 _lastKnownPos;
    
    private float _transitionTime;
    private float _nextTransitionAt;
    private EGroupState _prevState;
    private Vector3 _moveDir;

    public EnemyGroup(GroupConfig groupConfig, Vector3 center, float now)
    {
        _groupConfig = groupConfig;
        _homeCenter = center;
        _lastKnownPos = _homeCenter;
        
        State = Random.value < 0.5f ? EGroupState.Idle : EGroupState.Move;
        EnterState(now);
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
        var randDir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        
        var toSpawn = _homeCenter -  _lastKnownPos;
        toSpawn.y = 0f;
        var d = toSpawn.magnitude;

        var returnChance = Mathf.InverseLerp(_groupConfig.BiasStartDistance, _groupConfig.BiasFullDistance, d);
        
        if (Random.value < returnChance)
        {
            var centerDir = toSpawn.normalized;
            var randomAngle = Random.Range(-60f, 60f);
            _moveDir = Quaternion.Euler(0f, randomAngle, 0f) * centerDir;
        }
        else
        {
            _moveDir = randDir;
        }
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
