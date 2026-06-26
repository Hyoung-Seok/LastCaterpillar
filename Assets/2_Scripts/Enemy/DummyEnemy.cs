using UnityEngine;

public class DummyEnemy : MonoBehaviour
{
    [HideInInspector] public Vector3 Separation;
    [HideInInspector] public FlowField FlowField;
    
    [SerializeField] private float flowWeight = 1f;
    [SerializeField] private float separationWeight = 0.5f;
    
    [SerializeField] private CharacterController cc;
    [SerializeField] private float speed = 5f;

    
    private void Update()
    {
        var cellDir = FlowField.GetCurrentCellDirection(transform.position);
        var flow = new Vector3(cellDir.x, 0, cellDir.y).normalized;

        var dir = flow * flowWeight + Separation * separationWeight;
        
        cc.Move(dir.normalized * (speed * Time.deltaTime));
    }
}
