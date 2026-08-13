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
            var sep = Vector3.zero;
            var obsForce = Vector3.zero;
            
            foreach (var other in _enemyBuffer)
            {
                if(ReferenceEquals(other, self)) continue;

                if (TryCalculateRepulsion(selfPos, other, out var f))
                {
                    var w = RepulsionWeight(self.Mass, other.Mass);
                    sep += f * w;
                }
            }

            foreach (var obs in _obstacleBuffer)
            {
                if(TryCalculateRepulsion(selfPos, obs, out var f))
                {
                    obsForce += f;
                }
            }
            
            self.ApplyRepulsion(sep, obsForce);
        }

        foreach (var e in _enemyList)
        {
            if(!e.IsDead)
                e.Move(Time.deltaTime);
        }
        
        FlushPending();
    }

    private bool TryCalculateRepulsion(Vector3 selfPos, IRepulsionSource item, out Vector3 force)
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

    private float RepulsionWeight(float selfMass, float otherMass) => 2 * otherMass / (selfMass + otherMass);

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
