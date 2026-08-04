using System;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, ISpatialItem
{
    public Vector3 Position => transform.position;
    public virtual float InfluenceRadius => influenceRadius;

    [Header("Config")] 
    [SerializeField] protected float hp;
    [SerializeField] protected float speed;
    [SerializeField] protected float rotationSpeed;
    
    [Header("Seperation config")]
    [SerializeField] protected float influenceRadius;

    public event Action<Enemy> OnDeadEvent;

    public void TakeDamage(float damage)
    {
        hp -= damage;

        if (hp > 0) return;
        
        OnDead();
    }
    
    protected virtual void OnDead()
    {
        OnDeadEvent?.Invoke(this);
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }
}
