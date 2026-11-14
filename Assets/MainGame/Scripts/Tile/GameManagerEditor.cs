using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileGameManager))]
public class TileGameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TileGameManager manager = (TileGameManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Runtime Testing", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            GUI.backgroundColor = new Color(0.35f, 0.85f, 0.45f);
            if (GUILayout.Button("Restart Level (Respawn Tiles)", GUILayout.Height(32)))
            {
                manager.RestartLevel();
            }

            GUI.backgroundColor = new Color(0.45f, 0.75f, 1f);
            if (GUILayout.Button("Next Level", GUILayout.Height(26)))
            {
                manager.NextLevel();
            }

            GUI.backgroundColor = new Color(1f, 0.8f, 0.35f);
            if (GUILayout.Button("Force State = Playing (Unlock Input)", GUILayout.Height(24)))
            {
                manager.ForcePlayingState();
            }
            GUI.backgroundColor = Color.white;
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use the runtime Restart / Respawn buttons.", MessageType.Info);
        }
    }
}