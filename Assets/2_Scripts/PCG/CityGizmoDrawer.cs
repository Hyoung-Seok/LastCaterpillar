using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityGizmoDrawer : MonoBehaviour
{
    [Header("Gizmos")]
    [SerializeField] private bool isDrawing;
    [SerializeField, Range(0.5f, 1f)] private float fillRatio = 0.9f;
    [SerializeField] private float height = 0.1f;

    [Header("Type Color")] 
    [SerializeField] private List<TypeColor> typeColors;

    private CityGenerator _generator;

    private void OnDrawGizmos()
    {
        if (isDrawing == false) return;

        if (_generator == null)
        {
            _generator = GetComponent<CityGenerator>();
        }
        
        var layout = _generator.CityLayout;
        if (layout == null) return;
        
        DrawCall(layout);
    }

    private void DrawCall(CityLayout layout)
    {
        var size = layout.CellSize * fillRatio;
        var cubeSize = new Vector3(size, height, size);

        for (var x = 0; x < layout.Width; ++x)
        {
            for (var y = 0; y < layout.Height; ++y)
            {
                var cell = layout.Cells[x, y];
                if(cell == ECellType.Empty) continue;
                
                Gizmos.color = typeColors.FirstOrDefault(c => c.Type == cell)
                    ?.Color ?? Color.magenta;

                var pos = layout.ConvertCellPosToWorld(x, y);
                Gizmos.DrawCube(pos, cubeSize);
            }
        }
    }
}

[Serializable]
public class TypeColor
{
    public ECellType Type;
    public Color Color;
}
