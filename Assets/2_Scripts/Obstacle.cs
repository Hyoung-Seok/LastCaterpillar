using System;
using UnityEngine;

public class Obstacle : MonoBehaviour, IRepulsionSource
{
    public Vector3 Position => transform.position;
    public float InfluenceRadius => influenceRadius;
    
    [SerializeField] private float influenceRadius = 2.8f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, InfluenceRadius);
    }
}
