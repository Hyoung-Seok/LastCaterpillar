using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerManager))]
public class MainTurret : MonoBehaviour
{
    [Header("Components")] 
    [SerializeField] private ReticleController reticle;
    [SerializeField] private Transform turret;
    [SerializeField] private Transform firePos;
    
    [Header("Shell")]
    [SerializeField] private Shell shell;
    [SerializeField] private float reloadTime = 4.0f;
    [SerializeField] private float fireThreshold = 0.995f;
    
    private InputReader _inputReader;
    private InputAction _fire;
    private Shell _currentLoadedShell;

    private bool _isReloading = false;
    private float _curReloadTime = 0f;

    private void Start()
    {
        var pm = GetComponent<PlayerManager>();
        _inputReader = pm.InputReader;
        
        _fire = _inputReader.PlayerFire;
        _fire.performed += FireMainTurret;
        pm.OnDisableTurret += OnDisableTurret;
        
        _currentLoadedShell = shell;
    }

    private void Update()
    {
        reticle.UpdateReticleState(IsCanFire());
        
        if (_isReloading == false) return;
        
        _curReloadTime += Time.deltaTime;
        
        if (_curReloadTime >= reloadTime)
        {
            _isReloading = false;
            _curReloadTime = 0f;
        }
    }

    public void ChangeShell()
    {
        
    }
    
    private void FireMainTurret(InputAction.CallbackContext context)
    {
        if (!IsCanFire()) return;

        _isReloading = true;

        var s = Instantiate(_currentLoadedShell, firePos.position, Quaternion.identity);
        s.OnStartFire(firePos.position, _inputReader.AimPoint);
    }

    private bool IsCanFire()
    {
        // 현재 장전된 탄약이 없거나 장전중이라면 false
        if (_currentLoadedShell == null || _isReloading)
            return false;

        var toAim = _inputReader.AimPoint - firePos.position;
        toAim.y = 0;

        var minDist = _currentLoadedShell.MinFireDistance;
        // 포탄이 너무 가까우면 
        if (toAim.sqrMagnitude < minDist * minDist)
            return false;
        
        toAim.Normalize();
        var dot = Vector3.Dot(firePos.forward, toAim);
        
        // 포탑이 발사 지점을 바라보고 있지 않다면 false
        return dot > fireThreshold;
    }

    private void OnDisableTurret()
    {
        _fire.performed -= FireMainTurret;
    }
}
