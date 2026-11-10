using UnityEngine;

public interface IAttackTarget : IDamageable
{
    public Vector3 GetSurfacePoint(Vector3 from);
}
