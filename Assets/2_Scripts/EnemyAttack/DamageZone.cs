using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float radius;
    [SerializeField] private float tickInterval;
    [SerializeField] private int tickCount;

    [Header("Visual config")] 
    [SerializeField, Min(0.01f)] private float spreadDuration = 0.3f;
    [SerializeField, Min(0.01f)] private float dissolveDuration = 0.3f;

    [Header("Components")] 
    [SerializeField] private Renderer rangeAlertObj;

    private IAttackTarget _target;
    private List<Vector3> _localCenters;
    private float _damage;
    
    private float _nextTickTime;
    private float _warnDuration;
    private float _stateElapsed;
    private int _curTickCount;

    private Renderer[] _alertRenderers;
    private MaterialPropertyBlock _mpb;
    private ZoneState _curZoneState;
    
    private static readonly int FillID = Shader.PropertyToID("_Fill"); 
    private static readonly int AcidID = Shader.PropertyToID("_Acid");
    private static readonly int DissolveID = Shader.PropertyToID("_Dissolve");
    
    public void Init(IAttackTarget target, List<Vector3> centers, float damage, float warningDuration)
    {
        _target = target;
        _localCenters = centers;
        _damage = damage;
        _warnDuration = warningDuration;
        _stateElapsed = 0f;
        
        _nextTickTime = 0;
        _curTickCount = 0;

        _alertRenderers = new Renderer[centers.Count];
        _mpb = new MaterialPropertyBlock();
        
        _curZoneState = ZoneState.Alert;
        CreateRangeLocationAlert();
    }
    
    // 실제 대미지 적용 지점
    public void Land()
    {
        UpdateAlertShader(1);
        
        _curZoneState = ZoneState.Active;
        _stateElapsed = 0f;
    }

    private void Update()
    {
        _stateElapsed += Time.deltaTime;
        
        switch (_curZoneState)
        {
            case ZoneState.Alert:
                UpdateAlertShader(Mathf.Clamp01(_stateElapsed / _warnDuration));
                break;
            
            case ZoneState.Active:
                UpdateAlertShader(Mathf.Clamp01(_stateElapsed / spreadDuration));
                
                if(_nextTickTime > Time.time)
                    return;
                
                CheckAttackTargetInRange();
                
                _nextTickTime = Time.time + tickInterval;
                _curTickCount++;

                if (_curTickCount >= tickCount)
                {
                    _curZoneState = ZoneState.Dissolve;
                    _stateElapsed = 0f;
                }
                break;
            
            case ZoneState.Dissolve:
                UpdateAlertShader(Mathf.Clamp01(_stateElapsed / dissolveDuration));
                
                if(_stateElapsed >= dissolveDuration)
                    Destroy(gameObject);
                break;
        }
    }

    // 착탄 경고 지점 표시 : 이 땐 실제 데미지 X
    private void CreateRangeLocationAlert()
    {
        var i = 0;
        
        foreach (var center in _localCenters)
        {
            var obj = Instantiate(rangeAlertObj, transform);
            _alertRenderers[i++] = obj;
            
            obj.transform.localPosition = center;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
        }
    }

    private void UpdateAlertShader(float t)
    {
        switch (_curZoneState)
        {
            case ZoneState.Alert:
                _mpb.SetFloat(FillID, t);
                break;

            case ZoneState.Active:
                _mpb.SetFloat(AcidID, t);
                break;

            case ZoneState.Dissolve:
                _mpb.SetFloat(DissolveID, t);
                break;
        }

        foreach (var r in _alertRenderers)
        {
            r.SetPropertyBlock(_mpb);
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
    
    private enum ZoneState
    {
        Alert,
        Active,
        Dissolve
    }
}

