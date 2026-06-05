using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class CityGenerator : MonoBehaviour
{
    [Header("Debug")] 
    [SerializeField] private bool liveDebugMode;
    
    [Header("City Config")] 
    [SerializeField] private int seed;
    [SerializeField, Min(1)] private int width;
    [SerializeField, Min(2)] private int height;
    [SerializeField, Min(1)] private int cellSize;

    [Header("Road Config")] 
    [SerializeField, Min(1)] private int roadMinGap;
    [SerializeField, Min(1)] private int roadMaxGap;

    [Header("Voronoi")] 
    [SerializeField] private List<SeedConfig> seedConfigs;
    [SerializeField] private int minDistance = 10;
    [SerializeField] private int maxAttempts = 10;
    [SerializeField, Min(1)] private int buildingBandDepth = 2;

    [Header("Perlin Noise")] 
    [SerializeField, Range(0.01f, 0.2f)] private float frequency;
    [SerializeField, Min(0)] private float strength;
    [SerializeField, Range(0, 0.9f)] private float sparsity;
    [SerializeField, Range(0, 0.2f)] private float sparsityScale;
    
    public CityLayout CityLayout { get; private set; }
    private Random _prng;

    public void GenerateCity()
    {
        _prng = new Random(seed);
        CityLayout = new CityLayout(seed, width, height, cellSize);
        
        GenerateRoad();
        GenerateArea();
    }

    private void GenerateRoad()
    {
        var row = ChoseRoadLine(height);
        var col = ChoseRoadLine(width);

        foreach (var y in row)
        {
            for (var x = 0; x < width; ++x)
            {
                CityLayout.Cells[x, y] = ECellType.Road;
            }
        }

        foreach (var x in col)
        {
            for (var y = 0; y < height; ++y)
            {
                CityLayout.Cells[x, y] = ECellType.Road;
            }
        }
    }

    private void GenerateArea()
    {
        var allSeed = PlaceSeed();
        var warpOffset = new Vector4(_prng.Next(0, 10000), _prng.Next(0, 10000)
            ,_prng.Next(0, 10000),  _prng.Next(0, 10000));
        
        var sparsityOffset = new Vector2(_prng.Next(0, 10000), _prng.Next(0, 10000));

        for (var x = 0; x < width; ++x)
        {
            for (var y = 0; y < height; ++y)
            {
                if(CityLayout.Cells[x, y] == ECellType.Road) continue;
                if(CityLayout.NearRoadDirection(x,y, buildingBandDepth, _prng) == null) continue;

                var s = Mathf.PerlinNoise((x + sparsityOffset.x) * sparsityScale,
                    (y + sparsityOffset.y) * sparsityScale);
                
                if(s <= sparsity) continue;

                var p = WarpCell(x, y, warpOffset);
                CityLayout.Cells[x, y] = FindNearestSeedType(p, allSeed);
            }
        }
    }

    private Vector2 WarpCell(int x, int y, Vector4 warpOffset)
    {
        var wx = (Mathf.PerlinNoise(x * frequency + warpOffset.x, y * frequency + warpOffset.y) - 0.5f) * strength;
        var wy = (Mathf.PerlinNoise(x * frequency + warpOffset.w, y * frequency + warpOffset.z) - 0.5f) * strength;
        
        return new Vector2(wx + x, wy + y);
    }

    private List<(ECellType type, Vector2Int pos, float weight)> PlaceSeed()
    {
        var result = new List<(ECellType type, Vector2Int pos, float weight)>();
        
        foreach (var s in seedConfigs)
        {
            Vector2Int p;
            var attempts = 0;
            do
            {
                p = new Vector2Int(_prng.Next(0, width), _prng.Next(0, height));
                attempts++;
                
            } while (CheckSeedMinDistance(p, result) == false
                     && attempts < maxAttempts);
            result.Add((s.Type, p, s.Weight));
        }

        return result;
    }

    private ECellType FindNearestSeedType(Vector2 pos, List<(ECellType type, Vector2Int pos, float weight)> seeds)
    {
        var bestDist = float.MaxValue;
        var bestType = ECellType.Empty;

        for (var i = 0; i < seeds.Count; i++)
        {
            var dx = pos.x - seeds[i].pos.x;
            var dy = pos.y - seeds[i].pos.y;
            var distSq =  dx * dx + dy * dy;

            var weighted = distSq / seeds[i].weight;
            
            if(weighted >= bestDist) continue;
            
            bestDist = weighted;
            bestType = seeds[i].type;
        }

        return bestType;
    }

    private bool CheckSeedMinDistance(Vector2Int pos, List<(ECellType type, Vector2Int pos, float weight)> seeds)
    {
        foreach (var s in seeds)
        {
            var dis = Vector2Int.Distance(pos, s.pos);

            if (dis < minDistance) return false;
        }
        
        return true;
    }
    
    private List<int> ChoseRoadLine(int length)
    {
        var result = new List<int>();
        var cur = 0;

        while (cur < length)
        {
            result.Add(cur);
            cur += _prng.Next(roadMinGap, roadMaxGap + 1);
        }

        return result;
    }

    private void OnValidate()
    {
        if (roadMinGap > roadMaxGap)
        {
            roadMinGap = roadMaxGap;
        }

        if (liveDebugMode)
        {
            GenerateCity();   
        }
    }
}

[Serializable]
public class SeedConfig
{
    public ECellType Type;
    [Range(0.1f, 1f)] public float Weight = 0.1f;
}
