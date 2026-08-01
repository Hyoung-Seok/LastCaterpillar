using System;
using UnityEngine;

[Serializable]
public class LoadoutEntry
{
    [SerializeField] private ShellData data;
    [SerializeField] private int maxAmmo;
    
    public ShellData ShellData => data;
    public int MaxAmmo => maxAmmo;
}

