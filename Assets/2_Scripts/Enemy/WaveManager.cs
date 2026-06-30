using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private FlowField flowField;
    [SerializeField] private float separationRadius;
    [SerializeField] private DummyEnemy enemy;
    
    private List<DummyEnemy> _waveEnemies;
    private SpatialHash _spatialHash;
    private List<DummyEnemy> _rangeBuffer;

    public void Start()
    {
        _waveEnemies = new List<DummyEnemy>();
        _spatialHash = new SpatialHash(4);
        _rangeBuffer = new List<DummyEnemy>();

        for (var i = 0; i < 50; ++i)
        {
            var obj = Instantiate(enemy);
            
            obj.FlowField = flowField;
            obj.transform.position = transform.position + 
                                     new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            _waveEnemies.Add(obj);
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
            
            var selfPos = self.transform.position;
            var sep = Vector3.zero;
            var count = 0;

            foreach (var other in _rangeBuffer)
            {
                if(other == self) continue;
                
                var away = selfPos - other.transform.position;
                away.y = 0f;
                var dist = away.magnitude;

                if (dist > 0.0001f && dist < separationRadius)
                {
                    // 가까울수록 강하게
                    sep += away.normalized * (1 - dist / separationRadius);
                    count++;
                }
            }
            
            if(count > 0)
                sep /= count;

            self.Separation = sep;
        }
    }
}
