using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimController : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform turret;
    [SerializeField] private float rotationSpeed;

    private InputAction _aim;
    private Camera _camera;
    private Plane _aimPlane;
    
    private void Start()
    {
        _aim = inputReader.PlayerAim;
        _camera = Camera.main;
        _aimPlane = new Plane(Vector3.up, transform.position);
    }

    private void Update()
    {
        var screenPos = _aim.ReadValue<Vector2>();
        var ray = _camera.ScreenPointToRay(screenPos);

        if (_aimPlane.Raycast(ray, out var dist))
        {
            var hit = ray.GetPoint(dist);
            var dir = hit - turret.position;
            dir.y = 0;

            if (dir.sqrMagnitude > 0.001f)
            {
                var rot = Quaternion.LookRotation(dir);
                turret.rotation = Quaternion.RotateTowards
                    (turret.rotation, rot, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
