using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CityGenerator))]
public class CityGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        
        var generator = (CityGenerator)target;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate"))
        {
            generator.GenerateCity();
            SceneView.RepaintAll();
        }
        
        GUILayout.Space(5);

        if (GUILayout.Button("DestroyBuilding"))
        {
            generator.DestroyBuilding();
            SceneView.RepaintAll();
        }
        
        GUILayout.Space(5);

        if (GUILayout.Button("Apply Asset Data"))
        {
            generator.UpdateAssetData();
            SceneView.RepaintAll();
        }

        if (EditorGUI.EndChangeCheck() && generator.LiveDebugMode)
        {
            EditorApplication.delayCall += () => generator.GenerateCity();
        }
    }
}
