using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileLevelData))]
public class TileLevelEditor : Editor
{
    private int currentLayerIndex = 0;
    private bool isPainting = false;
    private bool paintValue = true;

    // Minimum width of one layer tab. Tabs wrap onto new rows when they run out of space.
    private const float TabMinWidth = 75f;

    public override void OnInspectorGUI()
    {
        TileLevelData level = (TileLevelData)target;
        serializedObject.Update();

        EditorGUILayout.Space();
        GUILayout.Label("Level Dimensions (For New Layers)", EditorStyles.boldLabel);
        level.defaultWidth = EditorGUILayout.IntSlider("Default Width", level.defaultWidth, 1, 30);
        level.defaultHeight = EditorGUILayout.IntSlider("Default Height", level.defaultHeight, 1, 30);

        EditorGUILayout.Space();
        GUILayout.Label("Layer Stacking", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        float newBaseHeight = EditorGUILayout.FloatField(
            new GUIContent("Base Layer Height", "Height of the first layer, unless that layer overrides it."),
            level.baseLayerHeight);
        float newDefaultShift = EditorGUILayout.FloatField(
            new GUIContent("Default Layer Shift", "Distance between a layer and the one below it, unless that layer overrides it."),
            level.defaultLayerShift);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(level, "Change Level Stacking Defaults");
            level.baseLayerHeight = newBaseHeight;
            level.defaultLayerShift = newDefaultShift;
            EditorUtility.SetDirty(level);
        }
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

        // --- Layer Selection Tabs (wrap onto new rows instead of getting cut off) ---
        GUILayout.Label("Select Layer to Edit", EditorStyles.boldLabel);

        string[] layerLabels = new string[level.layers.Count];
        for (int i = 0; i < layerLabels.Length; i++) layerLabels[i] = $"Layer {i + 1}";

        float availableWidth = EditorGUIUtility.currentViewWidth - 30f;
        int tabsPerRow = Mathf.Clamp(Mathf.FloorToInt(availableWidth / TabMinWidth), 1, layerLabels.Length);

        if (currentLayerIndex >= level.layers.Count) currentLayerIndex = level.layers.Count - 1;
        currentLayerIndex = GUILayout.SelectionGrid(currentLayerIndex, layerLabels, tabsPerRow);

        TileLayer currentLayer = level.layers[currentLayerIndex];

        EditorGUILayout.Space();
        GUILayout.Label($"Editing Layer {currentLayerIndex + 1} ({currentLayer.width}x{currentLayer.height})", EditorStyles.boldLabel);
        currentLayer.offset = EditorGUILayout.Vector2Field("Positional Shift (X, Y)", currentLayer.offset);

        DrawLayerShiftControls(currentLayer, level);

        EditorGUILayout.Space();

        DrawGrid(currentLayer, level);
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawLayerShiftControls(TileLayer layer, TileLevelData level)
    {
        bool isBase = currentLayerIndex == 0;
        string overrideLabel = isBase ? "Override Base Height" : "Override Shift";
        string valueLabel = isBase ? "Base Height" : "Layer Shift";
        float defaultValue = isBase ? level.baseLayerHeight : level.defaultLayerShift;

        EditorGUI.BeginChangeCheck();
        bool newOverride = EditorGUILayout.Toggle(
            new GUIContent(overrideLabel, "Use a custom value for this layer instead of the level default."),
            layer.overrideShift);

        float newCustom = layer.customShift;
        using (new EditorGUI.DisabledScope(!newOverride))
        {
            newCustom = EditorGUILayout.FloatField(
                new GUIContent(valueLabel, isBase
                    ? "Height of the base layer."
                    : "Distance above the previous layer. 0 = same height as the layer below."),
                newOverride ? layer.customShift : defaultValue);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(level, "Change Layer Shift");
            layer.overrideShift = newOverride;
            if (newOverride) layer.customShift = newCustom;
            EditorUtility.SetDirty(level);
        }

        float totalHeight = level.GetLayerStackPosition(currentLayerIndex);
        EditorGUILayout.LabelField("Resulting Height", totalHeight.ToString("0.###"));
    }

    private void DrawGrid(TileLayer layer, TileLevelData level)
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