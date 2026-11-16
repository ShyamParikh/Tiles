using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileLevelData))]
public class TileLevelEditor : Editor
{
    private int currentLayer;
    private bool isPainting;
    private bool paintValue = true;
    private bool showDefaultLook = true;
    private bool showFacePool = true;
    private bool showTilePool = true;
    private Vector2 gridScroll;
    private float cellSize = 24f;

    public override void OnInspectorGUI()
    {
        TileLevelData level = (TileLevelData)target;
        level.MigrateLegacyData();

        EditorGUILayout.Space();
        DrawSetup(level);
        EditorGUILayout.Space();
        DrawLayout(level);

        if (level.layers.Count == 0)
        {
            EditorGUILayout.HelpBox("Add a layer to start designing your board.", MessageType.Info);
            return;
        }

        DrawLayerEditor(level);
    }

    private void Edit(string undoName, Action change)
    {
        Undo.RecordObject(target, undoName);
        change();
        EditorUtility.SetDirty(target);
    }

    // ---------------- Library, look and pools ----------------

    private void DrawSetup(TileLevelData level)
    {
        GUILayout.Label("Library & Pools", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        TileLibrary library = (TileLibrary)EditorGUILayout.ObjectField("Tile Library", level.library, typeof(TileLibrary), false);
        if (EditorGUI.EndChangeCheck()) Edit("Change Tile Library", () => level.library = library);

        EditorGUILayout.Space(4);
        DrawDefaultLook(level);

        if (level.library == null)
        {
            EditorGUILayout.HelpBox("Assign a Tile Library to configure the Tile Pool and Face Pool.", MessageType.Info);
            return;
        }

        List<string> typeIds = level.library.GetTileTypeIds();
        EditorGUILayout.Space(4);
        DrawTilePool(level, typeIds);
        EditorGUILayout.Space(4);
        DrawFacePool(level, typeIds);

        EditorGUILayout.Space(4);
        EditorGUI.BeginChangeCheck();
        int maxFaces = EditorGUILayout.IntField(new GUIContent("Max Different Faces (Auto)", "0 = use every face in the pool."), level.maxDifferentFaces);
        int seed = EditorGUILayout.IntField(new GUIContent("Seed", "Fixes random generation when Randomize Placement Every Load is off. 0 = automatic."), level.seed);
        if (EditorGUI.EndChangeCheck()) Edit("Change Pool Randomization", () => { level.maxDifferentFaces = Mathf.Max(0, maxFaces); level.seed = seed; });
    }

    private void DrawDefaultLook(TileLevelData level)
    {
        showDefaultLook = EditorGUILayout.Foldout(showDefaultLook, "Level Default Look (Color & Material)", true, EditorStyles.foldoutHeader);
        if (!showDefaultLook) return;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Priority: Face > Tile Type > Level Default > the Tile prefab's own look.", EditorStyles.miniLabel);
        DrawLookRow("Change Default Board Look", "Board", 0f,
            ref level.overrideDefaultTileColor, ref level.defaultTileColor,
            ref level.overrideDefaultTileMaterial, ref level.defaultTileMaterial);
        DrawLookRow("Change Default Tray Look", "In Tray", 0f,
            ref level.overrideDefaultTrayColor, ref level.defaultTrayColor,
            ref level.overrideDefaultTrayMaterial, ref level.defaultTrayMaterial);

        EditorGUI.BeginChangeCheck();
        int colorSlot = EditorGUILayout.IntField(new GUIContent("Color Material Slot", "-1 = tint ALL slots."), level.targetMaterialSlot);
        int materialSlot = EditorGUILayout.IntField(new GUIContent("Material Slot", "-1 = replace ALL slots."), level.materialTargetSlot);
        if (EditorGUI.EndChangeCheck())
            Edit("Change Material Slots", () => { level.targetMaterialSlot = Mathf.Max(-1, colorSlot); level.materialTargetSlot = Mathf.Max(-1, materialSlot); });

        EditorGUILayout.EndVertical();
    }

    /// <summary>The board row and the tray row of one pool entry.</summary>
    private void DrawEntryLooks(PoolEntry entry)
    {
        DrawLookRow("Change Board Look", "Board", 18f, ref entry.overrideColor, ref entry.tileColor, ref entry.overrideMaterial, ref entry.tileMaterial);
        DrawLookRow("Change Tray Look", "In Tray", 18f, ref entry.trayOverrideColor, ref entry.trayColor, ref entry.trayOverrideMaterial, ref entry.trayMaterial);
    }

    /// <summary>One row: Label  [x] Color [picker]  [x] Material [object field].</summary>
    private void DrawLookRow(string undoName, string label, float indent,
        ref bool overrideColor, ref Color color, ref bool overrideMaterial, ref Material material)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(indent);
        GUILayout.Label(label, EditorStyles.miniLabel, GUILayout.Width(46f));

        bool newOverrideColor = EditorGUILayout.ToggleLeft("Color", overrideColor, GUILayout.Width(58f));
        Color newColor = color;
        using (new EditorGUI.DisabledScope(!newOverrideColor)) newColor = EditorGUILayout.ColorField(color, GUILayout.Width(60f));

        GUILayout.Space(8f);

        bool newOverrideMaterial = EditorGUILayout.ToggleLeft("Material", overrideMaterial, GUILayout.Width(68f));
        Material newMaterial = material;
        using (new EditorGUI.DisabledScope(!newOverrideMaterial))
            newMaterial = (Material)EditorGUILayout.ObjectField(material, typeof(Material), false, GUILayout.MinWidth(80f));

        EditorGUILayout.EndHorizontal();
        if (!EditorGUI.EndChangeCheck()) return;

        Undo.RecordObject(target, undoName);
        overrideColor = newOverrideColor;
        color = newColor;
        overrideMaterial = newOverrideMaterial;
        material = newMaterial;
        EditorUtility.SetDirty(target);
    }

