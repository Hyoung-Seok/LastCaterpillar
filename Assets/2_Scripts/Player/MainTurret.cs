using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerManager))]
public class MainTurret : MonoBehaviour
{
    [Header("Components")] 
    [SerializeField] private ReticleController reticle;
    [SerializeField] private Transform firePos;
    
    [Header("Shell")]
    [SerializeField] private LoadoutEntry[] loadOut;
    [SerializeField] private float reloadTime = 4.0f;
    [SerializeField] private float fireThreshold = 0.995f;
    
    private InputReader _inputReader;
    private InputAction _fire;
    
    private AmmoSlot[] _slots;
    private ShellData LoadedShell => _slots[_curShellIndex].ShellData;
    private AmmoSlot CurSlot =>  _slots[_curShellIndex];
    private int _curShellIndex;

    private bool _isReloading = false;
    private float _curReloadTime = 0f;

    private void Start()
    {
        var pm = GetComponent<PlayerManager>();
        _inputReader = pm.InputReader;
        
        InitSlot();
        
        _fire = _inputReader.PlayerFire;
        _fire.performed += FireMainTurret;
        pm.OnDisableTurret += OnDisableTurret;
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

    public void ChangeShell(int index)
    {
        if (index >= _slots.Length || index < 0)
            return;

        _curShellIndex = index;
        reticle.SetReticleRadius(LoadedShell.BlastRadius);
    }
    
    private void FireMainTurret(InputAction.CallbackContext context)
    {
        if (!IsCanFire()) return;

        _isReloading = true;

        var s = Instantiate(LoadedShell.ShellPrefab, firePos.position, Quaternion.identity);
        s.OnStartFire(LoadedShell, firePos.position, _inputReader.AimPoint);
        CurSlot.Consume();
        
        NoiseSystem.Instance.Emit(transform.position, CurSlot.ShellData.NoiseRadius);
    }

    private bool IsCanFire()
    {
        // 현재 장전된 탄약이 없거나 장전중이라면 false
        if (LoadedShell == null || _isReloading || CurSlot.CurrentAmmo <= 0)
            return false;

        var toAim = _inputReader.AimPoint - firePos.position;
        toAim.y = 0;

        var minDist = LoadedShell.MinFireDistance;
        // 포탄이 너무 가까우면 
        if (toAim.sqrMagnitude < minDist * minDist)
            return false;
        
        toAim.Normalize();
        var dot = Vector3.Dot(firePos.forward, toAim);
        
        // 포탑이 발사 지점을 바라보고 있지 않다면 false
        return dot > fireThreshold;
    }

    private void InitSlot()
    {
        if (loadOut.Length <= 0)
        {
            Debug.LogWarning("Loadout is empty", this);
            enabled = false;

            return;
        }
        
        _slots = new AmmoSlot[loadOut.Length];
        for (var i = 0; i < loadOut.Length; i++)
        {
            if (loadOut[i].ShellData == null)
            {
                Debug.LogError($"[MainTurret] loadOut[{i}]의 ShellData가 비어 있습니다.", this);
                enabled = false;
                
                return;
            }
            
            _slots[i] = new AmmoSlot(loadOut[i]);
        }

        ChangeShell(0);
    }

    private void OnDisableTurret()
    {
        _fire.performed -= FireMainTurret;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (_slots == null || _slots[_curShellIndex] == null)
            return;
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, LoadedShell.NoiseRadius);
    }
}

public class AmmoSlot
{
    public ShellData ShellData { get; private set; }
    public int MaxAmmo { get; private set; }
    public int CurrentAmmo { get; private set; }

    public AmmoSlot(LoadoutEntry entry)
    {
        ShellData = entry.ShellData;
        CurrentAmmo = MaxAmmo = entry.MaxAmmo;
    }

    public void Consume()
    {
        if (CurrentAmmo <= 0)
            return;
        
        CurrentAmmo--;
    }
}