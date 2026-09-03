using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class EnemyRegister : MonoBehaviour
{
    [SerializeField, Min(1), 
     Tooltip("모든 적의 BodyRadius 합보다 커야 함")] private int enemyHashCellSize = 1;
    [SerializeField, Min(1),
    Tooltip("모든 장애물의 influenceRadius 보다 커야함")] private int obstacleHashCellSize = 2;
    
    [SerializeField] private Transform smallObstacleParent;
    
    private List<Enemy> _enemyList;
    private List<IRepulsionReceiver> _repulsionsReceivers;
    private List<EnemyGroup> _fieldEnemyGroup;
    
    private SpatialHash<Enemy> _enemyHash;
    private SpatialHash<Obstacle> _obstacleHash;

    private List<Enemy> _enemyBuffer;
    private List<Obstacle> _obstacleBuffer;

    private List<Enemy> _pendingRemove;
    private List<EnemyGroup> _pendingGroup;

    private static EnemyRegister _instance;
    private static bool _isDestroy = false;
    
    private static readonly ProfilerMarker s_Prepare = new ("EnemyRegister.Prepare");
    private static readonly ProfilerMarker s_GroupTick = new ("EnemyRegister.GroupTick");
    private static readonly ProfilerMarker s_HashInsert = new("EnemyRegister.HashInsert");
    private static readonly ProfilerMarker s_Repulsion = new ("EnemyRegister.Repulsion");
    private static readonly ProfilerMarker s_MoveAll = new("EnemyRegister.MoveAll");

    public static EnemyRegister Instance
    {
        get
        {
            if (_isDestroy)
                return null;
            
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<EnemyRegister>();
                
                if (_instance == null)
                {
                    Debug.LogError($"{nameof(EnemyRegister)} not found.");
                    return null;
                }
            }
            
            _instance.EnsureInit();
            return _instance;
        }
    }

    public void RegisterEnemy(Enemy enemy)
    {
        _enemyList.Add(enemy);
        
        if(enemy is IRepulsionReceiver r)
            _repulsionsReceivers.Add(r);
    }

    public void UnRegisterEnemy(Enemy enemy)
    {
        _pendingRemove.Add(enemy);
    }

    public void RegisterFieldEnemyGroup(EnemyGroup group)
    {
        _fieldEnemyGroup.Add(group);
    }

    public void UnRegisterFieldEnemyGroup(EnemyGroup group)
    {
        _pendingGroup.Add(group);
    }

    public void QueryForRadius(Vector3 center, float radius, List<Enemy> buffer)
    {
        _enemyHash.QueryForRadius(center, radius, buffer);   
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("EnemyRegister 씬에 둘 이상 존재.");
            return;
        }
        
        _isDestroy = false;
        _instance = this;
        EnsureInit();
    }

    private void Update()
    {
        var dt = Time.deltaTime;
        using (s_Prepare.Auto())
        {
            _enemyHash.Clear();
            FlushPending();
        }

        using (s_GroupTick.Auto())
        {
            foreach (var group in _fieldEnemyGroup)
            {
                group.Tick(Time.time);
            }
        }

        using (s_HashInsert.Auto())
        {
            foreach (var e in _enemyList)
            {
                _enemyHash.Insert(e);
            }
        }

        using (s_Repulsion.Auto())
        {
            foreach (var self in _repulsionsReceivers)
            {
                var selfPos = self.Position;
                
                _enemyHash.Query(selfPos, _enemyBuffer);
                _obstacleHash.Query(selfPos, _obstacleBuffer);
                
                var correction = Vector3.zero; // 이번 프레임에 내가 물러나야 할 변위
                var overlapCount = 0; // 몇 마리와 겹쳤나(나중에 평균 낼 때 사용) 
                var obsForce = Vector3.zero;

                foreach (var other in _enemyBuffer)
                {
                    if (ReferenceEquals(other, self)) continue;

                    var away = selfPos - other.Position;
                    away.y = 0f;
                    var sqrD = away.sqrMagnitude;
                    var rMin = self.BodyRadius + other.BodyRadius;

                    if(sqrD >= rMin * rMin) continue;
                    if(sqrD < 1e-8f) continue;

                    var d = Mathf.Sqrt(sqrD);
             
                    var penetration = rMin - d;
                    var share = other.Mass / (self.Mass + other.Mass);
                    correction += (away / d) * (penetration * share);
                    overlapCount++;
                }

                if (overlapCount > 0)
                    correction /= overlapCount;

                var obsCorrection = Vector3.zero;
                var obsOverlapCount = 0;

                foreach (var obs in _obstacleBuffer)
                {
                    var away = selfPos - obs.Position;
                    away.y = 0;
                    var d = away.magnitude;
                    var rMin = self.BodyRadius + obs.BodyRadius;

                    if (d < rMin && d > 0.0001f)
                    {
                        obsCorrection += (away / d) * (rMin - d);
                        obsOverlapCount++;
                    }

                    if (TryCalculateSteering(away, d, obs, out var f))
                    {
                        obsForce += f;
                    }
                }

                if (obsOverlapCount > 0)
                    obsCorrection /= obsOverlapCount;

                correction += obsCorrection;
                self.ApplyRepulsion(correction, obsForce);
            }
        }

        using (s_MoveAll.Auto())
        {
            foreach (var e in _enemyList)
            {
                if (!e.IsDead)
                    e.Move(dt);
            }
        }

        FlushPending();
    }

    private bool TryCalculateSteering(Vector3 away, float dist, ISteeringSource item,
        out Vector3 force)
    {
        if (dist < 0.0001f || dist >= item.InfluenceRadius)
        {
            force = Vector3.zero;
            return false;
        }
        
        force = (away / dist) * (1 - dist / item.InfluenceRadius);
        return true;
    }

    private void FlushPending()
    {
        foreach (var e in _pendingRemove)
        {
            _enemyList.Remove(e);
            if(e is IRepulsionReceiver r) _repulsionsReceivers.Remove(r);
        }

        foreach (var g in _pendingGroup)
        {
            _fieldEnemyGroup.Remove(g);
        }
        
        _pendingRemove.Clear();
        _pendingGroup.Clear();
    }
    
    private void EnsureInit()
    {
        if (_enemyList != null) return;
        
        _enemyList = new List<Enemy>();
        _repulsionsReceivers = new List<IRepulsionReceiver>();
        _fieldEnemyGroup = new List<EnemyGroup>();
        
        _pendingRemove = new List<Enemy>();
        _pendingGroup = new List<EnemyGroup>();
        
        _enemyHash = new SpatialHash<Enemy>(enemyHashCellSize);
        _obstacleHash = new SpatialHash<Obstacle>(obstacleHashCellSize);

        _enemyBuffer = new List<Enemy>();
        _obstacleBuffer = new List<Obstacle>();

        if (smallObstacleParent != null)
        {
            for (var i = 0; i < smallObstacleParent.childCount; ++i)
            {
                if (smallObstacleParent.GetChild(i).TryGetComponent(out Obstacle obstacle))
                {
                    _obstacleHash.Insert(obstacle);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            _isDestroy = true;
        }
    }
}
