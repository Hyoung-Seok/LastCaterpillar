using System;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, ISpatialItem
{
    public Vector3 Position => transform.position;
    public float InfluenceRadius => influenceRadius;
    public bool IsDead => _isDead;

    [Header("Config")] 
    [SerializeField] protected float maxHp = 100;
    [SerializeField] protected float speed;
    [SerializeField] protected float rotationSpeed;
    
    [Header("Spatial config")]
    [SerializeField] protected float influenceRadius;
    
    private bool _isDead = false;
    private float _curHp;

    public void TakeDamage(float damage)
    {
        if(_isDead) return;
        
        _curHp -= damage;
        if (_curHp > 0) return;
        
        OnDead();
    }

    public abstract void Move(float dt);
    
    protected virtual void OnDead()
    {
        _isDead = true;
        gameObject.SetActive(false);
    }

    protected virtual void OnSpawned()
    {
        _isDead = false; 
        _curHp = maxHp;
    }
    protected virtual void OnDespawned() { _isDead = true; }

    private void OnEnable()
    {
        EnemyRegister.Instance.RegisterEnemy(this);
        OnSpawned();
    }

    private void OnDisable()
    {
        var register = EnemyRegister.Instance;
        
        if(register != null)
            register.UnRegisterEnemy(this);
        
        OnDespawned();
    }
}
