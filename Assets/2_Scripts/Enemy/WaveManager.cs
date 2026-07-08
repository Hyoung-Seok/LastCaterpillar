using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private FlowField flowField;
    [SerializeField] private Transform obstacleParent;
    [SerializeField] private float separationRadius;
    [SerializeField] private float obstacleRadius = 2.0f;
    [SerializeField] private DummyEnemy enemy;
    
    private List<DummyEnemy> _waveEnemies;
    private SpatialHash _spatialHash;
    private List<DummyEnemy> _rangeBuffer;
    private List<Vector3> _obstacleBuffer;

    public void Start()
    {
        _waveEnemies = new List<DummyEnemy>();
        _spatialHash = new SpatialHash(4);
        _rangeBuffer = new List<DummyEnemy>();
        _obstacleBuffer = new List<Vector3>();
        
        for (var i = 0; i < 100; ++i)
        {
            var obj = Instantiate(enemy);
            
            obj.FlowField = flowField;
            obj.transform.position = transform.position + 
                                     new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            _waveEnemies.Add(obj);
        }

        for (var i = 0; i < obstacleParent.childCount; ++i)
        {
            _spatialHash.Insert(obstacleParent.GetChild(i).position);
        }
    }

    private void Update()
    {
        _spatialHash.Clear();

        foreach (var wave in _waveEnemies)
        {
            _spatialHash.Insert(wave);
        }

        foreach (var self in _waveEnemies)
        {
            _spatialHash.Query(self.transform.position, _rangeBuffer);
            _spatialHash.Query(self.transform.position, _obstacleBuffer);
            
            var selfPos = self.transform.position;
            var sep = Vector3.zero;
            var obsForce = Vector3.zero;
            var count = 0;

            foreach (var other in _rangeBuffer)
            {
                if(other == self) continue;
                
                var away = selfPos - other.transform.position;
                away.y = 0f;
                var dist = away.magnitude;

                if (dist > 0.0001f && dist < separationRadius)
                {
                    sep += away.normalized * (1 - dist / separationRadius);
                    count++;
                }
            }

            foreach (var obs in _obstacleBuffer)
            {
                var away = selfPos - obs;
                away.y = 0f;
                var dist = away.magnitude;
                
                if(dist > 0.0001f && dist < obstacleRadius)
                    obsForce += away.normalized * (1 - dist / obstacleRadius);
            }
            
            if(count > 0)
                sep /= count;

            self.Separation = sep;
            self.ObstacleForce = obsForce;
        }
    }
}
