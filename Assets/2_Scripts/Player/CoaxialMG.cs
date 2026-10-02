using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerManager))]
public class CoaxialMG : MonoBehaviour
{
    [SerializeField] private GameObject roundPrefab;
    [SerializeField] private Transform roundContainer;
    [SerializeField] private CoaxialMGData config;
    [SerializeField] private Transform firePos;

    [SerializeField, Min(1)] private int initialPoolSize = 100;
    [SerializeField, Min(1)] private int expansionSize = 20; 

    private Queue<Round> _rounds; 
    private List<Round> _firedRounds;
    private List<Enemy> _hitCandidates;
    private List<Obstacle> _obstacleBuffer;
    private InputAction _fireAction;

    private readonly struct HitPick
    {
        public readonly float Along;
        public readonly Enemy Enemy;

        public HitPick(float along, Enemy enemy)
        {
            Along = along;
            Enemy = enemy;
        }
    }

    private HitPick[] _picks;
    private int _pickCount = 0;
    
    private float _nextFireTime;
    private bool _isOverheated;
    private float _heat = 0f;
    private EnemyRegister _enemyRegister;
    private FlowField _flowField;

    // UI 표시용 (0~1)
    public float Heat => _heat;
    public bool IsOverheated => _isOverheated;

    private void Start()
    {
        _enemyRegister = EnemyRegister.Instance;
        _flowField = FlowField.Instance;
        
        _rounds = new Queue<Round>();
        _firedRounds = new List<Round>();
        _hitCandidates = new List<Enemy>();
        _obstacleBuffer = new List<Obstacle>();
        _picks = new HitPick[config.PenetrationCount];
        
        _nextFireTime = Time.time + config.FireInterval;
        _isOverheated = false;
        _heat = 0f;
        _fireAction = GetComponent<PlayerManager>().InputReader.PlayerMgFire;
        
        CreateRound(initialPoolSize);
    }

    private void Update()
    {
        CoolDown();
        
        if (_fireAction.IsPressed())
        {
            FireCoaxialMg();
        }
        
        var dt = Time.deltaTime;
        var step = config.Velocity * dt;
        
        for (var i = _firedRounds.Count - 1; i >= 0; --i)
        {
            var r = _firedRounds[i];
            var prevPos = r.CurPosition;

            r.CurPosition += r.Direction * step;
            r.CurLifeTime += dt;
            r.Bullet.transform.position = r.CurPosition;

            var roundEnd = ProcessHits(r, prevPos, step) || _flowField.IsBlocked(r.CurPosition)
                                                         || r.CurLifeTime >= config.LifeTime;

            if (!roundEnd)
            {
                continue;
            }
            
            r.Bullet.SetActive(false);
            _rounds.Enqueue(r);

            _firedRounds[i] = _firedRounds[^1];
            _firedRounds.RemoveAt(_firedRounds.Count - 1);
        }
    }
    
    private void AddHeat()
    {
        _heat += config.HeatPerShot;

        if (_heat < 1)
            return;
        
        _heat = 1;
        _isOverheated = true;
    }

    private void CoolDown()
    {
        _heat = Mathf.Max(0, _heat - config.CoolingRate * Time.deltaTime);

        if (_isOverheated && _heat <= config.OverHeatRelease)
            _isOverheated = false;
    }

    // 1. step : 총알의 이동 거리 (config.Velocity * Time.deltaTime)
    private bool ProcessHits(Round r, Vector3 prev, float step)
    {
        prev.y = 0;
        
        _pickCount = 0;
        var budget = r.HitEnemies.Length - r.HitCount;
        var blockAlong = float.PositiveInfinity;

        var half = step * 0.5f;
        // N-1 과 N프레임의 총알 위치의 중앙 지점 계산
        var mid = prev + r.Direction * half;
        // 총알이 이동한 거리를 감싸는 반지름 범위 계산
        var radius = half + config.HitRadius;
            
        _enemyRegister.QueryForRadius(mid, radius, _hitCandidates);
        _enemyRegister.QueryObstacleForRadius(mid, half + _enemyRegister.MaxObsBodyRadius, _obstacleBuffer);

        foreach (var obs in _obstacleBuffer)
        {
            if (IsInCapsule(prev, obs.Position, r.Direction, step, obs.BodyRadius,
                    out var along))
            {
                blockAlong = Mathf.Min(blockAlong, along);
            }
        }
        
        foreach (var e in _hitCandidates)
        {
            if(e.IsDead) continue;
            
            // 캡슐 안에 적이 없다면 건너뜀
            if(!IsInCapsule(prev, e.Position, r.Direction, step, config.HitRadius, 
                   out var along))
                continue;
            
            // 장애물 뒤에 적이 있는 경우에는 건너뜀
            if(along > blockAlong)
                continue;
                
            // 이미 명중한 적이라면 스킵
            if(AlreadyHit(r, e))
                continue;

            TryPickNearest(along, e, budget);
        }

        for (var i = 0; i < _pickCount; ++i)
        {
            _picks[i].Enemy.TakeDamage(config.Damage);
            r.HitEnemies[r.HitCount++] = _picks[i].Enemy;
        }

        return r.HitCount >= r.HitEnemies.Length || blockAlong < float.PositiveInfinity;
    }

