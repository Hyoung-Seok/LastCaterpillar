using System;
using System.Collections.Generic;
using UnityEngine;

public class HE : Shell
{
    private List<DummyEnemy> _rangeBuffer;

    private void Awake()
    {
        _rangeBuffer = new List<DummyEnemy>();
    }

    public override void OnHit()
    {
        WaveManager.Instance.EnemyHash.QueryForRadius(transform.position, _shellData.BlastRadius, _rangeBuffer);

        if (_rangeBuffer.Count <= 0) return;

        foreach (var e in _rangeBuffer)
        {
            e.Dead();
        }
    }
}
