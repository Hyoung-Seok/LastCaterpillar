using System.Collections.Generic;
using UnityEngine;

public class LingeringProjectile : EnemyProjectile
{
    [Header("Components")]
    [SerializeField] private Renderer projectileRenderer;
    
    [Header("Lingering Config")] 
    [SerializeField] private DamageZone zonePrefab;
    [SerializeField, Min(1)] private int zoneCount;
    [SerializeField, Range(0f, 0.95f)] private float separationPoint;
    [SerializeField] private float scatterRadius;
    [SerializeField] private float heightOffset;
    
    [Header("Fragment Config")]
    [SerializeField] private Transform fragmentPrefab;
    [SerializeField, Min(0.01f)] private float fragmentArcHeight = 2f;
    
    private DamageZone _zone;
    private Transform[] _fragments;
    private bool _canSeparate;
    private bool _isSeparated;
    
    private float _remainingTime;
    private float _curTime;
    private List<Vector3> _localCenters;
    private Vector3 _burstPos;
    private Vector3 _zonePos;
    
    public override void Throw(IAttackTarget target, Enemy self, Vector3 start, Vector3 targetPos, 
        float minDistance, float maxDistance, float dmg)
    {
        base.Throw(target, self, start, targetPos, minDistance, maxDistance, dmg);
        
        _localCenters = CreateDamageZoneLocalCenter(targetPos);
        _canSeparate = _localCenters.Count > 1;
        _isSeparated = false;
        
        if(_canSeparate)
            _fragments = new Transform[_localCenters.Count];
        
        _zonePos = EndPos + Vector3.up * heightOffset;
        _zone = Instantiate(zonePrefab, _zonePos, Quaternion.identity);
        _zone.Init(Target, _localCenters, Damage, FlightDuration);
    }

    protected override void Update()
    {
        if (_canSeparate && FlightProgress >= separationPoint)
        {
            if (!_isSeparated)
            {
                _isSeparated = true;
                EnableSeparateProjectile();
            }
        
            UpdateSeparateProjectile();
        }
        
        base.Update();
    }

    protected override void OnLand(Vector3 curPos)
    {
        if (_isSeparated)
        {
            foreach (var obj in _fragments)
            {
                Destroy(obj.gameObject);
            }
        
            _fragments = null;
        }

        _zone.Land();
    }

    private List<Vector3> CreateDamageZoneLocalCenter(Vector3 targetPos)
    {
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

        return localCenters;
    }

    private void EnableSeparateProjectile()
    {
        projectileRenderer.enabled = false;
        
        _remainingTime = RemainingTime;
        _curTime = 0;
        _burstPos = transform.position;
        
        for (var i = 0; i < _localCenters.Count; i++)
        {
            var obj = Instantiate(fragmentPrefab, transform.position, Quaternion.identity);
            _fragments[i] = obj;
        }
    }

    private void UpdateSeparateProjectile()
    {
        _curTime += Time.deltaTime;
        var t = Mathf.Clamp01(_curTime / _remainingTime);

        for (var i = 0; i < _fragments.Length; i++)
        {
            var pos = EvaluateArc(_burstPos, _localCenters[i] + _zonePos, t, fragmentArcHeight);
            _fragments[i].position = pos;
        }
    }
}
