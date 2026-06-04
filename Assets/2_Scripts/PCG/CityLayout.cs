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
}
