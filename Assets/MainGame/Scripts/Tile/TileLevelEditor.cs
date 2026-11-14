using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileLevelData))]
public class TileLevelEditor : Editor
{
    private int currentLayerIndex = 0;
    private bool isPainting = false;
    private bool paintValue = true;
    private bool showFacePool = true;
    private bool showTilePool = true;

    private Vector2 gridScrollPos = Vector2.zero;
    private float gridCellSize = 24f;

    private const float TabMinWidth = 75f;

    public override void OnInspectorGUI()
    {
        TileLevelData level = (TileLevelData)target;
        level.MigrateLegacyData();

        EditorGUILayout.Space();
        DrawTileAndFaceSetup(level);

        EditorGUILayout.Space();
        GUILayout.Label("Level Dimensions (For New Layers)", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        int newWidth = EditorGUILayout.IntSlider("Default Width", level.defaultWidth, 1, 30);
        int newHeight = EditorGUILayout.IntSlider("Default Height", level.defaultHeight, 1, 30);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(level, "Change Default Dimensions");
            level.defaultWidth = newWidth;
            level.defaultHeight = newHeight;
            EditorUtility.SetDirty(level);
        }

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

        // --- Layer Selection Tabs ---
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

        EditorGUI.BeginChangeCheck();
        Vector2 newOffset = EditorGUILayout.Vector2Field("Positional Shift (X, Y)", currentLayer.offset);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(level, "Change Layer Offset");
            currentLayer.offset = newOffset;
            EditorUtility.SetDirty(level);
        }

        DrawLayerShiftControls(currentLayer, level);

        EditorGUILayout.Space();
        DrawAllocationSummary(level);

