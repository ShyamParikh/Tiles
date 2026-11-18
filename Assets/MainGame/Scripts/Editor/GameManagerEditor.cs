/*
 * TileGameManagerEditor
 * ---------------------
 * An optional custom inspector for TileGameManager. It draws the normal inspector, then a "Runtime Testing"
 * section with three coloured buttons (Restart Level, Next Level, Force State = Playing) that only work in
 * Play Mode. TileGameManager already has the same three as Odin buttons, so you can delete this script if you
 * prefer those. This script must sit in an Editor folder.
 */
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileGameManager))]
public class TileGameManagerEditor : Editor
{
    #region Unity Callbacks

    // Draws the default inspector and the runtime testing buttons.
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TileGameManager manager = (TileGameManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Runtime Testing", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (Button("Restart Level (Respawn Tiles)", new Color(0.35f, 0.85f, 0.45f), 32f)) manager.RestartLevel();
            if (Button("Next Level", new Color(0.45f, 0.75f, 1f), 26f)) manager.NextLevel();
            if (Button("Force State = Playing (Unlock Input)", new Color(1f, 0.8f, 0.35f), 24f)) manager.ForcePlayingState();
        }

        if (!Application.isPlaying) EditorGUILayout.HelpBox("Enter Play Mode to use the runtime buttons.", MessageType.Info);
    }

    #endregion

    #region Helpers

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