    private void FireCoaxialMg()
    {
        if (!IsCanFire()) return;

        if (_rounds.Count <= 0)
        {
            CreateRound(expansionSize);   
        }
        
        var r = _rounds.Dequeue();
        
        var dir = firePos.forward;
        dir.y = 0;
        r.Direction = dir.normalized;
        
        r.CurPosition = firePos.position;
        r.CurLifeTime = 0;

        if (r.HitEnemies == null || r.HitEnemies.Length != config.PenetrationCount)
        {
            r.HitEnemies = new Enemy[config.PenetrationCount];
        }

        if (_picks == null || _picks.Length != config.PenetrationCount)
        {
            _picks = new HitPick[config.PenetrationCount];
        }
        r.HitCount = 0;
        
        r.Bullet.transform.SetPositionAndRotation(firePos.position, Quaternion.LookRotation(r.Direction));
        r.Bullet.SetActive(true);
        
        _firedRounds.Add(r);
        _nextFireTime = Time.time + config.FireInterval;
        AddHeat();
    }

    private static bool IsInCapsule(Vector3 prev, Vector3 targetPos, Vector3 roundDir, float step, float radius, out float along)
    {
        targetPos.y = 0;
            
        // N-1 총알 위치에서 객체로 향하는 벡터
        var toTarget = targetPos - prev;
                
        // 두 벡터를 내적. 여기서 roundDir 즉, 총알이 이동한 단위벡터임
        // |a| * |B| * cos 이고, 여기서 |b| 가 1이니까, |toTarget| * cos이 됨. 즉, toTarget을 총알 진행 방향에 투영한 실제 거리
        along = Vector3.Dot(toTarget, roundDir);
            
        // 실제 선분 안에 있는지 확인
        //prev ●----------● cur              ● target
        //      0m       5m                 8m
        // 선분 밖에 있는 객체의 along 값을 선분 위 가장 가까운 점으로 clamp
        var clamped = Mathf.Clamp(along, 0f, step);
                
        // along은 prev 위치에서 수평으로 n 미터 떨어져 있다는 것을 의미. 그래서 roundDir의 위치의 수평 성분에서 어디에 위치해있는지 구함.
        // prev ●────●──────────────→ moveDir
        //      ← 3m →
        var closest = prev + roundDir * clamped;
        var sqrDistance = (closest - targetPos).sqrMagnitude;

        return sqrDistance <= radius * radius;
    }

    private void TryPickNearest(float along, Enemy enemy, int budget)
    {
        // 이미 가득 찼는데 가장 먼 적보다 더 멀리 있으면 버림
        if (_pickCount == budget && along >= _picks[_pickCount - 1].Along)
            return;
        
        // 자리 확보. 가득 찼으면 늘리지 않는다.
        if(_pickCount < budget)
            ++_pickCount;
        
        // 뒤에서부터 밀며 제자리 찾기
        var i = _pickCount - 1;
        while (i > 0 && _picks[i - 1].Along > along)
        {
            _picks[i] = _picks[i - 1];
            --i;
        }

        _picks[i] = new HitPick(along, enemy);
    }

    private void CreateRound(int count)
    {
        for (var i = 0; i < count; ++i)
        {
            var obj = Instantiate(roundPrefab, Vector3.zero, Quaternion.identity, roundContainer);
            _rounds.Enqueue(new Round(obj));
        }
    }

    private bool AlreadyHit(Round r, Enemy e)
    {
        for(var i = 0; i < r.HitCount; ++i)
        {
            if (ReferenceEquals(r.HitEnemies[i], e))
                return true;
        }

        return false;
    }
    
    private bool IsCanFire() => Time.time >= _nextFireTime && !_isOverheated;
}

public class Round
{
    public readonly GameObject Bullet;
    public Vector3 CurPosition;
    public Vector3 Direction;
    public float CurLifeTime;
    
    public Enemy[] HitEnemies;
    public int HitCount;

    public Round(GameObject bullet)
    {
        Bullet = bullet;
        CurPosition = Vector3.zero;
        CurLifeTime = 0;
        
        bullet.SetActive(false);
    }
}
