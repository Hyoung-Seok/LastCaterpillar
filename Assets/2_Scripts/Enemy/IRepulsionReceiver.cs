using UnityEngine;

public interface IRepulsionReceiver : IRepulsionSource
{
    public void ApplyRepulsion(Vector3 sep, Vector3 correction, Vector3 obsForce);
    public float Mass { get; }
}
