using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerManager))]
public class PlayerMoveController : MonoBehaviour
{
    [Header("Component")] 
    [SerializeField] private Rigidbody rb;

    [Header("Forward/Revers Config")] 
    [SerializeField, Min(1f)] private float forwardMaxSpeed = 10f;
    [SerializeField, Range(-100, -1)] private float reverseMaxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField, Min(0.1f)] private float maxDriveAccel = 10f;
    [SerializeField] private float breakDecel;
    [SerializeField] private float deceleration;
    
    [Header("Turn Config")]
    [SerializeField, Min(0.1f)] private float maxTurnAccel = 90f;
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

    [Header("Sound Config")] 
    [SerializeField, Min(0.1f)] private float idlingSoundRadius = 10f;
    [SerializeField, Min(0.1f)] private float maxMoveSoundRadius = 20f;
    [SerializeField, Min(0.1f)] private float noiseCheckInterval = 0.2f;
    
    private InputAction _move;
    private Vector2 _moveInput;
    private float _curSpeed = 0f;
    private float _curTurnSpeed = 0f;
    
    private float _curSoundRadius;
    private float _curNoiseCheckTime;
    
    private void Start()
    {   
        _move = GetComponent<PlayerManager>().InputReader.PlayerMove;
        
        _curSoundRadius = idlingSoundRadius;
        _curNoiseCheckTime = 0f;
    }

    private void Update()
    {
        _moveInput = _move.ReadValue<Vector2>();
        
        UpdateSpeed();
        UpdateTurnSpeed();
        
        _curNoiseCheckTime += Time.deltaTime;

        if (_curNoiseCheckTime >= noiseCheckInterval)
        {
            _curNoiseCheckTime -= noiseCheckInterval;
            NoiseSystem.Instance.Emit(transform.position, _curSoundRadius);
        }
    }
    
    private void FixedUpdate()
    {
        var groundedWheels = WheelRay();
        if (groundedWheels <= 0)
            return;

        PlayerMove();
        PlayerRotation();
    }

    private void PlayerMove()
    {
        var forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
        
        var v = rb.linearVelocity;
        v.y = 0;
        
        // 장애물 등에 막혔을 때 명령이 현실과 분리되는 것을 방지
        var actualForward = Vector3.Dot(v, forward);
        var slack = maxDriveAccel * Time.fixedDeltaTime;
        _curSpeed = Mathf.Clamp(_curSpeed, actualForward - slack, actualForward + slack);

        var target = forward * _curSpeed;
        var deltaV = target - v;
        var correction = Vector3.ClampMagnitude(deltaV, slack);
        
        rb.AddForce(correction, ForceMode.VelocityChange);
    }

    private void PlayerRotation()
    {
        var yaw = rb.angularVelocity.y;
        
        var actualYawDeg = yaw * Mathf.Rad2Deg;
        var turnSlack = maxTurnAccel * Time.fixedDeltaTime;
        _curTurnSpeed = Mathf.Clamp(_curTurnSpeed, actualYawDeg - turnSlack, actualYawDeg + turnSlack);
        
        var maxRot = maxTurnAccel * Mathf.Deg2Rad * Time.fixedDeltaTime;
        var target = _curTurnSpeed * Mathf.Deg2Rad;
        var deltaY = target - yaw;

        var correction = Mathf.Clamp(deltaY, -maxRot, maxRot);

        rb.AddTorque(Vector3.up * correction, ForceMode.VelocityChange);
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
        _curSoundRadius = Mathf.Lerp(idlingSoundRadius, maxMoveSoundRadius, 
            Mathf.Abs(_curSpeed) / forwardMaxSpeed);
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

    private int WheelRay()
    {
        var hitCount = 0;
        
        foreach (var wheel in wheelPos)
        {
            var hit = Physics.Raycast(wheel.position, -transform.up, 
                out var info, restLength, groundLayer);

            if (hit)
            {
                var force = CalculateSpringDamper(info, wheel);
                rb.AddForceAtPosition(force * transform.up, wheel.position);

                hitCount++;
            }
            
            Debug.DrawRay(wheel.position, -transform.up * restLength, hit ? Color.green : Color.red);
        }

        return hitCount;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _curSoundRadius);
    }
}
