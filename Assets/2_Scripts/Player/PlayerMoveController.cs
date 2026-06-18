using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveController : MonoBehaviour
{
    [Header("Component")] 
    [SerializeField] private Rigidbody rb;

    [Header("Forward/Revers Config")] 
    [SerializeField] private float forwardMaxSpeed;
    [SerializeField, Range(-100, -1)] private float reverseMaxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float breakDecel;
    [SerializeField] private float deceleration;
    
    [Header("Turn Config")]
    [SerializeField] private float turnSpeed;
    [SerializeField] private float turnBreakDecel;
    [SerializeField] private float turnAcceleration;
    [SerializeField] private float turnDeceleration;

    private GameInput _gameInput;
    private InputAction _move;
    private Vector2 _moveInput;
    private float _curSpeed = 0f;
    private float _curTurnSpeed = 0f;
    
    private void Awake()
    {
        _gameInput = new GameInput();
        _move = _gameInput.Player.Move;
    }

    private void Update()
    {
        _moveInput = _move.ReadValue<Vector2>();
        
        UpdateSpeed();
        UpdateTurnSpeed();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = transform.forward * _curSpeed;

        var angle = _curTurnSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, angle, 0));
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

    private void UpdateTurnSpeed()
    {
        var turnDir = Math.Sign(_moveInput.x);
        var turnSpeedDir = Math.Sign(_curTurnSpeed);
        var targetSpeed = 0f;
        var rate = 0f;
        
        if (turnDir == 0)
        {
            targetSpeed = 0f;
            rate = turnDeceleration;
        }
        else if (turnSpeedDir != 0 && turnDir != turnSpeedDir)
        {
            targetSpeed = 0f;
            rate = turnBreakDecel;
        }
        else
        {
            targetSpeed = turnDir * turnSpeed;
            rate = turnAcceleration;
        }
        
        _curTurnSpeed = Mathf.MoveTowards(_curTurnSpeed, targetSpeed, rate * Time.deltaTime);
    }

    private void OnEnable() => _gameInput.Enable();
    
    private void OnDisable() => _gameInput.Disable();
    
}
