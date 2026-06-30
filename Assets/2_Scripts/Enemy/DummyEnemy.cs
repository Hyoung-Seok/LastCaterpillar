using System;
using UnityEngine;

public class DummyEnemy : MonoBehaviour
{
    [HideInInspector] public Vector3 Separation;
    [HideInInspector] public FlowField FlowField;
    
    [SerializeField] private float maxSeparation = 0.4f;
    [SerializeField] private float flowWeight = 1f;
    [SerializeField] private float separationWeight = 0.5f;
    
    [SerializeField] private CharacterController cc;
    [SerializeField] private float speed = 5f;

    // --- 디버그 훅 (EnemyClickInspector가 켜고 끔, 진단 끝나면 제거) ---
    [HideInInspector] public bool DebugSelected;

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
        
        // 지금 이 프레임에서 이동 방향
        if (FlowField.IsBlocked(pos))
        {
            cc.Move(flowVec * step);
            return;
        }

        var desired = (flowVec + Vector3.ClampMagnitude(Separation, maxSeparation)).normalized * step;
        
        // 각 축을 그 축으로만 갔을 때 다음 위치가 막혔는지 따로 검사
        var probeX = pos + Probe(desired.x, 0);
        var probeZ = pos + Probe(0, desired.z);
        
        var blockX = FlowField.IsBlocked(probeX);
        var blockZ = FlowField.IsBlocked(probeZ);

        if (blockX) desired.x = 0;
        if (blockZ) desired.z = 0;
        
        var before = transform.position;
        var flags = cc.Move(desired);
            
        if (DebugSelected)
        {
            var moved = (transform.position - before).magnitude;
            var cell = FlowField.WorldToCell(pos);
            var cellType = FlowField.GetCellType(pos);
            Debug.Log(
                $"[{name}] cell={cell} type={cellType} flow={cellDir} sep={Separation} " +
                $"desired={desired} |desired|={desired.magnitude:F4} " +
                $"blockX={blockX} blockZ={blockZ} flags={flags} moved={moved:F4}");
        }
    }

    private Vector3 Probe(float dx, float dz)
    {
        return new Vector3(
            dx != 0 ? Mathf.Sign(dx) * (Mathf.Abs(dx) + _radius) : 0,
            0,
            dz != 0 ? Mathf.Sign(dz) * (Mathf.Abs(dz) + _radius) : 0);
    }
}
