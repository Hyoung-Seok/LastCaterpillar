using UnityEngine;

public class LingeringProjectile : EnemyProjectile
{
    [Header("Lingering Config")] 
    [SerializeField] private DamageZone zonePrefab;
    [SerializeField, Min(1)] private int zoneCount;
    [SerializeField] private float scatterRadius;
    
    private DamageZone _zone;
    
    public override void Throw(IAttackTarget target, Enemy self, Vector3 start, Vector3 targetPos, 
        float minDistance, float maxDistance, float dmg)
    {
        base.Throw(target, self, start, targetPos, minDistance, maxDistance, dmg);
        
        var localCenters = new Vector3[zoneCount];
        var ratio = scatterRadius / Mathf.Sqrt(zoneCount - 1); 
        
        for (var i = 0; i < zoneCount; i++)
        {
            localCenters[i] = GoldenSpiral.GetOffset(i, ratio);
        }

        targetPos.y = 0.1f;
        _zone = Instantiate(zonePrefab, targetPos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        _zone.Init(Target, localCenters, Damage);
    }

    protected override void OnLand(Vector3 curPos)
    {
        _zone.StartTick();
    }
}
