using System.Collections.Generic;
using UnityEngine;

public class ExplosiveProjectile : EnemyProjectile
{
    [SerializeField, Min(0.1f)] private float explodeRadius = 3f;
    
    private readonly List<Enemy> _buffer = new();
    
    protected override void OnLand(Vector3 curPos)
    {
        var targetSufPos = Target.GetSurfacePoint(curPos);
        var dis = GetHorizontalDistance(curPos, targetSufPos);
        
        if(dis < explodeRadius)
            Target.TakeDamage(Damage);
        
        EnemyRegister.Instance.QueryForRadius(curPos, explodeRadius, _buffer);

        foreach (var e in _buffer)
        {
            if(ReferenceEquals(e, Self))
                continue;
            
            e.TakeDamage(Damage);
        }
    }
}
