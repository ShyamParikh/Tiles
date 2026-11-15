using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FacePoolEntry
{
    public int faceId;

    [Tooltip("Exact number of tiles for this face (must be a multiple of 3, e.g. 3, 6, 9). Set to 0 to fill remaining spots automatically.")]
    public int tileCount = 0;

    [Tooltip("Optional: Force this face to always use a specific Tile Type ID (e.g. 'Gold'). Leave empty to use the Tile Pool.")]
    public string lockedTileTypeId = "";

    [Tooltip("Optional: Override the tile body color specifically for tiles with this face.")]
    public bool overrideColor = false;
    public Color tileColor = Color.white;

    [Tooltip("Optional: Override the tile body material specifically for tiles with this face.")]
    public bool overrideMaterial = false;
    public Material tileMaterial;
}

[Serializable]
public class TilePoolEntry
{
    public string tileTypeId;

    [Tooltip("Exact number of tiles that will use this 3D tile prefab. Set to 0 to fill remaining spots automatically.")]
    public int tileCount = 0;

    [Tooltip("Optional: Override the tile body color for this tile type in this level.")]
    public bool overrideColor = false;
    public Color tileColor = Color.white;

    [Tooltip("Optional: Override the tile body material for this tile type in this level.")]
    public bool overrideMaterial = false;
    public Material tileMaterial;
}

[CreateAssetMenu(fileName = "NewVisualLevel", menuName = "Tile/Visual Level Data")]
public class TileLevelData : ScriptableObject
{
    [Header("Tile Setup")]
    [Tooltip("Central store of tile prefabs and face sprites.")]
    public TileLibrary library;

    [Header("Tile Colors")]
    [Tooltip("Optional: Override the base color of all tiles in this level (unless overridden per Tile Type or Face).")]
    public bool overrideDefaultTileColor = false;
    public Color defaultTileColor = Color.white;
    [Tooltip("-1 = apply color to ALL material slots on the mesh. 0 = Element 0 only, 1 = Element 1 only, etc.")]
    public int targetMaterialSlot = -1;

    [Header("Tile Materials")]
    [Tooltip("Optional: Replace the body material of all tiles in this level (unless overridden per Tile Type or Face).")]
    public bool overrideDefaultTileMaterial = false;
    public Material defaultTileMaterial;
    [Tooltip("-1 = replace ALL material slots on the body mesh. 0 = Element 0 only, 1 = Element 1 only, etc.")]
    public int materialTargetSlot = -1;

    [Tooltip("Faces enabled for this level, with optional exact tile counts, locked tile types, and colors.")]
    public List<FacePoolEntry> facePoolEntries = new List<FacePoolEntry>();

    [Tooltip("3D tile prefabs enabled for this level, with optional exact tile counts and colors.")]
    public List<TilePoolEntry> tilePoolEntries = new List<TilePoolEntry>();

    [Tooltip("How many different faces to use when Auto-picking (0 = use the whole pool).")]
    public int maxDifferentFaces = 0;

    [Tooltip("Decides face selection and placement when Randomize Placement Every Load is off. 0 = automatic.")]
    public int seed = 0;

    [HideInInspector] public string tileTypeId = "";
    [HideInInspector] public List<int> facePool = new List<int>();

    [Header("Grid")]
    public int defaultWidth = 10;
    public int defaultHeight = 10;

    [Tooltip("Height of the first (base) layer, unless that layer overrides it.")]
    public float baseLayerHeight = 0f;

    [Tooltip("Distance between a layer and the one below it, unless that layer overrides it.")]
    public float defaultLayerShift = 0.3f;

    public List<TileLayer> layers = new List<TileLayer>();

    private void OnValidate()
    {
        MigrateLegacyData();
    }

