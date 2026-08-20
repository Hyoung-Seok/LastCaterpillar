using UnityEngine;

[CreateAssetMenu(fileName = "CoaxialMGData", menuName = "Scriptable Objects/CoaxialMGData")]
public class CoaxialMGData : ScriptableObject
{
    [Header("MG Bullet Config")]
    [SerializeField, Min(0.1f)] private float damage = 0.1f;
    [SerializeField, Min(0.1f)] private float velocity = 0.1f;
    [SerializeField, Min(0)] private int penetrationCount;
    [SerializeField, Min(0.1f)] private float hitRadius = 0.1f;

    [Header("MG Config")] 
    [SerializeField, Min(0.01f)] private float fireInterval = 0.01f;
    [SerializeField, Min(0.1f)] private float heatingRate = 0.1f;
    [SerializeField, Min(0.1f)] private float coolingRate = 0.1f;

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
    public float HeatingRate => heatingRate;
    public float CoolingRate => coolingRate;
    public float LifeTime => lifeTime;
    public float NoiseRadius => noiseRadius;
    public float NoiseInterval => noiseInterval;
    
}
