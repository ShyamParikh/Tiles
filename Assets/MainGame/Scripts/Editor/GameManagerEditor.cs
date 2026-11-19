/*
 * TileGameManagerEditor
 * ---------------------
 * An optional custom inspector for TileGameManager. It draws the normal inspector, then a "Runtime Testing"
 * section with three coloured buttons (Restart Level, Next Level, Force State = Playing) that only work in
 * Play Mode, and a "Progress" section with a Reset To Level 1 button that works in and outside Play Mode
 * (in Play Mode it also reloads level 1, outside it only resets the saved progress). This script must sit in
 * an Editor folder.
 */
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileGameManager))]
public class TileGameManagerEditor : Editor
{
    #region Unity Callbacks

    // Draws the default inspector, the runtime testing buttons and the progress button.
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TileGameManager manager = (TileGameManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Runtime Testing", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (Button("Restart Level (Respawn Tiles)", new Color(0.35f, 0.85f, 0.45f), 32f))
            {
                manager.RestartLevel();
            }
            if (Button("Next Level", new Color(0.45f, 0.75f, 1f), 26f))
            {
                manager.NextLevel();
            }
            if (Button("Force State = Playing (Unlock Input)", new Color(1f, 0.8f, 0.35f), 24f))
            {
                manager.ForcePlayingState();
            }
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use the runtime buttons.", MessageType.Info);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Progress", EditorStyles.boldLabel);

        if (Button("Reset To Level 1", new Color(1f, 0.45f, 0.4f), 26f))
        {
            ResetProgress(manager);
        }
    }

    #endregion

    #region Helpers

    // In Play Mode it resets and reloads level 1. Outside Play Mode it only resets the saved progress.
    private static void ResetProgress(TileGameManager manager)
    {
        if (Application.isPlaying)
        {
            manager.ResetToFirstLevel();
        }
        else
        {
            manager.ResetProgress();
        }
    }

    // A coloured button that reports whether it was clicked.
    private static bool Button(string label, Color color, float height)
    {
        GUI.backgroundColor = color;
        bool clicked = GUILayout.Button(label, GUILayout.Height(height));
        GUI.backgroundColor = Color.white;
        return clicked;
    }

    #endregion
}