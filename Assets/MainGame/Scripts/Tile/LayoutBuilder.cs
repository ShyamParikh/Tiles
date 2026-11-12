using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MahjongLevelData))]
public class MahjongLevelEditor : Editor
{
    private int currentLayerIndex = 0;
    private bool isPainting = false;
    private bool paintValue = true;

    public override void OnInspectorGUI()
    {
        MahjongLevelData level = (MahjongLevelData)target;
        serializedObject.Update();

        EditorGUILayout.Space();
        GUILayout.Label("Level Dimensions (For New Layers)", EditorStyles.boldLabel);
        level.defaultWidth = EditorGUILayout.IntSlider("Default Width", level.defaultWidth, 1, 30);
        level.defaultHeight = EditorGUILayout.IntSlider("Default Height", level.defaultHeight, 1, 30);
        EditorGUILayout.Space();
        
        // --- Layer Controls ---
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Add Layer", GUILayout.Height(30)))
        {
            Undo.RecordObject(level, "Add Layer");
            level.AddLayer();
            currentLayerIndex = level.layers.Count - 1;
            EditorUtility.SetDirty(level);
        }
        
        GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
        if (GUILayout.Button("Remove Top Layer", GUILayout.Height(30)) && level.layers.Count > 0)
        {
            Undo.RecordObject(level, "Remove Layer");
            level.layers.RemoveAt(level.layers.Count - 1);
            if (currentLayerIndex >= level.layers.Count) currentLayerIndex = Mathf.Max(0, level.layers.Count - 1);
            EditorUtility.SetDirty(level);
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        if (level.layers.Count == 0)
        {
            EditorGUILayout.HelpBox("Add a layer to start designing your board.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        
        // --- Layer Selection Tabs ---
        GUILayout.Label("Select Layer to Edit", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        for (int i = 0; i < level.layers.Count; i++)
        {
            GUI.backgroundColor = (currentLayerIndex == i) ? new Color(0.3f, 0.7f, 1f) : Color.white;
            if (GUILayout.Button($"Layer {i + 1}"))
            {
                currentLayerIndex = i;
            }
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        MahjongLayer currentLayer = level.layers[currentLayerIndex];
        
        EditorGUILayout.Space();
        GUILayout.Label($"Editing Layer {currentLayerIndex + 1} ({currentLayer.width}x{currentLayer.height})", EditorStyles.boldLabel);
        currentLayer.offset = EditorGUILayout.Vector2Field("Positional Shift (X, Y)", currentLayer.offset);
        EditorGUILayout.Space();
        
        DrawGrid(currentLayer, level);
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGrid(MahjongLayer layer, MahjongLevelData level)
    {
        Event e = Event.current;
        float cellSize = 25f;

        GUILayout.BeginVertical("box");
        for (int y = 0; y < layer.height; y++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            for (int x = 0; x < layer.width; x++)
            {
                Rect cellRect = GUILayoutUtility.GetRect(cellSize, cellSize, GUILayout.ExpandWidth(false));
                bool isActive = layer.GetCell(x, y);

                // Colors mimicking your reference UI
                Color cellColor = isActive ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.3f, 0.3f, 0.3f);
                EditorGUI.DrawRect(cellRect, cellColor);
                
                Handles.color = Color.black;
                Handles.DrawWireCube(cellRect.center, cellRect.size);

                // Draw/Erase logic
                if (e.type == EventType.MouseDown && cellRect.Contains(e.mousePosition))
                {
                    Undo.RecordObject(level, "Paint Tile");
                    isPainting = true;
                    paintValue = !isActive;
                    layer.SetCell(x, y, paintValue);
                    EditorUtility.SetDirty(level);
                    e.Use();
                }
                else if (e.type == EventType.MouseDrag && isPainting && cellRect.Contains(e.mousePosition))
                {
                    if (layer.GetCell(x, y) != paintValue)
                    {
                        Undo.RecordObject(level, "Paint Tile");
                        layer.SetCell(x, y, paintValue);
                        EditorUtility.SetDirty(level);
                    }
                    e.Use();
                }
                else if (e.type == EventType.MouseUp)
                {
                    isPainting = false;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        
        if (e.type == EventType.MouseDrag) Repaint();
    }
}