using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float radius;
    [SerializeField] private float tickInterval;
    [SerializeField] private int tickCount;

    [Header("Components")] 
    [SerializeField] private GameObject rangeAlertObj;

    private IAttackTarget _target;
    private Vector3[] _localCenters;
    private float _damage;
    
    private float _nextTickTime;
    private int _curTickCount;
    private bool _isTicking;
    
    public void Init(IAttackTarget target, Vector3[] centers, float damage)
    {
        _target = target;
        _localCenters = centers;
        _damage = damage;
        
        _nextTickTime = 0;
        _curTickCount = 0;
        _isTicking = false;
        
        CreateRangeLocationAlert();
    }
    
    // 실제 대미지 적용 지점
    public void StartTick() => _isTicking = true;

    private void Update()
    {
        if (!_isTicking) return;
        if (_nextTickTime > Time.time) return;
        
        CheckAttackTargetInRange();

        _nextTickTime = Time.time + tickInterval;
        _curTickCount++;

        if (_curTickCount >= tickCount)
            Destroy(gameObject);
    }

    // 착탄 경고 지점 표시 : 이 땐 실제 데미지 X
    private void CreateRangeLocationAlert()
    {
        foreach (var center in _localCenters)
        {
            var obj = Instantiate(rangeAlertObj, transform);

            obj.transform.localPosition = center;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
        }
    }

    private void CheckAttackTargetInRange()
    {
        foreach (var localCenter in _localCenters)
        {
            var worldCenter = transform.TransformPoint(localCenter);
            var targetPos = _target.GetSurfacePoint(worldCenter);
            
            var distance = worldCenter - targetPos;
            distance.y = 0;
            
            if(distance.sqrMagnitude > radius * radius)
                continue;
            
            _target.TakeDamage(_damage);
            return;
        }
    }
}
