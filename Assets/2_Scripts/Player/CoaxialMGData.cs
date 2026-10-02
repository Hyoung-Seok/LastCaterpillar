using UnityEngine;

[CreateAssetMenu(fileName = "CoaxialMGData", menuName = "Scriptable Objects/CoaxialMGData")]
public class CoaxialMGData : ScriptableObject
{
    [Header("MG Bullet Config")]
    [SerializeField, Min(0.1f)] private float damage = 0.1f;
    [SerializeField, Min(0.1f)] private float velocity = 0.1f;
    [SerializeField, Min(1)] private int penetrationCount = 1;
    [SerializeField, Min(0.1f)] private float hitRadius = 0.1f;

    [Header("MG Config")] 
    [SerializeField, Min(0.01f)] private float fireInterval = 0.01f;
    [SerializeField, Min(0.001f)] private float heatPerShot = 0.1f;
    [SerializeField, Min(0.001f)] private float coolingRate = 0.1f;
    [SerializeField, Range(0f, 0.99f)] private float overHeatRelease = 0.3f; 

    [Header("Life Config")] 
    [SerializeField, Min(0.1f)] private float lifeTime = 0.1f;

    [Header("Noise Config")] 
    [SerializeField, Min(0.1f)] private float noiseRadius = 0.1f;
    [SerializeField, Min(0.1f)] private float noiseInterval = 0.1f;

    public float Damage => damage;
    public float Velocity => velocity;
    public int PenetrationCount => penetrationCount;
    public float HitRadius => hitRadius;
    public float FireInterval => fireInterval;
    public float HeatPerShot => heatPerShot;
    public float CoolingRate => coolingRate;
    public float OverHeatRelease => overHeatRelease;
    public float LifeTime => lifeTime;
    public float NoiseRadius => noiseRadius;
    public float NoiseInterval => noiseInterval;
    
}