        EditorGUILayout.Space();
        DrawGrid(currentLayer, level);
    }

    // ---------------- Tile Pool & Face Pool Setup ----------------

    private void DrawTileAndFaceSetup(TileLevelData level)
    {
        GUILayout.Label("Library & Pools", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        TileLibrary newLibrary = (TileLibrary)EditorGUILayout.ObjectField("Tile Library", level.library, typeof(TileLibrary), false);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(level, "Change Tile Library");
            level.library = newLibrary;
            EditorUtility.SetDirty(level);
        }

        if (level.library == null)
        {
            EditorGUILayout.HelpBox("Assign a Tile Library to configure the Tile Pool and Face Pool.", MessageType.Info);
            return;
        }

        List<string> allTypeIds = level.library.GetTileTypeIds();

        // ================= 1. TILE POOL =================
        EditorGUILayout.Space(4);
        string tilePoolHeader = level.tilePoolEntries.Count == 0
            ? "Tile Pool (Auto: first tile type)"
            : $"Tile Pool ({level.tilePoolEntries.Count} selected)";
        showTilePool = EditorGUILayout.Foldout(showTilePool, tilePoolHeader, true, EditorStyles.foldoutHeader);

        if (showTilePool)
        {
            EditorGUILayout.BeginVertical("box");
            if (allTypeIds.Count == 0)
            {
                EditorGUILayout.HelpBox("The Tile Library has no tile types with a prefab assigned.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("Tick tile types to include. Count = 0 fills remaining board tiles automatically.", EditorStyles.miniLabel);

                for (int i = 0; i < allTypeIds.Count; i++)
                {
                    string typeId = allTypeIds[i];
                    TilePoolEntry existing = level.GetTileEntry(typeId);
                    bool inPool = existing != null;

                    EditorGUILayout.BeginHorizontal();

                    bool toggled = EditorGUILayout.ToggleLeft(typeId, inPool, GUILayout.Width(130f));
                    if (toggled && !inPool)
                    {
                        Undo.RecordObject(level, "Add Tile Type to Pool");
                        level.tilePoolEntries.Add(new TilePoolEntry { tileTypeId = typeId, tileCount = 0 });
                        EditorUtility.SetDirty(level);
                        existing = level.GetTileEntry(typeId);
                    }
                    else if (!toggled && inPool)
                    {
                        Undo.RecordObject(level, "Remove Tile Type from Pool");
                        level.tilePoolEntries.Remove(existing);
                        EditorUtility.SetDirty(level);
                        existing = null;
                    }

                    if (existing != null)
                    {
                        GUILayout.Label("Tiles:", GUILayout.Width(36f));
                        int updatedCount = EditorGUILayout.IntField(existing.tileCount, GUILayout.Width(50f));
                        if (GUILayout.Button("-3", GUILayout.Width(28f))) updatedCount -= 3;
                        if (GUILayout.Button("+3", GUILayout.Width(28f))) updatedCount += 3;

                        updatedCount = Mathf.Max(0, updatedCount);
                        if (updatedCount != existing.tileCount)
                        {
                            Undo.RecordObject(level, "Change Tile Type Count");
                            existing.tileCount = updatedCount;
                            EditorUtility.SetDirty(level);
                        }

                        GUILayout.Label(existing.tileCount == 0 ? "(Auto)" : $"({existing.tileCount / 3} sets)", EditorStyles.miniLabel);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (level.tilePoolEntries.Count > 0 && GUILayout.Button("Clear Tile Pool (Use Default)"))
                {
                    Undo.RecordObject(level, "Clear Tile Pool");
                    level.tilePoolEntries.Clear();
                    EditorUtility.SetDirty(level);
                }
            }
            EditorGUILayout.EndVertical();
        }

        // ================= 2. FACE POOL =================
        EditorGUILayout.Space(4);
        string facePoolHeader = level.facePoolEntries.Count == 0
            ? "Face Pool (Auto: all faces)"
            : $"Face Pool ({level.facePoolEntries.Count} selected)";
        showFacePool = EditorGUILayout.Foldout(showFacePool, facePoolHeader, true, EditorStyles.foldoutHeader);

        if (showFacePool)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Set exact Tile Count (multiples of 3) or 0 = Auto. Optionally lock a face to a specific Tile Type.", EditorStyles.miniLabel);

            // Build dropdown options for locking a face to a tile type
            string[] lockOptions = new string[allTypeIds.Count + 1];
            lockOptions[0] = "Any (Tile Pool)";
            for (int t = 0; t < allTypeIds.Count; t++) lockOptions[t + 1] = allTypeIds[t];

            foreach (TileFaceEntry face in level.library.Faces)
            {
                if (face == null || face.sprite == null) continue;

                FacePoolEntry existing = level.GetFaceEntry(face.id);
                bool inPool = existing != null;
                string label = string.IsNullOrEmpty(face.displayName) ? $"Face {face.id}" : $"{face.id}: {face.displayName}";

                EditorGUILayout.BeginHorizontal();

                bool toggled = EditorGUILayout.ToggleLeft(label, inPool, GUILayout.Width(130f));
                if (toggled && !inPool)
                {
                    Undo.RecordObject(level, "Add Face to Pool");
                    level.facePoolEntries.Add(new FacePoolEntry { faceId = face.id, tileCount = 0, lockedTileTypeId = "" });
                    EditorUtility.SetDirty(level);
                    existing = level.GetFaceEntry(face.id);
                }
                else if (!toggled && inPool)
                {
                    Undo.RecordObject(level, "Remove Face from Pool");
                    level.facePoolEntries.Remove(existing);
                    EditorUtility.SetDirty(level);
                    existing = null;
                }

                if (existing != null)
                {
                    GUILayout.Label("Tiles:", GUILayout.Width(36f));
                    int updatedCount = EditorGUILayout.IntField(existing.tileCount, GUILayout.Width(45f));
                    if (GUILayout.Button("-3", GUILayout.Width(26f))) updatedCount -= 3;
                    if (GUILayout.Button("+3", GUILayout.Width(26f))) updatedCount += 3;

                    updatedCount = Mathf.Max(0, updatedCount);
                    if (updatedCount != existing.tileCount)
                    {
                        Undo.RecordObject(level, "Change Face Tile Count");
                        existing.tileCount = updatedCount;
                        EditorUtility.SetDirty(level);
                    }

                    // Lock to specific Tile Type dropdown
                    int currentLockIdx = 0;
                    if (!string.IsNullOrEmpty(existing.lockedTileTypeId))
                    {
                        int found = allTypeIds.IndexOf(existing.lockedTileTypeId);
                        if (found >= 0) currentLockIdx = found + 1;
                    }

                    int newLockIdx = EditorGUILayout.Popup(currentLockIdx, lockOptions, GUILayout.MinWidth(95f));
                    string newLockId = newLockIdx == 0 ? "" : allTypeIds[newLockIdx - 1];
                    if (newLockId != existing.lockedTileTypeId)
                    {
                        Undo.RecordObject(level, "Change Face Locked Tile Type");
                        existing.lockedTileTypeId = newLockId;
                        EditorUtility.SetDirty(level);
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (existing != null && existing.tileCount > 0 && existing.tileCount % TileMergeData.TilesPerMerge != 0)
                {
                    EditorGUILayout.HelpBox($"'{label}' has {existing.tileCount} tiles — must be a multiple of {TileMergeData.TilesPerMerge} (e.g. 3, 6, 9).", MessageType.Warning);
                }
            }

            if (level.facePoolEntries.Count > 0 && GUILayout.Button("Clear Face Pool (Use All Faces Auto)"))
            {
                Undo.RecordObject(level, "Clear Face Pool");
                level.facePoolEntries.Clear();
                EditorUtility.SetDirty(level);
            }

            EditorGUILayout.EndVertical();
        }

        // ================= 3. GENERAL RANDOMIZATION SETTINGS =================
        EditorGUILayout.Space(4);
        EditorGUI.BeginChangeCheck();
        int newMaxFaces = EditorGUILayout.IntField(
            new GUIContent("Max Different Faces (Auto)", "0 = use every face in the pool."), level.maxDifferentFaces);
        int newSeed = EditorGUILayout.IntField(
            new GUIContent("Seed", "Fixes random generation when Randomize Placement Every Load is off. 0 = automatic."), level.seed);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(level, "Change Pool Randomization");
            level.maxDifferentFaces = Mathf.Max(0, newMaxFaces);
            level.seed = newSeed;
            EditorUtility.SetDirty(level);
        }
    }

    // ---------------- Allocation Summary ----------------

    private void DrawAllocationSummary(TileLevelData level)
    {
        int paintedCount = level.CountTiles();
        int per = TileMergeData.TilesPerMerge;
        int usableCount = (paintedCount / per) * per;

        int fixedFaceTotal = 0;
        int autoFaceCount = 0;
        foreach (FacePoolEntry entry in level.facePoolEntries)
        {
            if (entry == null) continue;
            if (entry.tileCount > 0) fixedFaceTotal += entry.tileCount;
            else autoFaceCount++;
        }

        int fixedTileTotal = 0;
        int autoTileCount = 0;
        foreach (TilePoolEntry entry in level.tilePoolEntries)
        {
            if (entry == null) continue;
            if (entry.tileCount > 0) fixedTileTotal += entry.tileCount;
            else autoTileCount++;
        }

        if (paintedCount % per != 0)
        {
            EditorGUILayout.HelpBox(
                $"Painted Grid: {paintedCount} tiles (not a multiple of {per}; last {paintedCount % per} will be skipped -> {usableCount} usable).",
                MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox(
                $"Painted Grid: {paintedCount} tiles ({paintedCount / per} sets of {per})\n" +
                $"Face Pool: {fixedFaceTotal} fixed tiles + {Mathf.Max(0, usableCount - fixedFaceTotal)} auto tiles ({autoFaceCount} auto faces)\n" +
                $"Tile Pool: {fixedTileTotal} fixed tiles + {Mathf.Max(0, usableCount - fixedTileTotal)} auto tiles ({autoTileCount} auto tile types)",
                MessageType.Info);
        }

        if (fixedFaceTotal > usableCount)
        {
            EditorGUILayout.HelpBox($"Face Pool fixed counts ({fixedFaceTotal}) exceed usable board tiles ({usableCount})! Paint more grid cells or reduce face counts.", MessageType.Error);
        }

        if (fixedTileTotal > usableCount)
        {
            EditorGUILayout.HelpBox($"Tile Pool fixed counts ({fixedTileTotal}) exceed usable board tiles ({usableCount})! Paint more grid cells or reduce tile counts.", MessageType.Error);
        }
    }

    // ---------------- Layer Shift ----------------

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

    // ---------------- Scrollable Horizontal & Vertical Grid Painting ----------------

    private void DrawGrid(TileLayer layer, TileLevelData level)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Grid Painting", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.Label("Cell Size:", GUILayout.Width(58f));
        gridCellSize = EditorGUILayout.Slider(gridCellSize, 14f, 36f, GUILayout.Width(130f));
        EditorGUILayout.EndHorizontal();

        Event e = Event.current;
        float viewWidth = EditorGUIUtility.currentViewWidth - 36f;
        float contentWidth = layer.width * (gridCellSize + 2f) + 16f;
        float contentHeight = layer.height * (gridCellSize + 2f) + 28f;
        float scrollHeight = Mathf.Clamp(contentHeight, 120f, 520f);

        gridScrollPos = EditorGUILayout.BeginScrollView(
            gridScrollPos,
            true,
            contentHeight > scrollHeight,
            GUILayout.Height(scrollHeight));

        GUILayout.BeginVertical("box", GUILayout.MinWidth(Mathf.Max(viewWidth - 10f, contentWidth)));
        for (int y = 0; y < layer.height; y++)
        {
            GUILayout.BeginHorizontal();
            if (contentWidth < viewWidth) GUILayout.FlexibleSpace();

            for (int x = 0; x < layer.width; x++)
            {
                Rect cellRect = GUILayoutUtility.GetRect(gridCellSize, gridCellSize, GUILayout.ExpandWidth(false));
                bool isActive = layer.GetCell(x, y);

                Color cellColor = isActive ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.3f, 0.3f, 0.3f);
                EditorGUI.DrawRect(cellRect, cellColor);

                Handles.color = Color.black;
                Handles.DrawWireCube(cellRect.center, cellRect.size);

                if (e.type == EventType.MouseDown && e.button == 0 && cellRect.Contains(e.mousePosition))
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
            }

            if (contentWidth < viewWidth) GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        EditorGUILayout.EndScrollView();

        if (e.type == EventType.MouseUp)
            isPainting = false;

        if (e.type == EventType.MouseDrag)
            Repaint();
    }
}