    private void DrawTilePool(TileLevelData level, List<string> typeIds)
    {
        string header = level.tilePoolEntries.Count == 0 ? "Tile Pool (Auto: first tile type)" : $"Tile Pool ({level.tilePoolEntries.Count} selected)";
        showTilePool = EditorGUILayout.Foldout(showTilePool, header, true, EditorStyles.foldoutHeader);
        if (!showTilePool) return;

        EditorGUILayout.BeginVertical("box");
        if (typeIds.Count == 0)
        {
            EditorGUILayout.HelpBox("The Tile Library has no tile types with a prefab assigned.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.LabelField("Tick tile types to include. Count = 0 fills remaining board tiles automatically.", EditorStyles.miniLabel);
            foreach (string id in typeIds)
            {
                EditorGUILayout.BeginHorizontal();
                TilePoolEntry entry = DrawToggle(level.tilePoolEntries, level.GetTileEntry(id), id, () => new TilePoolEntry { tileTypeId = id });
                if (entry != null) DrawCount(entry);
                EditorGUILayout.EndHorizontal();

                if (entry != null) DrawEntryLooks(entry);
            }

            if (level.tilePoolEntries.Count > 0 && GUILayout.Button("Clear Tile Pool (Use Default)"))
                Edit("Clear Tile Pool", level.tilePoolEntries.Clear);
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawFacePool(TileLevelData level, List<string> typeIds)
    {
        string header = level.facePoolEntries.Count == 0 ? "Face Pool (Auto: all faces)" : $"Face Pool ({level.facePoolEntries.Count} selected)";
        showFacePool = EditorGUILayout.Foldout(showFacePool, header, true, EditorStyles.foldoutHeader);
        if (!showFacePool) return;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Set exact Tile Count (multiples of 3) or 0 = Auto. Optionally lock a face to a Tile Type.", EditorStyles.miniLabel);

        string[] lockOptions = new string[typeIds.Count + 1];
        lockOptions[0] = "Any (Tile Pool)";
        typeIds.CopyTo(lockOptions, 1);

        foreach (TileFaceEntry face in level.library.Faces)
        {
            if (face == null || face.sprite == null) continue;

            string label = string.IsNullOrEmpty(face.displayName) ? $"Face {face.id}" : $"{face.id}: {face.displayName}";
            EditorGUILayout.BeginHorizontal();
            FacePoolEntry entry = DrawToggle(level.facePoolEntries, level.GetFaceEntry(face.id), label, () => new FacePoolEntry { faceId = face.id });
            if (entry != null)
            {
                DrawCount(entry);
                DrawLock(entry, typeIds, lockOptions);
            }
            EditorGUILayout.EndHorizontal();

            if (entry == null) continue;

            DrawEntryLooks(entry);
            if (entry.tileCount % TileRules.SetSize != 0)
                EditorGUILayout.HelpBox($"'{label}' has {entry.tileCount} tiles: it must be a multiple of {TileRules.SetSize}.", MessageType.Warning);
        }

        if (level.facePoolEntries.Count > 0 && GUILayout.Button("Clear Face Pool (Use All Faces Auto)"))
            Edit("Clear Face Pool", level.facePoolEntries.Clear);

        EditorGUILayout.EndVertical();
    }

    /// <summary>Draws the pool checkbox and adds / removes the entry when it changes.</summary>
    private T DrawToggle<T>(List<T> list, T entry, string label, Func<T> create) where T : class
    {
        bool on = EditorGUILayout.ToggleLeft(label, entry != null, GUILayout.Width(130f));

        if (on && entry == null)
        {
            T added = create();
            Edit("Add To Pool", () => list.Add(added));
            return added;
        }
        if (!on && entry != null)
        {
            T removed = entry;
            Edit("Remove From Pool", () => list.Remove(removed));
            return null;
        }
        return entry;
    }

    private void DrawCount(PoolEntry entry)
    {
        GUILayout.Label("Tiles:", GUILayout.Width(36f));
        int count = EditorGUILayout.IntField(entry.tileCount, GUILayout.Width(45f));
        if (GUILayout.Button("-3", GUILayout.Width(28f))) count -= 3;
        if (GUILayout.Button("+3", GUILayout.Width(28f))) count += 3;

        count = Mathf.Max(0, count);
        if (count != entry.tileCount) Edit("Change Tile Count", () => entry.tileCount = count);
        GUILayout.Label(entry.tileCount == 0 ? "(Auto)" : $"({entry.tileCount / TileRules.SetSize} sets)", EditorStyles.miniLabel);
    }

    private void DrawLock(FacePoolEntry entry, List<string> typeIds, string[] options)
    {
        int current = typeIds.IndexOf(entry.lockedTileTypeId) + 1;
        int chosen = EditorGUILayout.Popup(current, options, GUILayout.MinWidth(95f));
        if (chosen != current) Edit("Change Locked Tile Type", () => entry.lockedTileTypeId = chosen == 0 ? "" : typeIds[chosen - 1]);
    }

    // ---------------- Dimensions, stacking, layers ----------------

    private void DrawLayout(TileLevelData level)
    {
        GUILayout.Label("Level Dimensions (For New Layers)", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        int width = EditorGUILayout.IntSlider("Default Width", level.defaultWidth, 1, 30);
        int height = EditorGUILayout.IntSlider("Default Height", level.defaultHeight, 1, 30);
        if (EditorGUI.EndChangeCheck()) Edit("Change Default Dimensions", () => { level.defaultWidth = width; level.defaultHeight = height; });

        EditorGUILayout.Space();
        GUILayout.Label("Layer Stacking", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        float baseHeight = EditorGUILayout.FloatField(new GUIContent("Base Layer Height", "Height of the first layer, unless that layer overrides it."), level.baseLayerHeight);
        float shift = EditorGUILayout.FloatField(new GUIContent("Default Layer Shift", "Distance between a layer and the one below it, unless that layer overrides it."), level.defaultLayerShift);
        if (EditorGUI.EndChangeCheck()) Edit("Change Stacking Defaults", () => { level.baseLayerHeight = baseHeight; level.defaultLayerShift = shift; });

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Add Layer", GUILayout.Height(30)))
            Edit("Add Layer", () => { level.AddLayer(); currentLayer = level.layers.Count - 1; });

        GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
        if (GUILayout.Button("Remove Top Layer", GUILayout.Height(30)) && level.layers.Count > 0)
            Edit("Remove Layer", () => level.layers.RemoveAt(level.layers.Count - 1));
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLayerEditor(TileLevelData level)
    {
        EditorGUILayout.Space();
        GUILayout.Label("Select Layer to Edit", EditorStyles.boldLabel);

        string[] labels = new string[level.layers.Count];
        for (int i = 0; i < labels.Length; i++) labels[i] = $"Layer {i + 1}";

        int perRow = Mathf.Clamp(Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 30f) / 75f), 1, labels.Length);
        currentLayer = GUILayout.SelectionGrid(Mathf.Clamp(currentLayer, 0, labels.Length - 1), labels, perRow);
        TileLayer layer = level.layers[currentLayer];

        EditorGUILayout.Space();
        GUILayout.Label($"Editing Layer {currentLayer + 1} ({layer.width}x{layer.height})", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        Vector2 offset = EditorGUILayout.Vector2Field("Positional Shift (X, Y)", layer.offset);
        if (EditorGUI.EndChangeCheck()) Edit("Change Layer Offset", () => layer.offset = offset);

        DrawLayerShift(level, layer);
        EditorGUILayout.Space();
        DrawSummary(level);
        EditorGUILayout.Space();
        DrawGrid(level, layer);
    }

    private void DrawLayerShift(TileLevelData level, TileLayer layer)
    {
        bool isBase = currentLayer == 0;
        float fallback = isBase ? level.baseLayerHeight : level.defaultLayerShift;

        EditorGUI.BeginChangeCheck();
        bool overrideShift = EditorGUILayout.Toggle(
            new GUIContent(isBase ? "Override Base Height" : "Override Shift", "Use a custom value for this layer instead of the level default."),
            layer.overrideShift);

        float custom;
        using (new EditorGUI.DisabledScope(!overrideShift))
        {
            custom = EditorGUILayout.FloatField(
                new GUIContent(isBase ? "Base Height" : "Layer Shift", isBase ? "Height of the base layer." : "Distance above the previous layer. 0 = same height."),
                overrideShift ? layer.customShift : fallback);
        }

        if (EditorGUI.EndChangeCheck())
            Edit("Change Layer Shift", () => { layer.overrideShift = overrideShift; if (overrideShift) layer.customShift = custom; });

        EditorGUILayout.LabelField("Resulting Height", level.GetLayerStackPosition(currentLayer).ToString("0.###"));
    }

    private static void DrawSummary(TileLevelData level)
    {
        int per = TileRules.SetSize;
        int painted = level.CountTiles();
        int usable = painted / per * per;
        int faceFixed = FixedTotal(level.facePoolEntries, out int faceAuto);
        int tileFixed = FixedTotal(level.tilePoolEntries, out int tileAuto);

        if (painted % per != 0)
            EditorGUILayout.HelpBox($"Painted Grid: {painted} tiles (not a multiple of {per}; last {painted % per} will be skipped -> {usable} usable).", MessageType.Warning);
        else
            EditorGUILayout.HelpBox(
                $"Painted Grid: {painted} tiles ({painted / per} sets of {per})\n" +
                $"Face Pool: {faceFixed} fixed tiles + {Mathf.Max(0, usable - faceFixed)} auto tiles ({faceAuto} auto faces)\n" +
                $"Tile Pool: {tileFixed} fixed tiles + {Mathf.Max(0, usable - tileFixed)} auto tiles ({tileAuto} auto tile types)",
                MessageType.Info);

        if (faceFixed > usable) EditorGUILayout.HelpBox($"Face Pool fixed counts ({faceFixed}) exceed usable board tiles ({usable})!", MessageType.Error);
        if (tileFixed > usable) EditorGUILayout.HelpBox($"Tile Pool fixed counts ({tileFixed}) exceed usable board tiles ({usable})!", MessageType.Error);
    }

    private static int FixedTotal<T>(List<T> entries, out int autoCount) where T : PoolEntry
    {
        int total = 0;
        autoCount = 0;
        foreach (T entry in entries)
        {
            if (entry == null) continue;
            if (entry.tileCount > 0) total += entry.tileCount;
            else autoCount++;
        }
        return total;
    }

    // ---------------- Grid painting ----------------

    private void DrawGrid(TileLevelData level, TileLayer layer)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Grid Painting", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.Label("Cell Size:", GUILayout.Width(58f));
        cellSize = EditorGUILayout.Slider(cellSize, 14f, 36f, GUILayout.Width(130f));
        EditorGUILayout.EndHorizontal();

        Event e = Event.current;
        float viewWidth = EditorGUIUtility.currentViewWidth - 36f;
        float contentWidth = layer.width * (cellSize + 2f) + 16f;
        float contentHeight = layer.height * (cellSize + 2f) + 28f;
        float scrollHeight = Mathf.Clamp(contentHeight, 120f, 520f);

        gridScroll = EditorGUILayout.BeginScrollView(gridScroll, true, contentHeight > scrollHeight, GUILayout.Height(scrollHeight));
        GUILayout.BeginVertical("box", GUILayout.MinWidth(Mathf.Max(viewWidth - 10f, contentWidth)));

        for (int y = 0; y < layer.height; y++)
        {
            GUILayout.BeginHorizontal();
            if (contentWidth < viewWidth) GUILayout.FlexibleSpace();
            for (int x = 0; x < layer.width; x++) DrawCell(layer, x, y, e);
            if (contentWidth < viewWidth) GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        if (e.type == EventType.MouseUp) isPainting = false;
        if (e.type == EventType.MouseDrag) Repaint();
    }

    private void DrawCell(TileLayer layer, int x, int y, Event e)
    {
        Rect rect = GUILayoutUtility.GetRect(cellSize, cellSize, GUILayout.ExpandWidth(false));
        bool active = layer.GetCell(x, y);

        EditorGUI.DrawRect(rect, active ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.3f, 0.3f, 0.3f));
        Handles.color = Color.black;
        Handles.DrawWireCube(rect.center, rect.size);

        if (!rect.Contains(e.mousePosition)) return;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            isPainting = true;
            paintValue = !active;
            Edit("Paint Tile", () => layer.SetCell(x, y, paintValue));
            e.Use();
        }
        else if (e.type == EventType.MouseDrag && isPainting)
        {
            if (active != paintValue) Edit("Paint Tile", () => layer.SetCell(x, y, paintValue));
            e.Use();
        }
    }
}