using System;
using System.Collections.Generic;
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
    
    public CityLayout CityLayout { get; private set; }
    private Random _prng;

    public void GenerateCity()
    {
        _prng = new Random(seed);
        CityLayout = new CityLayout(seed, width, height, cellSize);
        
        GenerateRoad();
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
