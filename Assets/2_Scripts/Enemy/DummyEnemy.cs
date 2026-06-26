using UnityEngine;

public class DummyEnemy : MonoBehaviour
{
    [SerializeField] private FlowField flowField;
    [SerializeField] private CharacterController cc;
    [SerializeField] private float speed = 5f;

    private void Update()
    {
        var cellDir = flowField.GetCurrentCellDirection(transform.position);
        var dir = new Vector3(cellDir.x, 0, cellDir.y).normalized;
        
        cc.Move(dir * (speed * Time.deltaTime));
    }
}
