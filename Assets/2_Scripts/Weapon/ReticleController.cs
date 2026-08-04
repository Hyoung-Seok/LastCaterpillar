using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ReticleController : MonoBehaviour
{
    [Header("Components")] 
    [SerializeField] private InputReader inputReader;
    [SerializeField] private DecalProjector decal;

    [Header("Config")] 
    [SerializeField] private float yPos;
    [ColorUsage(true, true), SerializeField] private Color enableColor;
    [ColorUsage(true, true), SerializeField] private Color disableColor;

    private static readonly int COLOR = Shader.PropertyToID("_Color");
    private bool _prevState;
    
    private Material _enableMat;
    private Material _disableMat;

    private void Awake()
    {
        _enableMat = new Material(decal.material);
        _disableMat = new Material(decal.material);
        
        _enableMat.SetColor(COLOR, enableColor);
        _disableMat.SetColor(COLOR, disableColor);

        decal.material = _disableMat;
    }

    private void LateUpdate()
    {
        var p = inputReader.AimPoint;
        transform.position = new Vector3(p.x, yPos, p.z);
    }

    public void UpdateReticleState(bool isEnable)
    {
        if (_prevState == isEnable) return;
        
        _prevState = isEnable;
        decal.material = isEnable ? _enableMat : _disableMat;
    }

    public void SetReticleRadius(float radius)
    {
        var s = decal.size;
        s.x = s.y = radius * 2f;
        
        decal.size = s;
    }

    private void OnDestroy()
    {
        if(_enableMat != null) Destroy(_enableMat);
        if(_disableMat != null) Destroy(_disableMat);
    }
}
