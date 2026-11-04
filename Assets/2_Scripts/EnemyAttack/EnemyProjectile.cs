using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float explodeRadius = 3f;
    [SerializeField, Min(0.1f)] private float flightMinTime;
    [SerializeField, Min(0.1f)] private float flightMaxTime;
    [SerializeField, Min(0.1f)] private float arcMinHeight;
    [SerializeField, Min(0.1f)] private float arcMaxHeight;

    private IAttackTarget _target;
    private Enemy _self;
    private float _damage;
    private float _elapsedTime = 0;
    private float _flightDuration;
    private float _arcHeight;
    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private List<Enemy> _buffer = new List<Enemy>();

    public void Throw(IAttackTarget target, Enemy self, Vector3 start, Vector3 targetPos, 
        float minDistance, float maxDistance, float dmg)
    {
        _target = target;
        _self = self;
        _damage = dmg;
        
        _startPosition = start;
        _endPosition = targetPos;
        _endPosition.y = self.transform.position.y;
        
        var distance = GetHorizontalDistance(_startPosition, _endPosition);
        var ratio = Mathf.InverseLerp(minDistance, maxDistance, distance);
        
        _flightDuration = Mathf.Lerp(flightMinTime, flightMaxTime, ratio);
        _arcHeight = Mathf.Lerp(arcMinHeight, arcMaxHeight, ratio);

        _elapsedTime = 0;
        
        transform.position = start;
    }

    private void Update()
    {
        _elapsedTime += Time.deltaTime;
        
        var t = Mathf.Clamp01(_elapsedTime / _flightDuration);
        var pos = Vector3.Lerp(_startPosition, _endPosition, t);
        var offsetY = 4f * _arcHeight * t * (1f - t);

        pos.y += offsetY;
        transform.position = pos;

        if (t >= 1 || FlowField.Instance.IsBlocked(transform.position))
            Explode();
    }
    
    private void Explode()
    {
        var curPos = transform.position;
        var targetSufPos = _target.GetSurfacePoint(curPos);
        var dis = GetHorizontalDistance(curPos, targetSufPos);
        
        if(dis < explodeRadius)
            _target.TakeDamage(_damage);
        
        EnemyRegister.Instance.QueryForRadius(curPos, explodeRadius, _buffer);

        foreach (var e in _buffer)
        {
            if(ReferenceEquals(e, _self))
                continue;
            
            e.TakeDamage(_damage);
        }
        
        Destroy(gameObject);
    }
    
    private float GetHorizontalDistance(Vector3 start, Vector3 end)
    {
        var v = start - end;
        v.y = 0;
        
        return v.magnitude;
    }
}
