using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveController : MonoBehaviour
{
    [Header("Component")] 
    [SerializeField] private Rigidbody rb;

    [Header("Move Config")] 
    [SerializeField] private float forwardMaxSpeed;
    [SerializeField, Range(-100, -1)] private float reverseMaxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float breakDecel;
    [SerializeField] private float deceleration;

    private GameInput _gameInput;
    private InputAction _move;
    private Vector2 _moveInput;
    private float _curSpeed = 0f;
    
    private void Awake()
    {
        _gameInput = new GameInput();
        _move = _gameInput.Player.Move;
    }

    private void Update()
    {
        _moveInput = _move.ReadValue<Vector2>();
        
        UpdateSpeed();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = transform.forward * _curSpeed;
    }

    private void UpdateSpeed()
    {
        var inputDir = Math.Sign(_moveInput.y);
        var speedDir = Math.Sign(_curSpeed);
        var rate = 0f;
        var targetSpeed = 0f;
        
        if (inputDir == 0)
        {
            targetSpeed = 0f;
            rate = deceleration;
        }
        else if (speedDir != 0 && inputDir != speedDir)
        {
            targetSpeed = 0f;
            rate = breakDecel;
        }
        else
        {
            targetSpeed = (inputDir > 0) ? forwardMaxSpeed : reverseMaxSpeed;
            rate = acceleration;
        }
        
        _curSpeed = Mathf.MoveTowards(_curSpeed, targetSpeed, rate * Time.deltaTime);
    }

    private void OnEnable()
    {
        _gameInput.Enable();
    }

    private void OnDisable()
    {
        _gameInput.Disable();
    }
}
