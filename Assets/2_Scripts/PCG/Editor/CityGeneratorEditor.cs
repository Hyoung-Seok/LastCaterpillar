using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CityGenerator))]
public class CityGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        var generator = (CityGenerator)target;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate"))
        {
            generator.GenerateCity();
            SceneView.RepaintAll();
        }
    }
}
