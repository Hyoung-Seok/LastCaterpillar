using System.Collections.Generic;
using UnityEngine;

public class SpatialHash
{
    private readonly int _cellSize;
    private readonly Dictionary<long, List<DummyEnemy>> _buckets = new();

    public SpatialHash(int size)
    {
        _cellSize = size;
    }

    public void Clear()
    {
        foreach (var b in _buckets.Values)
        {
            b.Clear();
        }
    }

    public void Insert(DummyEnemy agent)
    {
        var pos = agent.transform.position;
        
        var cx = Mathf.FloorToInt(pos.x / _cellSize);
        var cy = Mathf.FloorToInt(pos.z / _cellSize);
        var key = Key(cx, cy);

        if (!_buckets.TryGetValue(key, out var list))
        {
            list = new List<DummyEnemy>();
            _buckets.Add(key, list);
        }
        
        list.Add(agent);
    }

    public void Query(Vector3 pos, List<DummyEnemy> result)
    {
        result.Clear();
        
        var cx = Mathf.FloorToInt(pos.x / _cellSize);
        var cy = Mathf.FloorToInt(pos.z / _cellSize);

        for (var dx = -1; dx <= 1; ++dx)
        {
            for (var dy = -1; dy <= 1; ++dy)
            {
                if(_buckets.TryGetValue(Key(cx + dx, cy + dy), out var list))
                    result.AddRange(list);
            }
        }
    }
    
    private long Key(int cx, int cy) => ((long)cx << 32) | (uint)cy;
}
