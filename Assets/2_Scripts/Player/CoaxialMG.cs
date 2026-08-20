using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerManager))]
public class CoaxialMG : MonoBehaviour
{
    [SerializeField] private GameObject roundPrefab;
    [SerializeField] private Transform roundContainer;
    [SerializeField] private CoaxialMGData config;
    [SerializeField] private Transform firePos;

    [SerializeField, Min(1)] private int initialPoolSize = 100;
    [SerializeField, Min(1)] private int expansionSize = 20; 

    private Queue<Round> _rounds; 
    private List<Round> _firedRounds;
    private InputAction _fireAction;
    
    private float _nextFireTime;
        
    private void Start()
    {
        _rounds = new Queue<Round>();
        _firedRounds = new List<Round>();
        _nextFireTime = Time.time + config.FireInterval;
        _fireAction = GetComponent<PlayerManager>().InputReader.PlayerMgFire;
        
        CreateRound(initialPoolSize);
    }

    private void Update()
    {
        if (_fireAction.IsPressed())
        {
            FireCoaxialMg();
        }

        var dt = Time.deltaTime;
        for (var i = _firedRounds.Count - 1; i >= 0; --i)
        {
            var r = _firedRounds[i];

            r.CurPosition += r.Direction * (config.Velocity * dt);
            r.CurLifeTime += dt;
            r.Bullet.transform.position = r.CurPosition;

            if (r.CurLifeTime < config.LifeTime)
            {
                continue;
            }
            
            r.Bullet.SetActive(false);
            _rounds.Enqueue(r);

            _firedRounds[i] = _firedRounds[^1];
            _firedRounds.RemoveAt(_firedRounds.Count - 1);
        }
    }

    private void FireCoaxialMg()
    {
        if (!IsCanFire()) return;

        if (_rounds.Count <= 0)
        {
            CreateRound(expansionSize);   
        }
        
        var r = _rounds.Dequeue();
        
        var dir = firePos.forward;
        dir.y = 0;
        r.Direction = dir.normalized;
        
        r.CurPosition = firePos.position;
        r.CurLifeTime = 0;
        
        r.Bullet.transform.SetPositionAndRotation(firePos.position, Quaternion.LookRotation(r.Direction));
        r.Bullet.SetActive(true);
        
        _firedRounds.Add(r);
        _nextFireTime = Time.time + config.FireInterval;
    }

    private void CreateRound(int count)
    {
        for (var i = 0; i < count; ++i)
        {
            var obj = Instantiate(roundPrefab, Vector3.zero, Quaternion.identity, roundContainer);
            _rounds.Enqueue(new Round(obj));
        }
    }
    
    private bool IsCanFire() => Time.time >= _nextFireTime;
}

public class Round
{
    public GameObject Bullet;
    public Vector3 CurPosition;
    public Vector3 Direction;
    public float CurLifeTime;

    public Round(GameObject bullet)
    {
        Bullet = bullet;
        CurPosition = Vector3.zero;
        CurLifeTime = 0;
        
        bullet.SetActive(false);
    }
}
