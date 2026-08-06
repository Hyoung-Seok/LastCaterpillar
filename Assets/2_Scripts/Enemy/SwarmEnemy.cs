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
        
        var cellDir = f.GetCurrentCellDirection(transform.position);
        var flowVec = new Vector3(cellDir.x, 0, cellDir.y).normalized;
        var step = speed * dt;
        var pos = transform.position;
        
        if (f.IsBlocked(pos))
        {
            cc.Move(flowVec * step);
            return;
        }

        var force = Vector3.ClampMagnitude(_separation, maxSeparation) +
                    Vector3.ClampMagnitude(_obstacleForce, maxObstacleForce);
        
        var desired = (flowVec + force).normalized * step;
        cc.Move(SlideAlongWalls(pos, desired, f));
        
        if(flowVec.sqrMagnitude > 0.0001f)
            RotationMoveDir(flowVec, dt);
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
