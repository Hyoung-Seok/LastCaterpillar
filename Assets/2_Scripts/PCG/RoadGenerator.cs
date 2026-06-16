using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CityGenerator))]
public class RoadGenerator : MonoBehaviour
{
    [Header("Normal Road")] 
    [SerializeField] private List<GameObject> straightRoads;
    
    [Header("Wide Road")]
    [SerializeField] private List<GameObject> wideRoads;

    public void GenerateRoad(List<(int pos, int width)> roads, 
        HashSet<int> crossRoad, bool isVertical, CityLayout layout)
    {
        foreach (var road in roads)
        {
            var baseRoadObj = road.width == 2 ? straightRoads : wideRoads;
            var length = isVertical ? layout.Height : layout.Width;

            for (var l = 0; l < length; ++l)
            {
                if(crossRoad.Contains(l)) continue;

                var index = 0;
                for (var w = road.pos; w < road.pos + road.width; ++w)
                {
                    var x = isVertical ? w : l;
                    var y = isVertical ? l : w;
                    
                    var pos = layout.ConvertCellPosToWorld(x, y);
                    Instantiate(baseRoadObj[index++], pos, Quaternion.identity, transform);
                }
            }
        }
    }
}
