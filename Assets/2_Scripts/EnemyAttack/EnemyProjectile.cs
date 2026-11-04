using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float flightMinTime;
    [SerializeField, Min(0.1f)] private float flightMaxTime;
    [SerializeField, Min(0.1f)] private float arcMinHeight;
    [SerializeField, Min(0.1f)] private float arcMaxHeight;
    
    private float _elapsedTime = 0;
    private float _flightDuration;
    private float _arcHeight;
    private Vector3 _startPosition;
    private Vector3 _endPosition;

    public void Throw(Vector3 start, Vector3 target, float minDistance, float maxDistance)
    {
        _startPosition = start;
        _endPosition = target;

        var distance = Vector3.Distance(start, target);
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
    }
}
