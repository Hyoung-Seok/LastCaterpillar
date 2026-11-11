using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Config")]
    [SerializeField, Min(0.1f)] private float flightMinTime;
    [SerializeField, Min(0.1f)] private float flightMaxTime;
    [SerializeField, Min(0.1f)] private float arcMinHeight;
    [SerializeField, Min(0.1f)] private float arcMaxHeight;

    protected IAttackTarget Target;
    protected Enemy Self;
    protected float Damage;
    
    private float _elapsedTime;
    private float _flightDuration;
    private float _arcHeight;
    private Vector3 _startPos;
    private Vector3 _endPos;

    protected abstract void OnLand(Vector3 curPos);
    
    public void Throw(IAttackTarget target, Enemy self, Vector3 start, Vector3 targetPos, 
        float minDistance, float maxDistance, float dmg)
    {
        Target = target;
        Self = self;
        Damage = dmg;
        
        _startPos = start;
        _endPos = targetPos;
        _endPos.y = self.transform.position.y;
        
        var distance = GetHorizontalDistance(_startPos, _endPos);
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
        var pos = Vector3.Lerp(_startPos, _endPos, t);
        var offsetY = 4f * _arcHeight * t * (1f - t);

        pos.y += offsetY;
        transform.position = pos;

        if (t >= 1 || FlowField.Instance.IsBlocked(transform.position))
        {
            OnLand(pos);
            Destroy(gameObject);
        }
    }
    
    protected float GetHorizontalDistance(Vector3 start, Vector3 end)
    {
        var v = start - end;
        v.y = 0;
        
        return v.magnitude;
    }
}
