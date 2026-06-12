using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CityGenerator))]
public class CityBuilder : MonoBehaviour
{
    private CityAssetLoader _assetLoader;
    private bool[,] _isVisited;
    private System.Random _rng;

    public void GenerateBuilding(CityLayout layout, int depth)
    {
        DestroyBuilding();
        
        _assetLoader ??= new CityAssetLoader();
        _rng = new System.Random(layout.Seed);
        var footPrints = BuildFootPrints(layout, depth);

        foreach (var bData in footPrints)
        {
            var obj = _assetLoader.GetRandom(bData.Type, bData.Size, _rng);
            if(obj == null) continue;

            var w = bData.Size.x;
            var h = bData.Size.y;

            var originWorld = layout.ConvertCellPosToWorld(bData.Origin.x, bData.Origin.y);
            var offset = new Vector3((w - 1) * 0.5f * layout.CellSize,
                obj.transform.position.y,
                (h - 1) * 0.5f * layout.CellSize);
            
            var center = originWorld + offset;
            var facing = bData.Facing ?? Vector2Int.up;
            var rot = Quaternion.LookRotation(new Vector3(facing.x, 0, facing.y));
            
            Instantiate(obj, center, rot, transform);
        }
    }
    
    public void DestroyBuilding()
    {
        if (transform.childCount == 0) return;

        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    public void UpdateAssetData()
    {
        _assetLoader = null;
    }

    private List<BData> BuildFootPrints(CityLayout layout, int depth)
    {
        var result = new List<BData>();
        _isVisited = new bool[layout.Width, layout.Height];

        for (var x = 0; x < layout.Width; x++)
        {
            for (var y = 0; y < layout.Height; y++)
            {
                if(_isVisited[x, y]) continue;
                
                var type = layout.Cells[x, y];
                if(type is ECellType.Road or ECellType.Empty or ECellType.CatWalk) continue;

                var facing = layout.NearRoadDirection(x, y, depth, _rng);
                var size = CalculateBuildingSize(x, y, facing, layout);
                
                if(size == Vector2Int.zero) continue;
                
                result.Add(new BData(new Vector2Int(x, y), size, type, facing));
            }
        }

        return result;
    }

    private Vector2Int CalculateBuildingSize(int x, int y, Vector2Int? facing, CityLayout layout)
    {
        var curType = layout.Cells[x, y];
        var wantVertical = (facing != null && facing.Value.x != 0);
        
        var candidateSizes = _assetLoader.GetPossibleSize(curType);
        var possibleSize = new List<Vector2Int>();

        foreach (var size in candidateSizes)
        {
            var oriented = OrientSize(size, wantVertical);
            if (IsFit(x, y, oriented, curType, layout))
            {
                possibleSize.Add(oriented);
            }
        }

        if (possibleSize.Count == 0)
        {
            _isVisited[x, y] = true;
            return Vector2Int.zero;
        }
        
        var chosen = possibleSize[_rng.Next(possibleSize.Count)];
        
        for (var dx = 0; dx < chosen.x; ++dx)
        {
            for (var dy = 0; dy < chosen.y; ++dy)
            {
                _isVisited[x + dx, y + dy] = true;
            }
        }

        return chosen;
    }

    private Vector2Int OrientSize(Vector2Int size, bool wantVertical)
    {
        if (size.x == size.y) return size;
        var isVertical = size.y > size.x;
        
        return isVertical == wantVertical ? size : new Vector2Int(size.y, size.x);
    }

    private bool IsFit(int x, int y, Vector2Int size, ECellType curType, CityLayout layout)
    {
        for (var dx = 0; dx < size.x; ++dx)
        {
            for (var dy = 0; dy < size.y; ++dy)
            {
                var cx = x + dx;
                var cy = y + dy;

                if (cx >= layout.Width || cy >= layout.Height || cx < 0 || cy < 0) return false;
                if (layout.Cells[cx, cy] != curType) return false;
                if (_isVisited[cx, cy]) return false;
            }
        }
        
        return true;
    }
}

public class BData
{
    public Vector2Int Origin;
    public Vector2Int Size;
    public ECellType Type;
    public Vector2Int? Facing;

    public BData(Vector2Int origin, Vector2Int size, ECellType type, Vector2Int? facing)
    {
        Origin = origin;
        Size = size;
        Type = type;
        Facing = facing;
    }
}
