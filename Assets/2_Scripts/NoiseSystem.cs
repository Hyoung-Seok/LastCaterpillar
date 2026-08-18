using System.Collections.Generic;
using UnityEngine;

public class NoiseSystem
{
    public static NoiseSystem Instance { get; } = new();
    private List<Enemy> _buffer;
    
    private NoiseSystem()
    {
        _buffer = new List<Enemy>();
    }

    public void Emit(Vector3 pos, float radius)
    {
        EnemyRegister.Instance.QueryForRadius(pos, radius, _buffer);
        
        foreach (var e in _buffer)
        {
            e.OnHeardNoise(pos, Time.time);
        }
    }
}
