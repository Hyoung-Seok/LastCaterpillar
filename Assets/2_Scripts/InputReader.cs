using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
public class InputReader : ScriptableObject
{
    // Input Action
    public InputAction PlayerMove => GameInput.Player.Move;
    public InputAction PlayerAim => GameInput.Player.Aim;
    public InputAction PlayerFire => GameInput.Player.Fire;
    public InputAction PlayerMgFire => GameInput.Player.FireMG;
    
    // Global Value
    public Vector3 AimPoint;
    
    private GameInput _gameInput;

    public GameInput GameInput
    {
        get
        {
            return _gameInput ??= new GameInput();
        }
    }
    
    public void EnableInput() => GameInput.Enable();
    public void DisableInput() => GameInput.Disable();
}
