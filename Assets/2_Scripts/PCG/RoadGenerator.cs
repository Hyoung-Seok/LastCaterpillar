using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CityGenerator))]
public class RoadGenerator : MonoBehaviour
{
    [Header("Normal Road")] 
    [SerializeField] private List<GameObject> straightRoads;
    
    [Header("Wide Road")]
    [SerializeField] private List<GameObject> wideRoads;

    public void GenerateHorizontalRoad(List<(int pos, int width)> roads, 
        HashSet<int> verticalRoad, CityLayout layout)
    {
        foreach (var road in roads)
        {
            var baseRoadObj = road.width == 2 ? straightRoads : wideRoads;

            for (var x = 0; x < layout.Width; ++x)
            {
                // TODO : 나중에 교차로 처리, 여기서만 처리하면 Vertical에선 처리할 필요 없음?
                if(verticalRoad.Contains(x)) continue;

                var index = 0;
                for (var y = road.pos; y < road.pos + road.width; ++y)
                {
                    var pos = layout.ConvertCellPosToWorld(x, y);
                    var obj = Instantiate(baseRoadObj[index++], pos, Quaternion.identity, transform);
                }
            }
        }
    }

    public void GenerateVerticalRoad(List<(int pos, int width)> roads,
        HashSet<int> horizontalRoad, CityLayout layout)
    {
        foreach (var road in roads)
        {
            var baseRoadObj =  road.width == 2 ? straightRoads : wideRoads;

            for (var y = 0; y < layout.Height; ++y)
            {
                if(horizontalRoad.Contains(y)) continue;

                var index = 0;
                for (var x = road.pos; x < road.pos + road.width; ++x)
                {
                    var pos = layout.ConvertCellPosToWorld(x, y);
                    var obj = Instantiate(baseRoadObj[index++], pos, Quaternion.identity, transform);
                }
            }
        }
    }
}
