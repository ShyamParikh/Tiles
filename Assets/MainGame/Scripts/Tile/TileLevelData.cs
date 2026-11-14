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
}

[Serializable]
public class TilePoolEntry
{
    public string tileTypeId;

    [Tooltip("Exact number of tiles that will use this 3D tile prefab. Set to 0 to fill remaining spots automatically.")]
    public int tileCount = 0;
}

[CreateAssetMenu(fileName = "NewVisualLevel", menuName = "Tile/Visual Level Data")]
public class TileLevelData : ScriptableObject
{
    [Header("Tile Setup")]
    [Tooltip("Central store of tile prefabs and face sprites.")]
    public TileLibrary library;

    [Tooltip("Faces enabled for this level, with optional exact tile counts and locked tile types. Empty = use all faces in library automatically.")]
    public List<FacePoolEntry> facePoolEntries = new List<FacePoolEntry>();

    [Tooltip("3D tile prefabs enabled for this level, with optional exact tile counts. Empty = use first tile type in library automatically.")]
    public List<TilePoolEntry> tilePoolEntries = new List<TilePoolEntry>();

    [Tooltip("How many different faces to use when Auto-picking (0 = use the whole pool).")]
    public int maxDifferentFaces = 0;

    [Tooltip("Decides face selection and placement when Randomize Placement Every Load is off. 0 = automatic.")]
    public int seed = 0;

    // Legacy fields kept hidden so existing level assets auto-migrate without losing data
    [HideInInspector] public string tileTypeId = "";
    [HideInInspector] public List<int> facePool = new List<int>();

    [Header("Grid")]
    public int defaultWidth = 10;
    public int defaultHeight = 10;

    [Tooltip("Height of the first (base) layer, unless that layer overrides it.")]
    public float baseLayerHeight = -15f;

    [Tooltip("Distance between a layer and the one below it, unless that layer overrides it.")]
    public float defaultLayerShift = -20f;

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