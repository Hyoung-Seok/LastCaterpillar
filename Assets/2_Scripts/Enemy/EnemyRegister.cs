using System.Collections.Generic;
using UnityEngine;

public class EnemyRegister : MonoBehaviour
{
    [SerializeField] private Transform smallObstacleParent;
    
    private List<Enemy> _enemyList;
    private List<IRepulsionReceiver> _repulsionsReceivers;
    private List<EnemyGroup> _fieldEnemyGroup;
    
    private SpatialHash<Enemy> _enemyHash;
    private SpatialHash<Obstacle> _obstacleHash;

    private List<Enemy> _enemyBuffer;
    private List<Obstacle> _obstacleBuffer;

    private List<Enemy> _pendingRemove;

    private static EnemyRegister _instance;
    private static bool _isDestroy = false;

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
        _enemyHash.Clear();
        FlushPending();

        foreach (var group in _fieldEnemyGroup)
        {
            group.Tick(Time.time);
        }

        foreach (var e in _enemyList)
        {
            _enemyHash.Insert(e);
        }

        foreach (var self in _repulsionsReceivers)
        {
            _enemyHash.Query(self.Position, _enemyBuffer);
            _obstacleHash.Query(self.Position, _obstacleBuffer);

            var selfPos = self.Position;
            var correction = Vector3.zero;      // 이번 프레임에 내가 물러나야 할 변위
            var overlapCount = 0;                      // 몇 바리와 겹쳤나(나중에 평균 낼 때 사용) 
            var obsForce = Vector3.zero;
            
            foreach (var other in _enemyBuffer)
            {
                if(ReferenceEquals(other, self)) continue;

                var away = selfPos - other.Position;
                away.y = 0f;
                var d = away.magnitude;
                var rMin = self.BodyRadius + other.BodyRadius;

                if (d < rMin && d > 0.0001f)
                {
                    var penetration = rMin - d;
                    var share = other.Mass / (self.Mass + other.Mass);
                    correction += (away / d) * (penetration * share);
                    overlapCount++;
                }
            }
            
            if(overlapCount > 0)
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
                
                if(TryCalculateRepulsion(selfPos, obs, out var f))
                {
                    obsForce += f;
                }
            }

            if (obsOverlapCount > 0)
                obsCorrection /= obsOverlapCount;

            correction += obsCorrection;
            self.ApplyRepulsion(correction, obsForce);
        }

        foreach (var e in _enemyList)
        {
            if(!e.IsDead)
                e.Move(Time.deltaTime);
        }
        
        FlushPending();
    }

    private bool TryCalculateRepulsion(Vector3 selfPos, ISteeringSource item, out Vector3 force)
    {
        var away = selfPos - item.Position;
        away.y = 0f;
        var dist = away.magnitude;

        if (dist < 0.0001f || dist >= item.InfluenceRadius)
        {
            force = Vector3.zero;
            return false;
        }
        
        force = away.normalized * (1 - dist / item.InfluenceRadius);
        return true;
    }

    private void FlushPending()
    {
        foreach (var e in _pendingRemove)
        {
            _enemyList.Remove(e);
            if(e is IRepulsionReceiver r) _repulsionsReceivers.Remove(r);
        }
        
        _pendingRemove.Clear();
    }
    
    private void EnsureInit()
    {
        if (_enemyList != null) return;
        
        _enemyList = new List<Enemy>();
        _repulsionsReceivers = new List<IRepulsionReceiver>();
        _fieldEnemyGroup = new List<EnemyGroup>();
        
        _pendingRemove = new List<Enemy>();

        // CellSize를 매직넘버로 넘기는게 맞나?
        _enemyHash = new SpatialHash<Enemy>(4);
        _obstacleHash = new SpatialHash<Obstacle>(4);

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
