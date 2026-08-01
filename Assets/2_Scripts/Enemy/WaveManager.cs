using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private FlowField flowField;
    [SerializeField] private Transform obstacleParent;
    [SerializeField] private DummyEnemy enemy;

    public static WaveManager Instance;
    public SpatialHash<DummyEnemy> EnemyHash => _enemyHash;
    
    private List<DummyEnemy> _waveEnemies;
    
    private SpatialHash<DummyEnemy> _enemyHash;
    private SpatialHash<Obstacle> _obstacleHash;
    
    private List<DummyEnemy> _rangeBuffer;
    private List<Obstacle> _obstacleBuffer;

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        _waveEnemies = new List<DummyEnemy>();
        
        _enemyHash = new SpatialHash<DummyEnemy>(4);
        _obstacleHash = new SpatialHash<Obstacle>(4);
        
        _rangeBuffer = new List<DummyEnemy>();
        _obstacleBuffer = new List<Obstacle>();

        for (var i = 0; i < 100; ++i)
        {
            var obj = Instantiate(enemy);

            obj.FlowField = flowField;
            obj.transform.position = transform.position +
                                     new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            _waveEnemies.Add(obj);
            obj.OnDead += RemoveEnemy;
        }

        for (var i = 0; i < obstacleParent.childCount; ++i)
        {
            _obstacleHash.Insert(obstacleParent.GetChild(i).GetComponent<Obstacle>());
        }
    }

    private void Update()
    {
        _enemyHash.Clear();

        foreach (var wave in _waveEnemies)
        {
            _enemyHash.Insert(wave);
        }

        foreach (var self in _waveEnemies)
        {
            _enemyHash.Query(self.transform.position, _rangeBuffer);
            _obstacleHash.Query(self.transform.position, _obstacleBuffer);
            
            var selfPos = self.transform.position;
            var sep = Vector3.zero;
            var obsForce = Vector3.zero;
            var count = 0;

            foreach (var other in _rangeBuffer)
            {
                if(other == self) continue;
                
                if (AccumulateRepulsion(selfPos, other, ref sep))
                    count++;
            }

            foreach (var obs in _obstacleBuffer)
            {
                AccumulateRepulsion(selfPos, obs, ref obsForce);
            }
            
            if(count > 0)
                sep /= count;

            self.Separation = sep;
            self.ObstacleForce = obsForce;
        }
    }

    private bool AccumulateRepulsion(Vector3 selfPos, ISpatialItem item, ref Vector3 separation)
    {
        var away = selfPos - item.Position;
        away.y = 0f;
        var dist = away.magnitude;

        if (dist > 0.0001f && dist < item.Radius)
        {
            separation += away.normalized * (1 - dist / item.Radius);
            return true;
        }

        return false;
    }

    private void RemoveEnemy(DummyEnemy e)
    {
        _waveEnemies.Remove(e);
    }
}
