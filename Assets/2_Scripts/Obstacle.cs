using UnityEngine;

public class Obstacle : MonoBehaviour, ISpatialItem
{
    public Vector3 Position => transform.position;
    public float Radius => influenceRadius;
    
    [SerializeField] private float influenceRadius = 2.8f;
}
