using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class CityGenerator : MonoBehaviour
{
    [Header("City Config")] 
    [SerializeField] private int seed;
    [SerializeField, Min(1)] private int width;
    [SerializeField, Min(2)] private int height;
    [SerializeField, Min(1)] private int cellSize;

    [Header("Road Config")] 
    [SerializeField, Min(1)] private int roadMinGap;
    [SerializeField, Min(1)] private int roadMaxGap;

    [Header("Voronoi")] 
    [SerializeField] private List<ECellType> seedTypes;
    [SerializeField] private int minDistance = 10;
    [SerializeField] private int maxAttempts = 10;
    [SerializeField, Min(1)] private int buildingBandDepth = 2;
    
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

        for (var x = 0; x < width; ++x)
        {
            for (var y = 0; y < height; ++y)
            {
                if(CityLayout.Cells[x, y] == ECellType.Road) continue;
                if(CityLayout.NearRoadDirection(x,y, buildingBandDepth, _prng) == null) continue;

                CityLayout.Cells[x, y] = FindNearestSeedType(new Vector2Int(x, y), allSeed);
            }
        }
    }

    private List<(ECellType type, Vector2Int pos)> PlaceSeed()
    {
        var result = new List<(ECellType type, Vector2Int pos)>();
        
        foreach (var type in seedTypes)
        {
            Vector2Int p;
            var attempts = 0;
            do
            {
                p = new Vector2Int(_prng.Next(0, width), _prng.Next(0, height));
                attempts++;
                
            } while (CheckSeedMinDistance(p, result) == false
                     && attempts < maxAttempts);
            result.Add((type, p));
        }

        return result;
    }

    private ECellType FindNearestSeedType(Vector2Int pos, List<(ECellType type, Vector2Int pos)> seeds)
    {
        var bestDist = float.MaxValue;
        var bestType = ECellType.Empty;

        foreach (var s in seeds)
        {
            var dx = pos.x - s.pos.x;
            var dy = pos.y - s.pos.y;
            var distance =  dx * dx + dy * dy;
            
            if(distance >= bestDist) continue;
            
            bestDist = distance;
            bestType = s.type;
        }

        return bestType;
    }

    private bool CheckSeedMinDistance(Vector2Int pos, List<(ECellType type, Vector2Int pos)> seeds)
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
    }
}
