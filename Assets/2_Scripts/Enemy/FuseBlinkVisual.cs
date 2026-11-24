using UnityEngine;

public class FuseBlinkVisual : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField, ColorUsage(false, true)] private Color emissionColor;
    [SerializeField] private float startFreq;
    [SerializeField] private float endFreq;
    [SerializeField] private float sharpness;
    
    private static readonly int EmissionID = Shader.PropertyToID("_EmissionColor");
    private MaterialPropertyBlock _mpb;
    
    private float _duration;
    private float _elapsedTime;
    private float _phase;

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        enabled = false;
    }

    private void Update()
    {
        _elapsedTime += Time.deltaTime;

        var progress = Mathf.Clamp01(_elapsedTime / _duration);
        var freq = Mathf.Lerp(startFreq, endFreq, progress);
        _phase += freq * Time.deltaTime;
        
        var blink = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * _phase), sharpness);
        
        ApplyColor(emissionColor * blink);
    }

    public void Begin(float duration)
    {
        _duration = duration;
        _phase = _elapsedTime = 0;
        
        enabled = true;
    }

    private void ApplyColor(Color color)
    {
        foreach (var r in renderers)
        {
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(EmissionID, color);
            r.SetPropertyBlock(_mpb);
        }
    }
}
