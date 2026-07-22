using UnityEngine;

public class DummyEnemy : MonoBehaviour, ISpatialItem
{
    public Vector3 Position => transform.position;
    public float Radius => separationRadius;

    [HideInInspector] public Vector3 Separation;
    [HideInInspector] public Vector3 ObstacleForce;
    [HideInInspector] public FlowField FlowField;
    
    [SerializeField] private float separationRadius = 0.5f;
    [SerializeField] private float maxSeparation = 0.4f;
    [SerializeField] private float maxObstacleForce = 0.8f;
    
    [SerializeField] private CharacterController cc;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float turnSpeed = 200f;

    private float _radius;
    
    private void Start()
    {
        _radius = cc.radius;
    }

    private void Update()
    {
        var cellDir = FlowField.GetCurrentCellDirection(transform.position);
        var flowVec = new Vector3(cellDir.x, 0, cellDir.y).normalized;
        var step = speed * Time.deltaTime;
        var pos = transform.position;
        
        if (FlowField.IsBlocked(pos))
        {
            cc.Move(flowVec * step);
            return;
        }

        var force = Vector3.ClampMagnitude(Separation, maxSeparation) +
                    Vector3.ClampMagnitude(ObstacleForce, maxObstacleForce);
        
        var desired = (flowVec + force).normalized * step;
        cc.Move(SlideAlongWalls(pos, desired));
        
        if(flowVec.sqrMagnitude > 0.0001f)
            RotationMoveDir(flowVec);
    }

    private Vector3 SlideAlongWalls(Vector3 pos, Vector3 desired)
    {
        var probeX = pos + ProbeOffset(desired.x, 0);
        var probeZ = pos + ProbeOffset(0, desired.z);
        
        var blockX = FlowField.IsBlocked(probeX);
        var blockZ = FlowField.IsBlocked(probeZ);

        if (blockX) desired.x = 0;
        if (blockZ) desired.z = 0;

        return desired;
    }

    private void RotationMoveDir(Vector3 dir)
    {
        var target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards
            (transform.rotation, target, Time.deltaTime * turnSpeed);
    }
    
    private Vector3 ProbeOffset(float dx, float dz)
    {
        return new Vector3(
            dx != 0 ? Mathf.Sign(dx) * (Mathf.Abs(dx) + _radius) : 0,
            0,
            dz != 0 ? Mathf.Sign(dz) * (Mathf.Abs(dz) + _radius) : 0);
    }
}
