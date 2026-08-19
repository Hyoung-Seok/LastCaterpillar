using System.Collections.Generic;
using UnityEngine;

public class NoiseSystem
{
    public static NoiseSystem Instance { get; } = new();
    
    private Queue<(Vector3, float)> _pending;
    private List<Enemy> _buffer;
    private bool _isEmitting = false;
    
    private NoiseSystem()
    {
        _buffer = new List<Enemy>();
        _pending = new Queue<(Vector3, float)>();
        _isEmitting = false;
    }

    public void Emit(Vector3 pos, float radius)
    {
        if (_isEmitting)
        {
            _pending.Enqueue((pos, radius));
            return;
        }
        
        _isEmitting = true;
        try
        {
            Dispatch((pos, radius));

            while (_pending.Count > 0)
                Dispatch(_pending.Dequeue());
        }
        finally
        {
            _isEmitting = false;
        }
    }

    private void Dispatch((Vector3 pos, float radius) item)
    {
        EnemyRegister.Instance.QueryForRadius(item.pos, item.radius, _buffer);
        foreach (var e in _buffer)
        {
            e.OnHeardNoise(item.pos, Time.time);
        }
    }
}
