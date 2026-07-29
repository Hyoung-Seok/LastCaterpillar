using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerManager))]
public class MainTurrent : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform turret;
    [SerializeField] private Transform firePos;
    
    [Header("Shell")]
    [SerializeField] private Shell shell;
    [SerializeField] private float reloadTime = 4.0f;
    [SerializeField] private float fireThreshold = 0.995f;
    
    private InputReader _inputReader;
    private InputAction _fire;
    private Shell _currentRoadedShell;

    private bool _isReloading = false;
    private float _curReloadTime = 0f;

    private void Start()
    {
        var pm = GetComponent<PlayerManager>();
        _inputReader = pm.InputReader;
        
        _fire = _inputReader.PlayerFire;
        _fire.performed += FireMainTurret;
        pm.OnDisableTurret += OnDisableTurret;
        
        _currentRoadedShell = shell;
    }

    private void Update()
    {
        if (_isReloading == false) return;
        
        _curReloadTime += Time.deltaTime;
        if (_curReloadTime >= reloadTime)
        {
            _isReloading = false;
            _curReloadTime = 0f;
        }
    }

    public void ChangeShellType(ShellType type)
    {
        
    }
    
    private void FireMainTurret(InputAction.CallbackContext context)
    {
        if (!IsCanFire()) return;

        _isReloading = true;

        var s = Instantiate(_currentRoadedShell, firePos.position, Quaternion.identity);
        s.OnStartFire(firePos.position, _inputReader.AimPoint);
    }

    private bool IsCanFire()
    {
        // 현재 장전된 탄약이 없거나 장전중이라면 false
        if (_currentRoadedShell == null || _isReloading)
            return false;

        var dir = _inputReader.AimPoint - firePos.position;
        dir.y = 0;
        dir.Normalize();
        
        var dot = Vector3.Dot(firePos.forward, dir);
        
        // 포탑이 발사 지점을 바라보고 있지 않다면 false
        return dot > fireThreshold;
    }

    private void OnDisableTurret()
    {
        _fire.performed -= FireMainTurret;
    }
}
