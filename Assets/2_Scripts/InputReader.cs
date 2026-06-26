using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
public class InputReader : ScriptableObject
{
    public InputAction PlayerMove => GameInput.Player.Move;
    public InputAction PlayerAim => GameInput.Player.Aim;
    
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
