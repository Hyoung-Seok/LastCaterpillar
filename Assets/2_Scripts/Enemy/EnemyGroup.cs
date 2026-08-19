using UnityEngine;

public class EnemyGroup
{
    public EGroupState State { get; private set; }
    public EGroupState EffectiveStateFor(float delay, float now) 
        => now >= _transitionTime + delay ? State : _prevState;
    public void ReportPosition(Vector3 pos) => _lastKnownPos = pos;
    public Vector3 MoveDir { get; private set; }
    public float MoveSpeedScale => _groupConfig.MoveSpeedScale;

    private readonly GroupConfig _groupConfig;
    
    private readonly Vector3 _homeCenter;
    private Vector3 _lastKnownPos;
    private Vector3 _checkPoint;
    private float _nextProgressCheckAt;
    
    private float _transitionTime;
    private float _nextTransitionAt;
    private EGroupState _prevState;

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

        if (State == EGroupState.Move && now >= _nextProgressCheckAt)
        {
            var progress = Vector3.Dot(_lastKnownPos - _checkPoint, MoveDir);
            
            if (progress < _groupConfig.MinProgressPerCheck)
            {
                _nextTransitionAt = now;   
            }
            else
            {
                _checkPoint = _lastKnownPos;
                _nextProgressCheckAt = now + _groupConfig.ProgressCheckInterval;
            }
        }

        if (now < _nextTransitionAt) return;

        _prevState = State;
        State = State == EGroupState.Idle ? EGroupState.Move : EGroupState.Idle;
        
        EnterState(now);
    }

    public float GetRelayDelay(int index, int count)
    {
        return count > 1 ? Mathf.Lerp(_groupConfig.MinDelayTime, _groupConfig.MaxDelayTime, (float)index / (count - 1))
                : 0f;
    }

    public void OnHeardNoise(float now)
    {
        if (State == EGroupState.Chase) return;
        
        _prevState = State;
        State = EGroupState.Chase;
        
        _transitionTime = now;
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
            MoveDir = Quaternion.Euler(0f, randomAngle, 0f) * centerDir;
        }
        else
        {
            MoveDir = randDir;
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
        _checkPoint = _lastKnownPos;
        _nextProgressCheckAt = now + _groupConfig.ProgressCheckInterval;
    }
    
}
