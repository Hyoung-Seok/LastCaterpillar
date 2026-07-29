using UnityEngine;
using System.Collections.Generic;
using Random = System.Random;

public class AreaGenerator
{
    private readonly Random _prng;
    private readonly CityLayout _cityLayout;
    private readonly AreaConfig _area;

    private readonly int _width;
    private readonly int _height;
    private readonly int _buildingBandDepth;
    
    public AreaGenerator(Random prng, CityLayout layout, AreaConfig areaConfig
        , int buildingBandDepth)
    {
        _prng = prng;
        _cityLayout = layout;
        _area = areaConfig;

        _buildingBandDepth = buildingBandDepth;
        
        _width = layout.Width;
        _height = layout.Height;
    }
    
    public void GenerateArea()
    {
        var allSeed = PlaceSeed();
        var warpOffset = new Vector4(_prng.Next(0, 10000), _prng.Next(0, 10000)
            ,_prng.Next(0, 10000),  _prng.Next(0, 10000));
        
        var sparsityOffset = new Vector2(_prng.Next(0, 10000), _prng.Next(0, 10000));

        for (var x = 0; x < _width; ++x)
        {
            for (var y = 0; y < _height; ++y)
            {
                if (_cityLayout.Cells[x, y] is ECellType.Road or ECellType.CatWalk) continue;
                if (_cityLayout.NearRoadDirection(x, y, _buildingBandDepth, _prng) == null) continue;

                var s = Mathf.PerlinNoise((x + sparsityOffset.x) * _area.SparsityScale,
                    (y + sparsityOffset.y) * _area.SparsityScale);
                s = Mathf.Clamp01(s);
                
                if(s < _area.Sparsity) continue;

                var p = WarpCell(x, y, warpOffset);
                _cityLayout.Cells[x, y] = FindNearestSeedType(p, allSeed);
            }
        }
    }

    private Vector2 WarpCell(int x, int y, Vector4 warpOffset)
    {
        var wx = (Mathf.PerlinNoise(x * _area.Frequency + warpOffset.x, y * _area.Frequency + warpOffset.y) - 0.5f) *
                 _area.Strength;
        var wy = (Mathf.PerlinNoise(x * _area.Frequency + warpOffset.w, y * _area.Frequency + warpOffset.z) - 0.5f) *
                 _area.Strength;
        
        return new Vector2(wx + x, wy + y);
    }

    private List<(ECellType type, Vector2Int pos, float weight)> PlaceSeed()
    {
        var result = new List<(ECellType type, Vector2Int pos, float weight)>();
        
        foreach (var s in _area.SeedConfigs)
        {
            Vector2Int p;
            var attempts = 0;
            do
            {
                p = new Vector2Int(_prng.Next(0, _width), _prng.Next(0, _height));
                attempts++;
                
            } while (CheckSeedMinDistance(p, result) == false
                     && attempts < _area.MaxAttempts);
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

            if (dis < _area.MinDistance) return false;
        }
        
        return true;
    }
}