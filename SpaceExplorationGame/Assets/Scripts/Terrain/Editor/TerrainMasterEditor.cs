using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainMaster))]
public class TerrainMasterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainMaster terrainMaster = (TerrainMaster)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate")) {
            terrainMaster.DrawMapInEditor();
        }
    }
}