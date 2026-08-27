using System;
using UnityEngine;

public class Obstacle : MonoBehaviour, ISteeringSource
{
    public Vector3 Position => transform.position;
    public float InfluenceRadius => influenceRadius;
    public float BodyRadius => bodyRadius;

    [SerializeField] private float influenceRadius = 2.8f;
    [SerializeField] private float bodyRadius = 0.3f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, InfluenceRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, BodyRadius);
    }
}
