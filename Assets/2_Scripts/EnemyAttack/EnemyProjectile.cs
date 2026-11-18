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
    protected float FlightProgress => FlightDuration > 0f ? Mathf.Clamp01(_elapsedTime / FlightDuration) : 0f; 
    protected float FlightDuration { get; private set; }
    protected float RemainingTime => FlightDuration - _elapsedTime;
    protected Vector3 EndPos => _endPos;

    private float _elapsedTime;
    private float _arcHeight;
    private Vector3 _startPos;
    private Vector3 _endPos;

    protected abstract void OnLand(Vector3 curPos);
    
    public virtual void Throw(IAttackTarget target, Enemy self, Vector3 start, Vector3 targetPos, 
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
        
        FlightDuration = Mathf.Lerp(flightMinTime, flightMaxTime, ratio);
        _arcHeight = Mathf.Lerp(arcMinHeight, arcMaxHeight, ratio);

        _elapsedTime = 0;
        
        transform.position = start;
    }

    protected virtual void Update()
    {
        _elapsedTime += Time.deltaTime;
        
        var t = Mathf.Clamp01(_elapsedTime / FlightDuration);
        var pos = EvaluateArc(_startPos, _endPos, t, _arcHeight);
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

    protected Vector3 EvaluateArc(Vector3 startPos, Vector3 endPos, float t, float height)
    {
        var pos = Vector3.Lerp(startPos, endPos, t);
        var offsetY = 4f * height * t * (1f - t);

        pos.y += offsetY;
        
        return pos;
    }
}
