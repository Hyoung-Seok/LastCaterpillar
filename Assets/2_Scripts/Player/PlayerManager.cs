using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    
    public InputReader InputReader => inputReader;
    public event Action OnDisableTurret;

    private void OnEnable()
    {
        inputReader.EnableInput();
    }

    private void OnDisable()
    {
        inputReader.DisableInput();
        OnDisableTurret?.Invoke();
    }
}
