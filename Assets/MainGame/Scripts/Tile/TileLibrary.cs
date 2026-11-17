using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileTypeEntry
{
    [Tooltip("The name a level uses to pick this type, e.g. Normal, Cloudy, Gold.")]
    public string id = "Normal";
    [Tooltip("The tile prefab. A Tile (or a subclass) sits on its root.")]
    public Tile prefab;
}

[Serializable]
public class TileFaceEntry
{
    [Tooltip("Two tiles match when they share this id.")]
    public int id;
    public string displayName;
    public Sprite sprite;
}

// The catalogue of the game: every tile type (prefab) and every face (sprite).
[CreateAssetMenu(fileName = "TileLibrary", menuName = "Tile/Tile Library")]
public class TileLibrary : ScriptableObject
{
    [SerializeField] private List<TileTypeEntry> tileTypes = new List<TileTypeEntry>();
    [SerializeField] private List<TileFaceEntry> faces = new List<TileFaceEntry>();

    public IReadOnlyList<TileFaceEntry> Faces => faces;

    // The prefab for this id. An unknown id falls back to the first usable type.
    public Tile GetTilePrefab(string id)
    {
        Tile fallback = null;
        foreach (TileTypeEntry entry in tileTypes)
        {
            if (entry == null || entry.prefab == null) continue;
            if (entry.id == id) return entry.prefab;
            if (fallback == null) fallback = entry.prefab;
        }
        return fallback;
    }

    public Sprite GetFaceSprite(int id)
    {
        TileFaceEntry face = faces.Find(f => f != null && f.id == id);
        return face != null ? face.sprite : null;
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

    // Every face id that actually has a sprite.
    public List<int> GetAllFaceIds()
    {
        List<int> ids = new List<int>();
        foreach (TileFaceEntry face in faces)
        {
            if (face != null && face.sprite != null && !ids.Contains(face.id)) ids.Add(face.id);
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