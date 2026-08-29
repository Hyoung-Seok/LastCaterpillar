using UnityEngine;

public interface ISteeringSource : ISpatialItem
{
    public float InfluenceRadius { get; }
}
