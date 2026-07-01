using UnityEngine;

public class DummyEnemy : MonoBehaviour
{
    [HideInInspector] public Vector3 Separation;
    [HideInInspector] public FlowField FlowField;
    
    [SerializeField] private float maxSeparation = 0.4f;
    
    [SerializeField] private CharacterController cc;
    [SerializeField] private float speed = 5f;

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

        var desired = (flowVec + Vector3.ClampMagnitude(Separation, maxSeparation)).normalized * step;
        cc.Move(SlideAlongWalls(pos, desired));
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
    
    private Vector3 ProbeOffset(float dx, float dz)
    {
        return new Vector3(
            dx != 0 ? Mathf.Sign(dx) * (Mathf.Abs(dx) + _radius) : 0,
            0,
            dz != 0 ? Mathf.Sign(dz) * (Mathf.Abs(dz) + _radius) : 0);
    }
}
