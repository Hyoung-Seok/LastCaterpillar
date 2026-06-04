using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class CityGenerator : MonoBehaviour
{
    [Header("City Config")] 
    [SerializeField] private int seed;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int cellSize;

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
        var row = ChoseRoadLine(width);
        var col = ChoseRoadLine(height);

        foreach (var x in row)
        {
            for (var y = 0; y < height; ++y)
            {
                CityLayout.Cells[x, y] = ECellType.Road;
            }
        }

        foreach (var y in col)
        {
            for (var x = 0; x < width; ++x)
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
            cur += _prng.Next(roadMinGap, roadMaxGap);
        }

        return result;
    }
}
