using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CityGenerator))]
public class RoadGenerator : MonoBehaviour
{
    public Transform RoadParent => roadParent;
    
    [Header("Parent")]
    [SerializeField] private Transform roadParent;
    
    [Header("Road")] 
    [SerializeField] private GameObject horizontalSmallRoad;
    [SerializeField] private GameObject verticalSmallRoad;

    [Header("CrossRoad")] 
    [SerializeField] private GameObject crossRoadObj;
    [SerializeField] private GameObject horizontalCrossWalk;
    [SerializeField] private GameObject verticalCrossWalk;
    
    [Header("Wide Road")]
    [SerializeField] private GameObject horizontalWideRoad;
    [SerializeField] private GameObject verticalWideRoad;
    
    [Header("CatWalk")]
    [SerializeField] private GameObject horizontalCatWalk;
    [SerializeField] private GameObject verticalCatWalk;
    [SerializeField] private GameObject cornerCatWalk;
    
    public void GenerateRoad(List<(int pos, int width)> roads, 
        HashSet<int> crossRoad, bool isVertical, CityLayout layout)
    {
        foreach (var road in roads)
        {
            var baseRoadObj = GetRoadType(isVertical, road.width);
            var length = isVertical ? layout.Height : layout.Width;
                     
            for (var l = 0; l < length; ++l)
            {
                if(crossRoad.Contains(l)) continue;
                
                var pos = isVertical ? layout.ConvertCellPosToWorld(road.pos, l) : 
                    layout.ConvertCellPosToWorld(l,  road.pos);
                var rot = isVertical ? Quaternion.identity : Quaternion.Euler(0, 90, 0);

                if (IsCrossWalk(l, length, road.width, crossRoad))
                {
                    var crossWalk = isVertical
                        ? Instantiate(verticalCrossWalk, pos, rot, roadParent)
                        : Instantiate(horizontalCrossWalk, pos, rot, roadParent);
                    
                    continue;
                }
                
                var obj = Instantiate(baseRoadObj, pos, rot, roadParent);
            }
        }
    }

    public void GenerateCrossRoad(List<(int pos, int width)> horizontal, 
        List<(int pos, int width)> vertical, int cellSize)
    {
        var roadWidth = CityGenerator.NormalRoadWidth;
        
        foreach (var h in horizontal)
        {
            foreach (var v in vertical)
            {
                var colStart = v.pos;
                var colEnd = v.pos + v.width;
                var rowStart = h.pos;
                var rowEnd = h.pos + h.width;

                for (var cx = colStart; cx < colEnd; cx += roadWidth)
                {
                    for (var cy = rowStart; cy < rowEnd; cy += roadWidth)
                    {
                        var pos = CityLayout.ConvertCellPosToWorld(cx, cy, cellSize);
                        Instantiate(crossRoadObj, pos, Quaternion.identity, roadParent);
                    }
                }
            }
        }
    }

    public void GenerateCatWalk(CityLayout cityLayout)
    {
        var width = cityLayout.Width;
        var height = cityLayout.Height;

        var table = new Dictionary<int, (GameObject prefab, float yRot)>(15);
        table[1] = (horizontalCatWalk, 0);
        table[4] = (horizontalCatWalk, 0);
        table[2] = (verticalCatWalk, 0);
        table[8] = (verticalCatWalk, 0);
        table[6] = (cornerCatWalk, 0);      // right down
        table[12] = (cornerCatWalk, 90);    // left down
        table[9] = (cornerCatWalk, 180);    // left top
        table[3] = (cornerCatWalk, 270);   // right top
        table[5] = (horizontalCatWalk, 0);
        table[10] = (verticalCatWalk, 0);
        
        for (var x = 0; x < width; ++x)
        {
            for (var y = 0; y < height; ++y)
            {
                if(cityLayout.Cells[x, y] != ECellType.CatWalk) continue;
                
                var mask = GetRoadMask(x, y, cityLayout);
                var pos = cityLayout.ConvertCellPosToWorld(x, y);
                
                if (table.TryGetValue(mask, out var tile))
                {
                    Instantiate(tile.prefab, pos, Quaternion.Euler(0, tile.yRot, 0), roadParent);
                }
            }
        }
    }

    private bool IsRoad(int x, int y, CityLayout layout)
    {
        if(x < 0 || x >= layout.Width || y < 0 || y >= layout.Height) return false;
        
        return layout.Cells[x, y] is ECellType.Road;
    }

    private int GetRoadMask(int x, int y, CityLayout layout)
    {
        var m = 0;

        if (IsRoad(x, y + 1, layout)) m |= 1;   // up
        if (IsRoad(x + 1, y, layout)) m |= 2;   // right
        if (IsRoad(x, y - 1, layout)) m |= 4;   // down
        if (IsRoad(x - 1, y, layout)) m |= 8;   // left

        return m;
    }

    private GameObject GetRoadType(bool isVertical, int width)
    {
        if (width == 2)
            return isVertical ? verticalSmallRoad : horizontalSmallRoad;

        return isVertical ? verticalWideRoad : horizontalWideRoad;
    }

    private bool IsCrossWalk(int index, int length, int width, HashSet<int> crossRoad)
    {
        if (index + 1 > length || index - 1 < 0 || width != 2) return false;
        return crossRoad.Contains(index - 1) || crossRoad.Contains(index + 1);
    }
}
