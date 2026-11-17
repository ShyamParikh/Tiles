using System;
using System.Collections.Generic;
using UnityEngine;

// What a face or a tile type wants for the level: how many tiles, and how they look on the board and in the tray.
[Serializable]
public class PoolEntry
{
    [Tooltip("Exact tile count (a multiple of 3 for faces). 0 = fill the remaining spots automatically.")]
    public int tileCount;

    public bool overrideColor;
    public Color tileColor = Color.white;
    public bool overrideMaterial;
    public Material tileMaterial;

    public bool trayOverrideColor;
    public Color trayColor = Color.white;
    public bool trayOverrideMaterial;
    public Material trayMaterial;

    // The material this entry wants (null = no opinion).
    public Material MaterialFor(bool tray)
    {
        return tray ? (trayOverrideMaterial ? trayMaterial : null) : (overrideMaterial ? tileMaterial : null);
    }

    // True when this entry wants to set the color.
    public bool ColorFor(bool tray, out Color color)
    {
        color = tray ? trayColor : tileColor;
        return tray ? trayOverrideColor : overrideColor;
    }
}

[Serializable]
public class FacePoolEntry : PoolEntry
{
    public int faceId;
    [Tooltip("Optional: this face always uses this Tile Type ID. Empty = use the Tile Pool.")]
    public string lockedTileTypeId = "";
}

[Serializable]
public class TilePoolEntry : PoolEntry
{
    public string tileTypeId;
}

// One level: the grid, which faces and tile types appear, and how tiles look.
[CreateAssetMenu(fileName = "NewVisualLevel", menuName = "Tile/Visual Level Data")]
public class TileLevelData : ScriptableObject
{
    // New level assets start with these stacking values. Existing assets keep whatever they already have.
    public const float DefaultBaseLayerHeight = -30f;
    public const float DefaultLayerShiftValue = -90f;

    public TileLibrary library;

    [Tooltip("Level-wide look. A Face or Tile Type entry beats it. -1 = every material slot.")]
    public bool overrideDefaultTileColor;
    public Color defaultTileColor = Color.white;
    public int targetMaterialSlot = -1;
    public bool overrideDefaultTileMaterial;
    public Material defaultTileMaterial;
    public int materialTargetSlot = -1;

    [Tooltip("Level-wide look inside the tray, for tiles that don't set their own.")]
    public bool overrideDefaultTrayColor;
    public Color defaultTrayColor = Color.white;
    public bool overrideDefaultTrayMaterial;
    public Material defaultTrayMaterial;

    public List<FacePoolEntry> facePoolEntries = new List<FacePoolEntry>();
    public List<TilePoolEntry> tilePoolEntries = new List<TilePoolEntry>();

    [Tooltip("How many different faces to use when picking automatically (0 = the whole pool).")]
    public int maxDifferentFaces;
    [Tooltip("Keeps the random picks the same every load when Randomize Placement is off. 0 = automatic.")]
    public int seed;

    // Old single-type fields, only kept so older level assets can be converted.
    [HideInInspector] public string tileTypeId = "";
    [HideInInspector] public List<int> facePool = new List<int>();

    public int defaultWidth = 10;
    public int defaultHeight = 10;
    public float baseLayerHeight = DefaultBaseLayerHeight;
    public float defaultLayerShift = DefaultLayerShiftValue;
    public List<TileLayer> layers = new List<TileLayer>();

    private void OnValidate()
    {
        MigrateLegacyData();
    }

    // Right-click the asset to put an existing level back on the default stacking values.
    [ContextMenu("Reset Stacking To Defaults")]
    private void ResetStacking()
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "Reset Stacking To Defaults");
#endif
        baseLayerHeight = DefaultBaseLayerHeight;
        defaultLayerShift = DefaultLayerShiftValue;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public void MigrateLegacyData()
    {
        bool changed = false;

        if (facePool.Count > 0 && facePoolEntries.Count == 0)
        {
            foreach (int id in facePool) facePoolEntries.Add(new FacePoolEntry { faceId = id });
            facePool.Clear();
            changed = true;
        }

        if (!string.IsNullOrEmpty(tileTypeId) && tilePoolEntries.Count == 0)
        {
            tilePoolEntries.Add(new TilePoolEntry { tileTypeId = tileTypeId });
            tileTypeId = "";
            changed = true;
        }

#if UNITY_EDITOR
        if (changed) UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public FacePoolEntry GetFaceEntry(int id) { return facePoolEntries.Find(e => e != null && e.faceId == id); }
    public TilePoolEntry GetTileEntry(string id) { return tilePoolEntries.Find(e => e != null && e.tileTypeId == id); }

    // What a tile should look like on the board (tray = false) or in the tray (tray = true).
    public TileLookSpec ResolveLook(int faceId, string typeId, bool tray)
    {
        TileLookSpec spec = new TileLookSpec { materialSlot = materialTargetSlot, colorSlot = targetMaterialSlot };

        // The most specific source wins: the face, then the tile type, then the level default.
        PoolEntry[] sources = { GetFaceEntry(faceId), GetTileEntry(typeId), DefaultEntry() };
        foreach (PoolEntry source in sources)
        {
            if (source == null) continue;

            if (spec.material == null) spec.material = source.MaterialFor(tray);
            if (!spec.hasColor && source.ColorFor(tray, out Color color))
            {
                spec.hasColor = true;
                spec.color = color;
            }
        }
        return spec;
    }

    // The level-wide look wrapped as an entry, so it goes through the same code as the others.
    private PoolEntry DefaultEntry()
    {
        return new PoolEntry
        {
            overrideColor = overrideDefaultTileColor,
            tileColor = defaultTileColor,
            overrideMaterial = overrideDefaultTileMaterial,
            tileMaterial = defaultTileMaterial,
            trayOverrideColor = overrideDefaultTrayColor,
            trayColor = defaultTrayColor,
            trayOverrideMaterial = overrideDefaultTrayMaterial,
            trayMaterial = defaultTrayMaterial
        };
    }

    public void AddLayer()
    {
        TileLayer layer = new TileLayer(defaultWidth, defaultHeight);
        layer.customShift = layers.Count == 0 ? baseLayerHeight : defaultLayerShift;
        layers.Add(layer);
    }

    public float GetLayerShift(int index)
    {
        if (index < 0 || index >= layers.Count) return 0f;
        if (layers[index].overrideShift) return layers[index].customShift;
        return index == 0 ? baseLayerHeight : defaultLayerShift;
    }

    // Height of a layer = every shift up to and including it.
    public float GetLayerStackPosition(int index)
    {
        float total = 0f;
        for (int i = 0; i <= index && i < layers.Count; i++) total += GetLayerShift(i);
        return total;
    }

    public int CountTiles()
    {
        int total = 0;
        foreach (TileLayer layer in layers)
        {
            if (layer != null) total += layer.CountCells();
        }
        return total;
    }
}