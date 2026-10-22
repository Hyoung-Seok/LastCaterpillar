using System;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public abstract class EnemyAttack : MonoBehaviour
{
    [SerializeField] protected float damage;
    [SerializeField] protected float contactDistance;

    protected Enemy Self;
    
    public abstract bool CanAttack(Vector3 targetPos, IDamageable target);
    public abstract void Attack(IDamageable target);
    
    protected virtual void Awake()
    {
        Self = GetComponent<Enemy>();
    }
}
