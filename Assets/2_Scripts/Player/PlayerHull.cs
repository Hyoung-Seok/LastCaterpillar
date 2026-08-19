using System.Collections.Generic;
using UnityEngine;

public class PlayerHull : MonoBehaviour, IDamageable
{
    [SerializeField] private Collider hullCollider;
    [SerializeField] private float maxHp = 1000f;
    [SerializeField] private float contactRadius = 10f;
    
    private List<Enemy> _buffer;
    private float _curHp;

    private void Start()
    {
        _buffer = new List<Enemy>();
        _curHp = maxHp;
    }

    private void Update()
    {
        var pos = transform.position;
        EnemyRegister.Instance.QueryForRadius(pos, contactRadius, _buffer);
        
        foreach (var e in _buffer)
        {
            var surfacePoint = hullCollider.ClosestPoint(e.Position);
            e.OnPlayerContact(surfacePoint, this);
        }
    }
    
    public void TakeDamage(float dmg)
    {
        _curHp -= dmg;
        
        // TODO : 사망 처리는 나중에. 지금은 로그만
        Debug.Log(_curHp);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, contactRadius);
    }
}