    public void MigrateLegacyData()
    {
        bool migrated = false;

        if (facePool != null && facePool.Count > 0 && (facePoolEntries == null || facePoolEntries.Count == 0))
        {
            if (facePoolEntries == null) facePoolEntries = new List<FacePoolEntry>();
            foreach (int id in facePool)
            {
                if (!HasFaceEntry(id))
                    facePoolEntries.Add(new FacePoolEntry { faceId = id, tileCount = 0, lockedTileTypeId = "" });
            }
            facePool.Clear();
            migrated = true;
        }

        if (!string.IsNullOrEmpty(tileTypeId) && (tilePoolEntries == null || tilePoolEntries.Count == 0))
        {
            if (tilePoolEntries == null) tilePoolEntries = new List<TilePoolEntry>();
            tilePoolEntries.Add(new TilePoolEntry { tileTypeId = tileTypeId, tileCount = 0 });
            tileTypeId = "";
            migrated = true;
        }

#if UNITY_EDITOR
        if (migrated)
            UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public bool HasFaceEntry(int id)
    {
        if (facePoolEntries == null) return false;
        for (int i = 0; i < facePoolEntries.Count; i++)
        {
            if (facePoolEntries[i] != null && facePoolEntries[i].faceId == id) return true;
        }
        return false;
    }

    public FacePoolEntry GetFaceEntry(int id)
    {
        if (facePoolEntries == null) return null;
        for (int i = 0; i < facePoolEntries.Count; i++)
        {
            if (facePoolEntries[i] != null && facePoolEntries[i].faceId == id) return facePoolEntries[i];
        }
        return null;
    }

    public bool HasTileEntry(string id)
    {
        if (tilePoolEntries == null) return false;
        for (int i = 0; i < tilePoolEntries.Count; i++)
        {
            if (tilePoolEntries[i] != null && tilePoolEntries[i].tileTypeId == id) return true;
        }
        return false;
    }

    public TilePoolEntry GetTileEntry(string id)
    {
        if (tilePoolEntries == null) return null;
        for (int i = 0; i < tilePoolEntries.Count; i++)
        {
            if (tilePoolEntries[i] != null && tilePoolEntries[i].tileTypeId == id) return tilePoolEntries[i];
        }
        return null;
    }

    // ---------------- Look resolution (Face > Tile Type > Level Default) ----------------

    /// <summary>Returns true if this level wants to override the body color of a tile with this face / tile type.</summary>
    public bool TryGetTileColor(int faceId, string tileTypeId, out Color color)
    {
        FacePoolEntry face = GetFaceEntry(faceId);
        if (face != null && face.overrideColor)
        {
            color = face.tileColor;
            return true;
        }

        TilePoolEntry tile = GetTileEntry(tileTypeId);
        if (tile != null && tile.overrideColor)
        {
            color = tile.tileColor;
            return true;
        }

        if (overrideDefaultTileColor)
        {
            color = defaultTileColor;
            return true;
        }

        color = Color.white;
        return false;
    }

    /// <summary>Returns true if this level wants to replace the body material of a tile with this face / tile type.</summary>
    public bool TryGetTileMaterial(int faceId, string tileTypeId, out Material material)
    {
        FacePoolEntry face = GetFaceEntry(faceId);
        if (face != null && face.overrideMaterial && face.tileMaterial != null)
        {
            material = face.tileMaterial;
            return true;
        }

        TilePoolEntry tile = GetTileEntry(tileTypeId);
        if (tile != null && tile.overrideMaterial && tile.tileMaterial != null)
        {
            material = tile.tileMaterial;
            return true;
        }

        if (overrideDefaultTileMaterial && defaultTileMaterial != null)
        {
            material = defaultTileMaterial;
            return true;
        }

        material = null;
        return false;
    }

    // ---------------- Layers ----------------

    public void AddLayer()
    {
        TileLayer layer = new TileLayer(defaultWidth, defaultHeight);
        layer.customShift = layers.Count == 0 ? baseLayerHeight : defaultLayerShift;
        layers.Add(layer);
    }

    public float GetLayerShift(int index)
    {
        if (index < 0 || index >= layers.Count) return 0f;
        TileLayer layer = layers[index];
        if (layer.overrideShift) return layer.customShift;
        return index == 0 ? baseLayerHeight : defaultLayerShift;
    }

    public float GetLayerStackPosition(int index)
    {
        float total = 0f;
        for (int i = 0; i <= index && i < layers.Count; i++)
            total += GetLayerShift(i);
        return total;
    }

    public int CountTiles()
    {
        int total = 0;
        foreach (TileLayer layer in layers)
        {
            if (layer == null || layer.grid == null) continue;

            for (int y = 0; y < layer.height; y++)
            {
                for (int x = 0; x < layer.width; x++)
                {
                    if (layer.GetCell(x, y)) total++;
                }
            }
        }
        return total;
    }
}