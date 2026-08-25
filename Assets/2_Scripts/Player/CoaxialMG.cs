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
    private InputAction _fireAction;
    
    private float _nextFireTime;
    private EnemyRegister _enemyRegister;
        
    private void Start()
    {
        _enemyRegister = EnemyRegister.Instance;
        
        _rounds = new Queue<Round>();
        _firedRounds = new List<Round>();
        _hitCandidates = new List<Enemy>();
        
        _nextFireTime = Time.time + config.FireInterval;
        _fireAction = GetComponent<PlayerManager>().InputReader.PlayerMgFire;
        
        CreateRound(initialPoolSize);
    }

    private void Update()
    {
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

            var roundEnd = ProcessHits(r, prevPos, step) || r.CurLifeTime >= config.LifeTime;

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

    // 1. step : 총알의 이동 거리 (config.Velocity * Time.deltaTime)
    private bool ProcessHits(Round r, Vector3 prev, float step)
    {
        prev.y = 0;

        var half = step * 0.5f;
        // N-1 과 N프레임의 총알 위치의 중앙 지점 계싼
        var mid = prev + r.Direction * half;
        // 총알이 이동한 거리를 감싸는 반지름 범위 계산
        var radius = half + config.HitRadius;
            
        _enemyRegister.QueryForRadius(mid, radius, _hitCandidates);
        
        foreach (var e in _hitCandidates)
        {
            if(e.IsDead) continue;
            
            var enemyPos = e.Position;
            enemyPos.y = 0;
            
            // 2. N-1 총알 위치에서 적으로 향하는 벡터
            var toEnemy = enemyPos - prev;
                
            // 두 벡터를 내적. 여기서 r.Direction은 총알이 이동한 단위벡터임
            // |a| * |B| * cos 이고, 여기서 |b| 가 1이니까, |toEnemy| * cos이 됨. 즉, toEnemy를 총알 진행 방향에 투영한 실제 거리
            var along = Vector3.Dot(toEnemy, r.Direction);
            
            // 실제 선분 안에 있는지 확인
            //prev ●----------● cur              ● enemy
            //      0m       5m                 8m
            // 총알이 실제 이동하지 않는 위치에 있는 적을 검사하지 않도록 제한
            along = Mathf.Clamp(along, 0f, step);
                
            // along은 prev 위치에서 수평으로 n 미터 떨어져 있다는 것을 의미. 그래서 moveDir의 위치의 수평 성분에서 어디에 위치해있는지 구함.
            // prev ●────●──────────────→ moveDir
            //      ← 3m →
            var closest = prev + r.Direction * along;
            var sqrDistance = (closest - enemyPos).sqrMagnitude;
                
            if(sqrDistance > config.HitRadius * config.HitRadius) 
                continue;
                
            // 이 적은 명중
            e.TakeDamage(config.Damage);
            r.CurPenetrationCount--;

            if (r.CurPenetrationCount <= 0)
            {
                return true;
            }
        }

        return false;
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
        r.CurPenetrationCount = config.PenetrationCount;
        
        r.Bullet.transform.SetPositionAndRotation(firePos.position, Quaternion.LookRotation(r.Direction));
        r.Bullet.SetActive(true);
        
        _firedRounds.Add(r);
        _nextFireTime = Time.time + config.FireInterval;
    }

    private void CreateRound(int count)
    {
        for (var i = 0; i < count; ++i)
        {
            var obj = Instantiate(roundPrefab, Vector3.zero, Quaternion.identity, roundContainer);
            _rounds.Enqueue(new Round(obj));
        }
    }
    
    private bool IsCanFire() => Time.time >= _nextFireTime;
}

public class Round
{
    public GameObject Bullet;
    public Vector3 CurPosition;
    public Vector3 Direction;
    public float CurLifeTime;
    public int CurPenetrationCount;

    public Round(GameObject bullet)
    {
        Bullet = bullet;
        CurPosition = Vector3.zero;
        CurLifeTime = 0;
        
        bullet.SetActive(false);
    }
}
