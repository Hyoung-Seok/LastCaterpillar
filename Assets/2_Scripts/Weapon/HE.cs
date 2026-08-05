using System;
using System.Collections.Generic;
using UnityEngine;

public class HE : Shell
{
    private List<Enemy> _rangeBuffer;

    private void Awake()
    {
        _rangeBuffer = new List<Enemy>();
    }

    public override void OnHit()
    {
        EnemyRegister.Instance.EnemyHash.QueryForRadius(transform.position, _shellData.BlastRadius, _rangeBuffer);

        if (_rangeBuffer.Count <= 0) return;

        foreach (var e in _rangeBuffer)
        {
            e.TakeDamage(100);
        }
    }
}
