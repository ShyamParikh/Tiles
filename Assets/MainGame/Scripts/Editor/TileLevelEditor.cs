/*
 * TileLevelEditor
 * ---------------
 * The custom inspector for TileLevelData. It draws, from top to bottom: the tile library field, the level
 * default look (color and material for the board and the tray), the tile type pool, the face pool, the random
 * settings, the saved seeds (tick the ones to use, add a random one or save the board that was spawned last,
 * and switch seed use on or off), the grid and stacking settings, the layer tabs, and the grid where you paint
 * tiles with the mouse (click to flip a cell, drag to paint). Every change goes through Edit(), so it is
 * undoable and saved, and Draw() applies a control's new value only when it really changed. It warns when
 * counts are not multiples of 3 or are bigger than the board. This script must sit in an Editor folder.
 */
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileLevelData))]
public class TileLevelEditor : Editor
{
    #region Private Variables

    private int layerIndex;
    private bool painting;
    private bool paintValue = true;
    private bool showLook = true, showTypes = true, showFaces = true, showSeeds = true;
    private Vector2 gridScroll;
    private float cellSize = 24f;

    #endregion

    #region Unity Callbacks

    // Draws the whole inspector.
    public override void OnInspectorGUI()
    {
        TileLevelData level = (TileLevelData)target;
        level.MigrateLegacyData();

        GUILayout.Label("Library & Pools", EditorStyles.boldLabel);
        Draw("Change Tile Library", level.library,
            v => (TileLibrary)EditorGUILayout.ObjectField("Tile Library", v, typeof(TileLibrary), false), v => level.library = v);

        DrawDefaultLook(level);

        if (level.library == null)
        {
            EditorGUILayout.HelpBox("Assign a Tile Library to set up the tile and face pools.", MessageType.Info);
        }
        else
        {
            List<string> typeIds = level.library.GetTileTypeIds();
            DrawTypePool(level, typeIds);
            DrawFacePool(level, typeIds);
            Draw("Change Max Faces", level.maxDifferentFaces,
                v => EditorGUILayout.IntField(new GUIContent("Max Different Faces (auto)", "0 = use every face in the pool."), v), v => level.maxDifferentFaces = Mathf.Max(0, v));
            Draw("Change Seed", level.seed,
                v => EditorGUILayout.IntField(new GUIContent("Seed", "Keeps random picks the same each load. 0 = automatic."), v), v => level.seed = v);
            DrawSeeds(level);
        }

        DrawLayout(level);
        if (level.layers.Count > 0) DrawLayerEditor(level);
        else EditorGUILayout.HelpBox("Add a layer to start designing your board.", MessageType.Info);
    }

    #endregion

    #region Editing Helpers

    // Wraps a change so it is undoable and saved.
    private void Edit(string undoName, Action change)
    {
        Undo.RecordObject(target, undoName);
        change();
        EditorUtility.SetDirty(target);
    }

    // Draws one control and only applies the new value when it really changed.
    private void Draw<T>(string undoName, T current, Func<T, T> control, Action<T> apply)
    {
        EditorGUI.BeginChangeCheck();
        T value = control(current);
        if (EditorGUI.EndChangeCheck()) Edit(undoName, () => apply(value));
    }

    #endregion

    #region Looks

    // The level-wide look: board row, tray row and the material slots.
    private void DrawDefaultLook(TileLevelData level)
    {
        showLook = EditorGUILayout.Foldout(showLook, "Level Default Look (Color & Material)", true, EditorStyles.foldoutHeader);
        if (!showLook) return;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Priority: Face > Tile Type > Level Default > the prefab's own look.", EditorStyles.miniLabel);
        DrawLookRow("Board", 0f, ref level.overrideDefaultTileColor, ref level.defaultTileColor, ref level.overrideDefaultTileMaterial, ref level.defaultTileMaterial);
        DrawLookRow("In Tray", 0f, ref level.overrideDefaultTrayColor, ref level.defaultTrayColor, ref level.overrideDefaultTrayMaterial, ref level.defaultTrayMaterial);
        Draw("Change Color Slot", level.targetMaterialSlot,
            v => EditorGUILayout.IntField(new GUIContent("Color Material Slot", "-1 = tint every slot."), v), v => level.targetMaterialSlot = Mathf.Max(-1, v));
        Draw("Change Material Slot", level.materialTargetSlot,
            v => EditorGUILayout.IntField(new GUIContent("Material Slot", "-1 = replace every slot."), v), v => level.materialTargetSlot = Mathf.Max(-1, v));
        EditorGUILayout.EndVertical();
    }

