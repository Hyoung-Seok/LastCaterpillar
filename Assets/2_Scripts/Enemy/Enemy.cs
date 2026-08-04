using System;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, ISpatialItem
{
    public Vector3 Position => transform.position;
    public float InfluenceRadius => influenceRadius;

    [Header("Config")] 
    [SerializeField] protected float hp;
    [SerializeField] protected float speed;
    [SerializeField] protected float rotationSpeed;
    
    [Header("Spatial config")]
    [SerializeField] protected float influenceRadius;

    public event Action<Enemy> OnDeadEvent;
    
    private bool _isDead = false;

    public void TakeDamage(float damage)
    {
        if(_isDead) return;
        
        hp -= damage;
        if (hp > 0) return;
        
        OnDead();
    }
    
    protected virtual void OnDead()
    {
        _isDead = true;
        OnDeadEvent?.Invoke(this);
    }
    
    protected virtual void OnSpawned() { _isDead = false; }
    protected virtual void OnDespawned() { _isDead = true; }

    private void OnEnable()
    {
        // 등록하기
        OnSpawned();
    }

    private void OnDisable()
    {
        // 해제하기
        OnDespawned();
    }
}
