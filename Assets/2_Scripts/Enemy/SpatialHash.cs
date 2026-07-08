using System.Collections.Generic;
using UnityEngine;

public class SpatialHash<T> where T : ISpatialItem
{
    private readonly int _cellSize;
    private readonly Dictionary<long, List<T>> _buckets = new();
    
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

    public void Insert(T agent)
    {
        var pos = agent.Position;
        
        var cx = Mathf.FloorToInt(pos.x / _cellSize);
        var cy = Mathf.FloorToInt(pos.z / _cellSize);
        var key = Key(cx, cy);

        if (!_buckets.TryGetValue(key, out var list))
        {
            list = new List<T>();
            _buckets.Add(key, list);
        }
        
        list.Add(agent);
    }

    public void Query(Vector3 pos, List<T> result)
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
