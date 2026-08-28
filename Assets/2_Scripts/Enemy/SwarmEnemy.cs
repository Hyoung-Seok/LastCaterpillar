using System;
using Unity.Profiling;
using UnityEngine;
using Random = UnityEngine.Random;

public class SwarmEnemy : Enemy, IRepulsionReceiver
{
    [SerializeField, Range(0.1f, 0.95f)] private float maxObsForce = 0.9f;
    [SerializeField] private float correctionStiffens = 1f;
    
    private float _moveSpeed;
    private Vector3 _correction;
    private Vector3 _obstacleForce;
    
    private static readonly ProfilerMarker s_TransformMove = new("SwarmEnemy.TransformMove");

    public void ApplyRepulsion(Vector3 correction, Vector3 obsForce)
    {
        _correction = correction;
        _obstacleForce = obsForce;
    }

    private void Awake()
    {
        _moveSpeed = baseMoveSpeed * Random.Range(0.8f, 1.3f);
        animator.SetBool("IsMove", true);
    }

    public override void OnPlayerContact(Vector3 pos, IDamageable target)
    {
        if (IsDead) return;

        var distance = pos - Position;
        distance.y = 0;
        
        if (distance.sqrMagnitude > contactDistance * contactDistance)
            return;
        
        target.TakeDamage(damage);
        OnDead();
    }

    public override void Move(float dt)
    {
        var f = FlowField.Instance;
        if (f == null)
            return;
        
        var pos = transform.position;
        var step = _moveSpeed * dt;

        if (f.IsBlocked(pos))
        {
            using (s_TransformMove.Auto())
                transform.position += ToFlowVector(f.GetCurrentCellDirection(pos)) * step;
            return;
        }
        
        var (dir, speedScale) = GetDesiredMove(pos, f);
        var clampedObsForce = Vector3.ClampMagnitude(_obstacleForce, maxObsForce);
        
        // 1단계 : 조향 - dt 있음
        var steerDir = (dir + clampedObsForce).normalized;
        using (s_TransformMove.Auto())
            transform.position += SlideAlongWalls(pos, steerDir * (step * speedScale), f);
        
        // 2단계 : 겹침 해소 - dt 없음
        var pos2 = transform.position;
        using (s_TransformMove.Auto())
            transform.position += SlideAlongWalls(pos2, _correction * correctionStiffens, f);
        
        if(dir.sqrMagnitude > 0.0001f)
            RotationMoveDir(dir, dt);
    }

    /// dir = 단위벡터 또는 영벡터(크기를 태우지 말 것), speedScale = 0~1
    protected virtual (Vector3 dir, float speedScale) GetDesiredMove(Vector3 pos, FlowField f)
    {
        return (ToFlowVector(f.GetCurrentCellDirection(pos)), 1f);
    }

    private Vector3 SlideAlongWalls(Vector3 pos, Vector3 desired, FlowField f)
    {
        var probeX = pos + ProbeOffset(desired.x, 0);
        var probeZ = pos + ProbeOffset(0, desired.z);
        
        var blockX = f.IsBlocked(probeX);
        var blockZ = f.IsBlocked(probeZ);

        if (blockX) desired.x = 0;
        if (blockZ) desired.z = 0;

        return desired;
    }

    private void RotationMoveDir(Vector3 dir, float dt)
    {
        var target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards
            (transform.rotation, target, dt * rotationSpeed);
    }
    
    private Vector3 ProbeOffset(float dx, float dz)
    {
        return new Vector3(
            dx != 0 ? Mathf.Sign(dx) * (Mathf.Abs(dx) + bodyRadius) : 0,
            0,
            dz != 0 ? Mathf.Sign(dz) * (Mathf.Abs(dz) + bodyRadius) : 0);
    }
    
    private Vector3 ToFlowVector(Vector2Int vec) => new Vector3(vec.x, 0, vec.y).normalized;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.brown;
        Gizmos.DrawWireSphere(transform.position, contactDistance);
    }
}
