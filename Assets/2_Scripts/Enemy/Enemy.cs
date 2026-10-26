using System;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IRepulsionSource, ISteeringSource, IDamageable
{
    public Vector3 Position => transform.position;
    public float BodyRadius => bodyRadius;
    public float Mass => mass;
    public float InfluenceRadius => influenceRadius;
    public bool IsDead => _isDead;
    public EnemyAttackController AtkController => atkController;
    
    [Header("Components")]
    [SerializeField] protected Animator animator;

    [Header("Attack Source")] 
    [SerializeField] protected EnemyAttackController atkController;
    [SerializeField] protected EnemyAttack nonPhaseAttack;

    [Header("Config")] 
    [SerializeField] protected float maxHp = 100;
    [SerializeField, Min(0.1f)] protected float damage = 50f;
    [SerializeField, Min(0.1f)] protected float contactDistance = 3f;
    [SerializeField] protected float baseMoveSpeed = 5f;
    [SerializeField] protected float rotationSpeed;
    [SerializeField] protected float bodyRadius = 0.3f;
    [SerializeField, Tooltip("반발 저항 가중치"), Min(0.1f)] private float mass;
    [SerializeField, Min(0.1f)] protected float influenceRadius;
    
    private bool _isDead = false;
    private float _curHp;

    public void TakeDamage(float dmg)
    {
        if(_isDead) return;
        
        _curHp -= dmg;
        if (_curHp > 0) return;
        
        OnDead();
    }

    public void PlayAnimationClipByHashCode(int hash, float transitionTime)
    {
        animator.CrossFadeInFixedTime(hash, transitionTime);
    }

    public abstract void Move(float dt);

    public virtual void OnHeardNoise(Vector3 pos, float now) { }

    public virtual void OnPlayerContact(Vector3 pos, IDamageable target) { }

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

    protected virtual void OnValidate()
    {
        if(atkController == null && nonPhaseAttack == null)
            Debug.LogWarning("적에게 할당된 공격이 없습니다.");
        
        if(atkController != null && nonPhaseAttack != null)
            Debug.LogError($"{atkController}와 {nonPhaseAttack}에 공격이 모두 할당되었습니다.");
    }
}
