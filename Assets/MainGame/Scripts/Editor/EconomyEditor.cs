/*using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileEconomyManager))]
public class TileEconomyManagerEditor : Editor
{
    public override bool RequiresConstantRepaint()
    {
        return Application.isPlaying;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(10);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use the currency debugger.", MessageType.Info);
            return;
        }

        TileEconomyManager manager = (TileEconomyManager)target;
        manager.EnsureInitialized();
        manager.DrawDebugPanel();
    }
}*/