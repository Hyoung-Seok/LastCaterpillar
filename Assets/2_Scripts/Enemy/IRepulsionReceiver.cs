using UnityEngine;

public interface IRepulsionReceiver : ISpatialItem
{
    public void ApplyRepulsion(Vector3 sep, Vector3 obsForce);
    public float Mass { get; }
}
