using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CityGenerator))]
public class RoadGenerator : MonoBehaviour
{
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
                        ? Instantiate(verticalCrossWalk, pos, rot, transform)
                        : Instantiate(horizontalCrossWalk, pos, rot, transform);
                    
                    continue;
                }
                
                var obj = Instantiate(baseRoadObj, pos, rot, transform);
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
                        Instantiate(crossRoadObj, pos, Quaternion.identity, transform);
                    }
                }
            }
        }
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