    // The board row and the tray row of one pool entry.
    private void DrawEntryLooks(PoolEntry e)
    {
        DrawLookRow("Board", 18f, ref e.overrideColor, ref e.tileColor, ref e.overrideMaterial, ref e.tileMaterial);
        DrawLookRow("In Tray", 18f, ref e.trayOverrideColor, ref e.trayColor, ref e.trayOverrideMaterial, ref e.trayMaterial);
    }

    // One line: Label  [x] Color [picker]  [x] Material [object].
    private void DrawLookRow(string label, float indent, ref bool useColor, ref Color color, ref bool useMaterial, ref Material material)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(indent);
        GUILayout.Label(label, EditorStyles.miniLabel, GUILayout.Width(46f));

        bool newUseColor = EditorGUILayout.ToggleLeft("Color", useColor, GUILayout.Width(58f));
        Color newColor = color;
        using (new EditorGUI.DisabledScope(!newUseColor)) newColor = EditorGUILayout.ColorField(color, GUILayout.Width(60f));

        GUILayout.Space(8f);

        bool newUseMaterial = EditorGUILayout.ToggleLeft("Material", useMaterial, GUILayout.Width(68f));
        Material newMaterial = material;
        using (new EditorGUI.DisabledScope(!newUseMaterial))
            newMaterial = (Material)EditorGUILayout.ObjectField(material, typeof(Material), false, GUILayout.MinWidth(80f));

        EditorGUILayout.EndHorizontal();
        if (!EditorGUI.EndChangeCheck()) return;

