using System.Collections.Generic;
using UnityEngine;

public class CityLayout
{
    public int Seed { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int CellSize { get; private set; }
    public ECellType[,] Cells;
    
    public CityLayout(int seed, int width, int height, int cellSize)
    {
        Seed = seed;
        Width = width;
        Height = height;
        CellSize = cellSize;
        
        Cells = new ECellType[width, height];
    }

    public Vector3 ConvertCellPosToWorld(int x, int y)
    {
        return new Vector3(x * CellSize, 0f, y * CellSize);
    }
    
    public Vector2Int? NearRoadDirection(int x, int y, int depth, System.Random rng)
    {
        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        for (var i = 1; i <= depth; ++i)
        {
            var candidates = new List<Vector2Int>();

            foreach (var dir in dirs)
            {
                var cx = x + dir.x * i;
                var cy = y + dir.y * i;
                
                if(cx < 0 || cy < 0 || cx >= Width || cy >= Height) continue;
                if(Cells[cx, cy] is ECellType.Road or ECellType.CatWalk) candidates.Add(dir);
            }
            
            if(candidates.Count > 0)
                return candidates[rng.Next(candidates.Count)];
        }

        return null;
    }
}
