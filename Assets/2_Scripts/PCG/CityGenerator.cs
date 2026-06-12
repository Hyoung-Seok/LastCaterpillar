using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class CityGenerator : MonoBehaviour
{
    [Header("Debug")] 
    [SerializeField] private bool liveDebugMode;
    [SerializeField] private bool isGenerateBuilding;
    
    [Header("Components")]
    [SerializeField] private CityBuilder cityBuilder;
    
    [Header("City Config")] 
    [SerializeField] private int seed;
    [SerializeField, Min(1)] private int width;
    [SerializeField, Min(2)] private int height;
    [SerializeField, Min(1)] private int cellSize;
    [SerializeField, Min(0)] private int roadStartDepth;

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
    public bool LiveDebugMode => liveDebugMode;
    
    private Random _prng;

    public void GenerateCity()
    {
        _prng = new Random(seed);
        CityLayout = new CityLayout(seed, width, height, cellSize);
        
        GenerateRoad();
        GenerateArea();
        
        if(isGenerateBuilding)
            cityBuilder.GenerateBuilding(CityLayout, buildingBandDepth);
    }

    public void DestroyBuilding()
    {
        cityBuilder.DestroyBuilding();
    }

    public void UpdateAssetData()
    {
        cityBuilder.UpdateAssetData();
    }

    private void GenerateRoad()
    {
        var horizontal = ChoseRoadLine(height);
        var vertical = ChoseRoadLine(width);

        foreach (var h in horizontal)
        {
            for (var w = 0; w < h.width; ++w)
            {
                // y는 증가시킨 채 고정
                var yy = h.startPos + w;
                if(yy >= height) continue;

                // x는 순회하며 도로로 지정
                for (var x = 0; x < width; ++x)
                {
                    CityLayout.Cells[x, yy] = ECellType.Road;
                }
            }
        }

        foreach (var v in vertical)
        {
            for (var w = 0; w < v.width; ++w)
            {
                var xx = v.startPos + w;
                if(xx >= width) continue;

                for (var y = 0; y < height; ++y)
                {
                    CityLayout.Cells[xx, y] = ECellType.Road;
                }
            }
        }

        foreach (var h in horizontal)
        {
            for (var x = 0; x < width; ++x)
            {
                var up = h.startPos + h.width;
                var down = h.startPos - 1;
                
                if (up < height && CityLayout.Cells[x, up] == ECellType.Empty) CityLayout.Cells[x, up] = ECellType.CatWalk;
                if (down >= 0 && CityLayout.Cells[x, down] == ECellType.Empty) CityLayout.Cells[x, down] = ECellType.CatWalk;
            }
        }

        foreach (var v in vertical)
        {
            for (var y = 0; y < height; ++y)
            {
                var left = v.startPos - 1;
                var right = v.startPos + v.width;

                if (left >= 0 && CityLayout.Cells[left, y] == ECellType.Empty)
                    CityLayout.Cells[left, y] = ECellType.CatWalk;
                if (right < width && CityLayout.Cells[right, y] == ECellType.Empty)
                    CityLayout.Cells[right, y] = ECellType.CatWalk;
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
                if (CityLayout.Cells[x, y] is ECellType.Road or ECellType.CatWalk) continue;
                if (CityLayout.NearRoadDirection(x, y, buildingBandDepth, _prng) == null) continue;

                var s = Mathf.PerlinNoise((x + sparsityOffset.x) * sparsityScale,
                    (y + sparsityOffset.y) * sparsityScale);
                s = Mathf.Clamp01(s);
                
                if(s < sparsity) continue;

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
    
    private List<(int startPos, int width)> ChoseRoadLine(int length)
    {
        var result = new List<(int startPos, int width)>();
        var cur = roadStartDepth;

        while (cur < length - roadStartDepth)
        {
            var roadWidth = _prng.Next(0, 2) == 0 ? 2 : 4;

            if (cur + roadWidth >= length)
            {
                roadWidth = 2;
            }
            
            result.Add((cur, roadWidth));
            cur += _prng.Next(roadMinGap, roadMaxGap + 1) + roadWidth;
        }

        return result;
    }

    private void OnValidate()
    {
        if (roadMinGap > roadMaxGap)
        {
            roadMinGap = roadMaxGap;
        }
    }
}

[Serializable]
public class SeedConfig
{
    public ECellType Type;
    [Range(0.1f, 1f)] public float Weight = 0.1f;
}
