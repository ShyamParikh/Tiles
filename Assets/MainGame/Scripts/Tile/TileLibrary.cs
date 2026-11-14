using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class TileTypeEntry
{
    [Tooltip("Unique name a level uses to pick this tile type, e.g. Classic, Wood, Gold.")]
    public string id = "Classic";

    [Tooltip("The 3D tile prefab. The Tile component must be on its root.")]
    public Tile prefab;
}

[Serializable]
public class TileFaceEntry
{
    [Tooltip("Unique number. Two tiles match when they share the same face ID.")]
    public int id;

    public string displayName;

    [Tooltip("The image drawn on the front of the tile.")]
    public Sprite sprite;
}

/// <summary>
/// One central store for every tile type (3D prefab) and every tile face (sprite).
/// Lists are what you edit in the inspector; dictionaries are built from them at runtime for fast lookup.
/// </summary>
[CreateAssetMenu(fileName = "TileLibrary", menuName = "Tile/Tile Library")]
public class TileLibrary : ScriptableObject
{
    [SerializeField] private List<TileTypeEntry> tileTypes = new List<TileTypeEntry>();
    [SerializeField] private List<TileFaceEntry> faces = new List<TileFaceEntry>();

    private Dictionary<string, Tile> typeLookup;
    private Dictionary<int, Sprite> faceLookup;

    public IReadOnlyList<TileTypeEntry> TileTypes => tileTypes;
    public IReadOnlyList<TileFaceEntry> Faces => faces;

    private void OnEnable()
    {
        typeLookup = null;
        faceLookup = null;
    }

    private void OnValidate()
    {
        typeLookup = null;
        faceLookup = null;
    }

    private void BuildLookups()
    {
        if (typeLookup != null && faceLookup != null) return;

        typeLookup = new Dictionary<string, Tile>();
        faceLookup = new Dictionary<int, Sprite>();

        foreach (TileTypeEntry entry in tileTypes)
        {
            if (entry == null || entry.prefab == null || string.IsNullOrEmpty(entry.id)) continue;

            if (typeLookup.ContainsKey(entry.id))
                Debug.LogWarning($"TileLibrary: duplicate tile type id '{entry.id}'. Only the first one is used.", this);
            else
                typeLookup.Add(entry.id, entry.prefab);
        }

        foreach (TileFaceEntry entry in faces)
        {
            if (entry == null) continue;

            if (faceLookup.ContainsKey(entry.id))
                Debug.LogWarning($"TileLibrary: duplicate face id {entry.id}. Only the first one is used.", this);
            else
                faceLookup.Add(entry.id, entry.sprite);
        }
    }

    // ---------------- Tile types (3D prefabs) ----------------

    public Tile GetTilePrefab(string id)
    {
        BuildLookups();

        if (!string.IsNullOrEmpty(id))
        {
            if (typeLookup.TryGetValue(id, out Tile prefab)) return prefab;
            Debug.LogWarning($"TileLibrary: tile type '{id}' not found. Using the first available type.", this);
        }

        foreach (TileTypeEntry entry in tileTypes)
        {
            if (entry != null && entry.prefab != null) return entry.prefab;
        }
        return null;
    }

    public List<string> GetTileTypeIds()
    {
        BuildLookups();

        List<string> ids = new List<string>();
        foreach (TileTypeEntry entry in tileTypes)
        {
            if (entry == null || entry.prefab == null || string.IsNullOrEmpty(entry.id)) continue;
            if (!ids.Contains(entry.id)) ids.Add(entry.id);
        }
        return ids;
    }

    // ---------------- Tile faces (sprites) ----------------

    public bool TryGetFaceSprite(int id, out Sprite sprite)
    {
        BuildLookups();
        return faceLookup.TryGetValue(id, out sprite) && sprite != null;
    }

    public Sprite GetFaceSprite(int id)
    {
        TryGetFaceSprite(id, out Sprite sprite);
        return sprite;
    }

    // Every face id that has a sprite assigned, in list order.
    public List<int> GetAllFaceIds()
    {
        BuildLookups();

        List<int> ids = new List<int>();
        HashSet<int> seen = new HashSet<int>();
        foreach (TileFaceEntry entry in faces)
        {
            if (entry == null || entry.sprite == null) continue;
            if (seen.Add(entry.id)) ids.Add(entry.id);
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
        typeLookup = null;
        faceLookup = null;
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }
}