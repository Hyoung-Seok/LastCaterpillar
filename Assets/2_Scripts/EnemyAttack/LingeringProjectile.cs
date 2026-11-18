using System.Collections.Generic;
using UnityEngine;

public class LingeringProjectile : EnemyProjectile
{
    [Header("Lingering Config")] 
    [SerializeField] private DamageZone zonePrefab;
    [SerializeField, Min(1)] private int zoneCount;
    [SerializeField] private float scatterRadius;
    [SerializeField] private float heightOffset;
    
    private DamageZone _zone;
    
    public override void Throw(IAttackTarget target, Enemy self, Vector3 start, Vector3 targetPos, 
        float minDistance, float maxDistance, float dmg)
    {
        base.Throw(target, self, start, targetPos, minDistance, maxDistance, dmg);
        
        var localCenters = new List<Vector3>();
        var ratio = zoneCount > 1 ? scatterRadius / Mathf.Sqrt(zoneCount - 1) : 0;
        var angle = Random.Range(0f, 2 * Mathf.PI);
        
        for (var i = 0; i < zoneCount; i++)
        {
            var offset = GoldenSpiral.GetOffset(i, ratio, angle);

            if (FlowField.Instance.IsBlocked(targetPos + offset))
                continue;
            
            localCenters.Add(offset);
        }

        var zonePos = EndPos + Vector3.up * heightOffset;
        _zone = Instantiate(zonePrefab, zonePos, Quaternion.identity);
        _zone.Init(Target, localCenters, Damage);
    }

    protected override void Update()
    {
        base.Update();
        
        _zone.UpdateAlertRange(FlightProgress);
    }

    protected override void OnLand(Vector3 curPos)
    {
        _zone.StartTick();
    }
}