        Undo.RecordObject(target, "Change Look");
        useColor = newUseColor;
        color = newColor;
        useMaterial = newUseMaterial;
        material = newMaterial;
        EditorUtility.SetDirty(target);
    }

    #endregion

    #region Pools

    // The tile type pool: tick the types the level uses and give them counts and looks.
    private void DrawTypePool(TileLevelData level, List<string> typeIds)
    {
        string title = level.tilePoolEntries.Count == 0 ? "Tile Pool (auto: first tile type)" : $"Tile Pool ({level.tilePoolEntries.Count} selected)";
        showTypes = EditorGUILayout.Foldout(showTypes, title, true, EditorStyles.foldoutHeader);
        if (!showTypes) return;

        EditorGUILayout.BeginVertical("box");
        if (typeIds.Count == 0) EditorGUILayout.HelpBox("The Tile Library has no tile types with a prefab.", MessageType.Warning);

        foreach (string id in typeIds)
        {
            EditorGUILayout.BeginHorizontal();
            TilePoolEntry entry = DrawToggle(level.tilePoolEntries, level.GetTileEntry(id), id, () => new TilePoolEntry { tileTypeId = id });
            if (entry != null) DrawCount(entry);
            EditorGUILayout.EndHorizontal();

            if (entry != null) DrawEntryLooks(entry);
        }

        if (level.tilePoolEntries.Count > 0 && GUILayout.Button("Clear Tile Pool (use the first type)"))
            Edit("Clear Tile Pool", level.tilePoolEntries.Clear);
        EditorGUILayout.EndVertical();
    }

    // The face pool: tick the faces, give them counts, lock them to a tile type, and set their looks.
    private void DrawFacePool(TileLevelData level, List<string> typeIds)
    {
        string title = level.facePoolEntries.Count == 0 ? "Face Pool (auto: all faces)" : $"Face Pool ({level.facePoolEntries.Count} selected)";
        showFaces = EditorGUILayout.Foldout(showFaces, title, true, EditorStyles.foldoutHeader);
        if (!showFaces) return;

        string[] lockOptions = new string[typeIds.Count + 1];
        lockOptions[0] = "Any (Tile Pool)";
        typeIds.CopyTo(lockOptions, 1);

        EditorGUILayout.BeginVertical("box");
        foreach (TileFaceEntry face in level.library.Faces)
        {
            if (face == null || face.sprite == null) continue;

            string label = string.IsNullOrEmpty(face.displayName) ? $"Face {face.id}" : $"{face.id}: {face.displayName}";
            EditorGUILayout.BeginHorizontal();
            FacePoolEntry entry = DrawToggle(level.facePoolEntries, level.GetFaceEntry(face.id), label, () => new FacePoolEntry { faceId = face.id });
            if (entry != null)
            {
                DrawCount(entry);
                int current = typeIds.IndexOf(entry.lockedTileTypeId) + 1;
                int chosen = EditorGUILayout.Popup(current, lockOptions, GUILayout.MinWidth(95f));
                if (chosen != current) Edit("Lock Tile Type", () => entry.lockedTileTypeId = chosen == 0 ? "" : typeIds[chosen - 1]);
            }
            EditorGUILayout.EndHorizontal();

            if (entry == null) continue;
            DrawEntryLooks(entry);
            if (entry.tileCount % TileRules.SetSize != 0)
                EditorGUILayout.HelpBox($"'{label}' has {entry.tileCount} tiles. It must be a multiple of {TileRules.SetSize}.", MessageType.Warning);
        }

        if (level.facePoolEntries.Count > 0 && GUILayout.Button("Clear Face Pool (use all faces)"))
            Edit("Clear Face Pool", level.facePoolEntries.Clear);
        EditorGUILayout.EndVertical();
    }

    // The tick box of a pool row. Ticking adds the entry, unticking removes it.
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
            Edit("Remove From Pool", () => list.Remove(entry));
            return null;
        }
        return entry;
    }

    // The tile count field with -3 / +3 buttons.
    private void DrawCount(PoolEntry entry)
    {
        GUILayout.Label("Tiles:", GUILayout.Width(36f));
        int count = EditorGUILayout.IntField(entry.tileCount, GUILayout.Width(45f));
        if (GUILayout.Button("-3", GUILayout.Width(28f))) count -= 3;
        if (GUILayout.Button("+3", GUILayout.Width(28f))) count += 3;

        count = Mathf.Max(0, count);
        if (count != entry.tileCount) Edit("Change Tile Count", () => entry.tileCount = count);
        GUILayout.Label(entry.tileCount == 0 ? "(auto)" : $"({entry.tileCount / TileRules.SetSize} sets)", EditorStyles.miniLabel);
    }

    #endregion

    #region Saved Seeds

    // The saved seeds of the level: tick the ones to use, add or save new ones.
    private void DrawSeeds(TileLevelData level)
    {
        string title = $"Saved Seeds ({level.GetSelectedSeeds().Count} of {level.savedSeeds.Count} in use)";
        showSeeds = EditorGUILayout.Foldout(showSeeds, title, true, EditorStyles.foldoutHeader);
        if (!showSeeds)
        {
            return;
        }

        EditorGUILayout.BeginVertical("box");

        EditorGUI.BeginChangeCheck();
        GUIContent useLabel = new GUIContent("Use Saved Seeds (instead of random generation)", "On = every load takes the next ticked seed and cycles through them. Off = the normal random generation.");
        bool useSeeds = EditorGUILayout.ToggleLeft(useLabel, level.useSavedSeeds);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Change Use Saved Seeds");
            level.useSavedSeeds = useSeeds;
            EditorUtility.SetDirty(target);
        }

        int removeIndex = -1;
        for (int i = 0; i < level.savedSeeds.Count; i++)
        {
            if (DrawSeedRow(level.savedSeeds[i]))
            {
                removeIndex = i;
            }
        }
        if (removeIndex >= 0)
        {
            Undo.RecordObject(target, "Remove Seed");
            level.savedSeeds.RemoveAt(removeIndex);
            EditorUtility.SetDirty(target);
        }

        if (level.savedSeeds.Count == 0)
        {
            EditorGUILayout.HelpBox("No seeds yet. Add a random one, or spawn the level and press Save Current Layout.", MessageType.Info);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Random Seed"))
        {
            AddSeed(level, TileSeedEntry.CreateRandom("Seed " + (level.savedSeeds.Count + 1)));
        }

        using (new EditorGUI.DisabledScope(!CanSaveCurrentLayout(level)))
        {
            if (GUILayout.Button("Save Current Layout"))
            {
                TileSeedEntry current = TileVisualSpawner.LastLayout.Copy();
                current.label = "Seed " + (level.savedSeeds.Count + 1);
                current.use = true;
                AddSeed(level, current);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Tick All"))
        {
            SetAllSeedsUsed(level, true);
        }
        if (GUILayout.Button("Untick All"))
        {
            SetAllSeedsUsed(level, false);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Numbers: content / face / placement. Rnd = random amount of sets per face.", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
    }

    // One seed: tick, name, its numbers, Rnd flag and a remove button. Returns true when it should be removed.
    private bool DrawSeedRow(TileSeedEntry entry)
    {
        bool remove = false;

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        bool use = EditorGUILayout.Toggle(entry.use, GUILayout.Width(18f));
        string label = EditorGUILayout.TextField(entry.label, GUILayout.MinWidth(80f));
        GUILayout.Label($"{entry.contentSeed} / {entry.faceSeed} / {entry.placementSeed}", EditorStyles.miniLabel);
        bool randomAmounts = EditorGUILayout.ToggleLeft(new GUIContent("Rnd", "Random amount of sets per face."), entry.randomFaceAmounts, GUILayout.Width(42f));
        if (GUILayout.Button("X", GUILayout.Width(22f)))
        {
            remove = true;
        }
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck() && !remove)
        {
            Undo.RecordObject(target, "Change Seed");
            entry.use = use;
            entry.label = label;
            entry.randomFaceAmounts = randomAmounts;
            EditorUtility.SetDirty(target);
        }
        return remove;
    }

    // Adds a seed to the level (undoable and saved).
    private void AddSeed(TileLevelData level, TileSeedEntry entry)
    {
        Undo.RecordObject(target, "Add Seed");
        level.savedSeeds.Add(entry);
        EditorUtility.SetDirty(target);
    }

    // Ticks or unticks every seed.
    private void SetAllSeedsUsed(TileLevelData level, bool used)
    {
        Undo.RecordObject(target, "Tick Seeds");
        foreach (TileSeedEntry entry in level.savedSeeds)
        {
            if (entry != null)
            {
                entry.use = used;
            }
        }
        EditorUtility.SetDirty(target);
    }

    // True when the board spawned last belongs to this level and is not saved yet.
    private static bool CanSaveCurrentLayout(TileLevelData level)
    {
        if (TileVisualSpawner.LastLevel != level || TileVisualSpawner.LastLayout == null)
        {
            return false;
        }

        foreach (TileSeedEntry entry in level.savedSeeds)
        {
            if (entry != null && entry.SameLayout(TileVisualSpawner.LastLayout))
            {
                return false;
            }
        }
        return true;
    }

    #endregion

    #region Grid And Layers

    // Default size, stacking values, and the add / remove layer buttons.
    private void DrawLayout(TileLevelData level)
    {
        EditorGUILayout.Space();
        GUILayout.Label("Grid & Stacking", EditorStyles.boldLabel);
        Draw("Change Width", level.defaultWidth, v => EditorGUILayout.IntSlider("Default Width", v, 1, 30), v => level.defaultWidth = v);
        Draw("Change Height", level.defaultHeight, v => EditorGUILayout.IntSlider("Default Height", v, 1, 30), v => level.defaultHeight = v);
        Draw("Change Base Height", level.baseLayerHeight,
            v => EditorGUILayout.FloatField(new GUIContent("Base Layer Height", "Height of the first layer."), v), v => level.baseLayerHeight = v);
        Draw("Change Layer Shift", level.defaultLayerShift,
            v => EditorGUILayout.FloatField(new GUIContent("Default Layer Shift", "Distance between a layer and the one below."), v), v => level.defaultLayerShift = v);

        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Add Layer", GUILayout.Height(30)))
            Edit("Add Layer", () => { level.AddLayer(); layerIndex = level.layers.Count - 1; });

        GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
        if (GUILayout.Button("Remove Top Layer", GUILayout.Height(30)) && level.layers.Count > 0)
            Edit("Remove Layer", () => level.layers.RemoveAt(level.layers.Count - 1));
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
    }

    // The layer tabs and everything for the selected layer.
    private void DrawLayerEditor(TileLevelData level)
    {
        EditorGUILayout.Space();
        string[] labels = new string[level.layers.Count];
        for (int i = 0; i < labels.Length; i++) labels[i] = $"Layer {i + 1}";

        int perRow = Mathf.Clamp(Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 30f) / 75f), 1, labels.Length);
        layerIndex = GUILayout.SelectionGrid(Mathf.Clamp(layerIndex, 0, labels.Length - 1), labels, perRow);
        TileLayer layer = level.layers[layerIndex];

        Draw("Change Layer Offset", layer.offset, v => EditorGUILayout.Vector2Field("Positional Shift (X, Y)", v), v => layer.offset = v);

        bool first = layerIndex == 0;
        Draw("Override Shift", layer.overrideShift, v => EditorGUILayout.Toggle(first ? "Override Base Height" : "Override Shift", v), v => layer.overrideShift = v);
        using (new EditorGUI.DisabledScope(!layer.overrideShift))
            Draw("Change Layer Shift", layer.customShift, v => EditorGUILayout.FloatField(first ? "Base Height" : "Layer Shift", v), v => layer.customShift = v);
        EditorGUILayout.LabelField("Resulting Height", level.GetLayerStackPosition(layerIndex).ToString("0.###"));

        DrawSummary(level);
        DrawGrid(layer);
    }

    // The painted tile count, with warnings when it doesn't fit the sets.
    private static void DrawSummary(TileLevelData level)
    {
        int per = TileRules.SetSize;
        int painted = level.CountTiles();
        int usable = painted / per * per;
        int faceFixed = FixedTotal(level.facePoolEntries);
        int tileFixed = FixedTotal(level.tilePoolEntries);

        if (painted % per != 0)
            EditorGUILayout.HelpBox($"Painted: {painted} tiles, not a multiple of {per}. The last {painted % per} will be skipped.", MessageType.Warning);
        else
            EditorGUILayout.HelpBox($"Painted: {painted} tiles ({painted / per} sets of {per}).", MessageType.Info);

        if (faceFixed > usable) EditorGUILayout.HelpBox($"Face Pool counts ({faceFixed}) are more than the board holds ({usable})!", MessageType.Error);
        if (tileFixed > usable) EditorGUILayout.HelpBox($"Tile Pool counts ({tileFixed}) are more than the board holds ({usable})!", MessageType.Error);
    }

    // The sum of all fixed counts in a pool.
    private static int FixedTotal<T>(List<T> entries) where T : PoolEntry
    {
        int total = 0;
        foreach (T entry in entries)
        {
            if (entry != null) total += entry.tileCount;
        }
        return total;
    }

    #endregion

    #region Grid Painting

    // The scrollable grid of cells for one layer.
    private void DrawGrid(TileLayer layer)
    {
        EditorGUILayout.Space();
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

        if (e.type == EventType.MouseUp) painting = false;
        if (e.type == EventType.MouseDrag) Repaint();
    }

    // One cell. Click flips it, dragging paints the same value over several cells.
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
            painting = true;
            paintValue = !active;
            Edit("Paint Tile", () => layer.SetCell(x, y, paintValue));
            e.Use();
        }
        else if (e.type == EventType.MouseDrag && painting)
        {
            if (active != paintValue) Edit("Paint Tile", () => layer.SetCell(x, y, paintValue));
            e.Use();
        }
    }

    #endregion
}