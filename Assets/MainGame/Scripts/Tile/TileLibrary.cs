using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileTypeEntry
{
    [Tooltip("Unique name a level uses to pick this tile type, e.g. Normal, Cloudy, Gold.")]
    public string id = "Normal";
    [Tooltip("The tile prefab. A Tile (or subclass) must be on its root.")]
    public Tile prefab;
}

[Serializable]
public class TileFaceEntry
{
    [Tooltip("Unique number. Two tiles match when they share the same face ID.")]
    public int id;
    public string displayName;
    public Sprite sprite;
}

/// <summary>Every tile type (prefab) and tile face (sprite) of the game.</summary>
[CreateAssetMenu(fileName = "TileLibrary", menuName = "Tile/Tile Library")]
public class TileLibrary : ScriptableObject
{
    [SerializeField] private List<TileTypeEntry> tileTypes = new List<TileTypeEntry>();
    [SerializeField] private List<TileFaceEntry> faces = new List<TileFaceEntry>();

    public IReadOnlyList<TileTypeEntry> TileTypes => tileTypes;
    public IReadOnlyList<TileFaceEntry> Faces => faces;

    /// <summary>The prefab for this id, or the first available prefab when the id is unknown.</summary>
    public Tile GetTilePrefab(string id)
    {
        Tile fallback = null;
        foreach (TileTypeEntry entry in tileTypes)
        {
            if (entry == null || entry.prefab == null) continue;
            if (entry.id == id) return entry.prefab;
            if (fallback == null) fallback = entry.prefab;
        }

        if (!string.IsNullOrEmpty(id)) Debug.LogWarning($"TileLibrary: tile type '{id}' not found. Using the first available type.", this);
        return fallback;
    }

    public Sprite GetFaceSprite(int id)
    {
        foreach (TileFaceEntry entry in faces)
        {
            if (entry != null && entry.id == id) return entry.sprite;
        }
        return null;
    }

    public List<string> GetTileTypeIds()
    {
        List<string> ids = new List<string>();
        foreach (TileTypeEntry entry in tileTypes)
        {
            if (entry != null && entry.prefab != null && !string.IsNullOrEmpty(entry.id) && !ids.Contains(entry.id)) ids.Add(entry.id);
        }
        return ids;
    }

    /// <summary>Every face id that has a sprite, in list order.</summary>
    public List<int> GetAllFaceIds()
    {
        List<int> ids = new List<int>();
        foreach (TileFaceEntry entry in faces)
        {
            if (entry != null && entry.sprite != null && !ids.Contains(entry.id)) ids.Add(entry.id);
        }
        return ids;
    }

    [ContextMenu("Assign Sequential Face IDs")]
    private void AssignSequentialFaceIds()
    {
        for (int i = 0; i < faces.Count; i++)
        {
            if (faces[i] != null) faces[i].id = i;
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}