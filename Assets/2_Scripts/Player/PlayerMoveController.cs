using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveController : MonoBehaviour
{
    [Header("Component")] 
    [SerializeField] private InputReader inputReader;
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

    [Header("Wheel Config")] 
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform[] wheelPos;
    [SerializeField] private float restLength = 1.4f;
    [SerializeField] private float springStrength = 0.1f;
    [SerializeField] private float damper = 10f;
    
    private InputAction _move;
    private Vector2 _moveInput;
    private float _curSpeed = 0f;
    private float _curTurnSpeed = 0f;
    
    private void Start()
    {   
        inputReader.EnableInput();
        _move = inputReader.PlayerMove;
    }

    private void Update()
    {
        _moveInput = _move.ReadValue<Vector2>();
        
        UpdateSpeed();
        UpdateTurnSpeed();
    }
    
    private void FixedUpdate()
    {        
        WheelRay();
        
        var v = transform.forward * _curSpeed;
        v.y = rb.linearVelocity.y;
        rb.linearVelocity = v;

        var av = rb.angularVelocity;
        av.y = _curTurnSpeed * Mathf.Deg2Rad;
        rb.angularVelocity = av;
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

    private void WheelRay()
    {
        foreach (var wheel in wheelPos)
        {
            var hit = Physics.Raycast(wheel.position, -transform.up, 
                out var info, restLength, groundLayer);

            if (hit)
            {
                var force = CalculateSpringDamper(info, wheel);
                rb.AddForceAtPosition(force * transform.up, wheel.position);
            }
            
            Debug.DrawRay(wheel.position, -transform.up * restLength, hit ? Color.green : Color.red);
        }
    }

    private float CalculateSpringDamper(RaycastHit hit, Transform wheel)
    {
        var compression = restLength - hit.distance;       //눌린 정도의 계산
        var springForce = compression * springStrength;
        
        // 댐퍼 힘
        var pointVel = rb.GetPointVelocity(wheel.position);     // wheel 지점의 힘을 구함
        var springVel = Vector3.Dot(pointVel, transform.up);
        var dampForce = springVel * damper;
        
        return springForce - dampForce;
    }
}
