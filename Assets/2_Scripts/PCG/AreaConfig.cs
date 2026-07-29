using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class AreaConfig
{
    [Header("Voronoi")] 
    [SerializeField] private List<SeedConfig> seedConfigs;
    [SerializeField] private int minDistance = 10;
    [SerializeField] private int maxAttempts = 10;
    
    [Header("Perlin Noise")] 
    [SerializeField, Range(0.01f, 0.2f)] private float frequency;
    [SerializeField, Min(0)] private float strength;
    [SerializeField, Range(0, 0.9f)] private float sparsity;
    [SerializeField, Range(0, 0.2f)] private float sparsityScale;

    public List<SeedConfig> SeedConfigs => seedConfigs;
    public int MinDistance => minDistance;
    public int MaxAttempts => maxAttempts;
    public float Frequency => frequency;
    public float Strength => strength;
    public float Sparsity => sparsity;
    public float SparsityScale => sparsityScale;
}