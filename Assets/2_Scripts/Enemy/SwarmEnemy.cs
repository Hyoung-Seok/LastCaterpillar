using UnityEngine;

public class SwarmEnemy : Enemy, IRepulsionReceiver
{
    [SerializeField] private float maxSeparation = 0.4f;
    [SerializeField] private float maxObstacleForce = 0.8f;
    
    [Header("Components")]
    [SerializeField] private CharacterController cc;
    
    private float _bodyRadius;
    private Vector3 _separation;
    private Vector3 _obstacleForce;

    public void ApplyRepulsion(Vector3 sep, Vector3 obsForce)
    {
        _separation = sep;
        _obstacleForce = obsForce;
    }

    private void Awake()
    {
        _bodyRadius = cc.radius;
    }

    public override void Move(float dt)
    {
        var f = FlowField.Instance;
        if (f == null)
            return;
        
        var pos = transform.position;
        var step = speed * dt;

        if (f.IsBlocked(pos))
        {
            var flowDir = f.GetCurrentCellDirection(pos);

            cc.Move(new Vector3(flowDir.x, 0f, flowDir.y) * step);
            return;
        }
        
        var (dir, speedScale) = GetDesiredMove(pos, f);
        
        var force = Vector3.ClampMagnitude(Vector3.ClampMagnitude(_separation, maxSeparation) +
                    Vector3.ClampMagnitude(_obstacleForce, maxObstacleForce), 0.9f);
        
        var desired = (dir + force).normalized * (step * speedScale);
        cc.Move(SlideAlongWalls(pos, desired, f));
        
        if(dir.sqrMagnitude > 0.0001f)
            RotationMoveDir(dir, dt);
    }

    /// dir = 단위벡터 또는 영벡터(크기를 태우지 말 것), speedScale = 0~1
    protected virtual (Vector3 dir, float speedScale) GetDesiredMove(Vector3 pos, FlowField f)
    {
        var dir = f.GetCurrentCellDirection(pos);
        return (new Vector3(dir.x, 0, dir.y).normalized, 1f);
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
            dx != 0 ? Mathf.Sign(dx) * (Mathf.Abs(dx) + _bodyRadius) : 0,
            0,
            dz != 0 ? Mathf.Sign(dz) * (Mathf.Abs(dz) + _bodyRadius) : 0);
    }
}
