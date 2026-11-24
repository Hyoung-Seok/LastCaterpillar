using UnityEngine;

public interface IPursuitAttack
{
    public float PursuitDuration { get; }
    public float PursuitMoveScale { get; }
    
    public void OnPursuitEnter();
    public bool ShouldFireEarly(Vector3 targetPos);
}
