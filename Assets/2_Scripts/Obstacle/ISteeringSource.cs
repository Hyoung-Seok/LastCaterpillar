using UnityEngine;

public interface ISteeringSource : IRepulsionSource
{
    public float InfluenceRadius { get; }
}
