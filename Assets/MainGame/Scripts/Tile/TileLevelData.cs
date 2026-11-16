using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Shared settings of a face or tile-type entry in a level pool.</summary>
[Serializable]
public class PoolEntry
{
    [Tooltip("Exact tile count (a multiple of 3 for faces). 0 = fill the remaining spots automatically.")]
    public int tileCount = 0;

    [Header("Board look")]
    public bool overrideColor = false;
    public Color tileColor = Color.white;
    public bool overrideMaterial = false;
    public Material tileMaterial;

    [Header("Look in the tray (when the tile reaches a tray slot)")]
    public bool trayOverrideColor = false;
    public Color trayColor = Color.white;
    public bool trayOverrideMaterial = false;
    public Material trayMaterial;
}

[Serializable]
public class FacePoolEntry : PoolEntry
{
    public int faceId;
    [Tooltip("Optional: force this face to always use one Tile Type ID. Empty = use the Tile Pool.")]
    public string lockedTileTypeId = "";
}

[Serializable]
public class TilePoolEntry : PoolEntry
{
    public string tileTypeId;
}

[CreateAssetMenu(fileName = "NewVisualLevel", menuName = "Tile/Visual Level Data")]
public class TileLevelData : ScriptableObject
{
    public TileLibrary library;

    [Tooltip("Optional: override the body color of every tile in this level (unless a Face or Tile Type overrides it).")]
    public bool overrideDefaultTileColor = false;
    public Color defaultTileColor = Color.white;
    [Tooltip("-1 = tint ALL material slots. 0 = Element 0 only, 1 = Element 1 only, etc.")]
    public int targetMaterialSlot = -1;

    [Tooltip("Optional: replace the body material of every tile in this level (unless a Face or Tile Type overrides it).")]
    public bool overrideDefaultTileMaterial = false;
    public Material defaultTileMaterial;
    [Tooltip("-1 = replace ALL material slots. 0 = Element 0 only, 1 = Element 1 only, etc.")]
    public int materialTargetSlot = -1;

    [Tooltip("Optional: every tile of this level changes to this color when it reaches a tray slot (unless a Face or Tile Type overrides it).")]
    public bool overrideDefaultTrayColor = false;
    public Color defaultTrayColor = Color.white;
    [Tooltip("Optional: every tile of this level changes to this material when it reaches a tray slot (unless a Face or Tile Type overrides it).")]
    public bool overrideDefaultTrayMaterial = false;
    public Material defaultTrayMaterial;

    public List<FacePoolEntry> facePoolEntries = new List<FacePoolEntry>();
    public List<TilePoolEntry> tilePoolEntries = new List<TilePoolEntry>();

    [Tooltip("How many different faces to use when auto-picking (0 = the whole pool).")]
    public int maxDifferentFaces = 0;
    [Tooltip("Decides face selection and placement when Randomize Placement Every Load is off. 0 = automatic.")]
    public int seed = 0;

    [HideInInspector] public string tileTypeId = "";
    [HideInInspector] public List<int> facePool = new List<int>();

    public int defaultWidth = 10;
    public int defaultHeight = 10;
    public float baseLayerHeight = 0f;
    public float defaultLayerShift = 0.3f;
    public List<TileLayer> layers = new List<TileLayer>();

    private void OnValidate()
    {
        MigrateLegacyData();
    }

    /// <summary>Converts the old single tile type / face id list into pool entries.</summary>
    public void MigrateLegacyData()
    {
        bool migrated = false;

        if (facePool.Count > 0 && facePoolEntries.Count == 0)
        {
            foreach (int id in facePool) facePoolEntries.Add(new FacePoolEntry { faceId = id });
            facePool.Clear();
            migrated = true;
        }

        if (!string.IsNullOrEmpty(tileTypeId) && tilePoolEntries.Count == 0)
        {
            tilePoolEntries.Add(new TilePoolEntry { tileTypeId = tileTypeId });
            tileTypeId = "";
            migrated = true;
        }

#if UNITY_EDITOR
        if (migrated) UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public FacePoolEntry GetFaceEntry(int id)
    {
        foreach (FacePoolEntry entry in facePoolEntries)
        {
            if (entry != null && entry.faceId == id) return entry;
        }
        return null;
    }

    public TilePoolEntry GetTileEntry(string id)
    {
        foreach (TilePoolEntry entry in tilePoolEntries)
        {
            if (entry != null && entry.tileTypeId == id) return entry;
        }
        return null;
    }

    /// <summary>The look a tile has on the board. Priority: Face > Tile Type > Level default.</summary>
    public TileLookSpec ResolveBoardLook(int faceId, string typeId)
    {
        FacePoolEntry face = GetFaceEntry(faceId);
        TilePoolEntry type = GetTileEntry(typeId);
        TileLookSpec spec = new TileLookSpec { materialSlot = materialTargetSlot, colorSlot = targetMaterialSlot };

        PoolEntry material = First(face, type, e => e.overrideMaterial && e.tileMaterial != null);
        if (material != null) spec.material = material.tileMaterial;
        else if (overrideDefaultTileMaterial) spec.material = defaultTileMaterial;

        PoolEntry color = First(face, type, e => e.overrideColor);
        if (color != null) SetColor(ref spec, color.tileColor);
        else if (overrideDefaultTileColor) SetColor(ref spec, defaultTileColor);

        return spec;
    }

    /// <summary>The look a tile changes to when it reaches a tray slot. An empty spec means no change.</summary>
    public TileLookSpec ResolveTrayLook(int faceId, string typeId)
    {
        FacePoolEntry face = GetFaceEntry(faceId);
        TilePoolEntry type = GetTileEntry(typeId);
        TileLookSpec spec = new TileLookSpec { materialSlot = materialTargetSlot, colorSlot = targetMaterialSlot };

        PoolEntry material = First(face, type, e => e.trayOverrideMaterial && e.trayMaterial != null);
        if (material != null) spec.material = material.trayMaterial;
        else if (overrideDefaultTrayMaterial) spec.material = defaultTrayMaterial;

        PoolEntry color = First(face, type, e => e.trayOverrideColor);
        if (color != null) SetColor(ref spec, color.trayColor);
        else if (overrideDefaultTrayColor) SetColor(ref spec, defaultTrayColor);

        return spec;
    }

    private static PoolEntry First(PoolEntry a, PoolEntry b, Predicate<PoolEntry> match)
    {
        if (a != null && match(a)) return a;
        return b != null && match(b) ? b : null;
    }

    private static void SetColor(ref TileLookSpec spec, Color color)
    {
        spec.hasColor = true;
        spec.color = color;
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