/*
 * TileLibrary
 * -----------
 * The catalogue of the game, as one asset: every tile type (an id plus a prefab) and every face (an id plus
 * a sprite). Levels never point at prefabs or sprites directly, they ask the library by id. Two tiles match
 * when their face ids match. An unknown tile type id falls back to the first usable type. The right-click
 * menu "Assign Sequential Face IDs" numbers the faces for you. TileLevelEditor reads the ids to build its
 * tick lists, and TileVisualSpawner reads the prefabs and sprites.
 */
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileTypeEntry
{
    #region Public Variables

    [Tooltip("The name a level uses to pick this type, e.g. Normal, Cloudy, Gold.")]
    public string id = "Normal";
    [Tooltip("The tile prefab. A Tile (or a subclass) sits on its root.")]
    public Tile prefab;

    #endregion
}

[Serializable]
public class TileFaceEntry
{
    #region Public Variables

    [Tooltip("Two tiles match when they share this id.")]
    public int id;
    public string displayName;
    public Sprite sprite;

    #endregion
}

[CreateAssetMenu(fileName = "TileLibrary", menuName = "Tile/Tile Library")]
public class TileLibrary : ScriptableObject
{
    #region Private Variables

    [SerializeField] private List<TileTypeEntry> tileTypes = new List<TileTypeEntry>();
    [SerializeField] private List<TileFaceEntry> faces = new List<TileFaceEntry>();

    #endregion

    #region Properties

    public IReadOnlyList<TileFaceEntry> Faces => faces;

    #endregion

    #region Tile Types

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

    // Every tile type id that has a prefab.
    public List<string> GetTileTypeIds()
    {
        List<string> ids = new List<string>();
        foreach (TileTypeEntry entry in tileTypes)
        {
            if (entry != null && entry.prefab != null && !string.IsNullOrEmpty(entry.id) && !ids.Contains(entry.id)) ids.Add(entry.id);
        }
        return ids;
    }

    #endregion

    #region Faces

    // The sprite for a face id (null if there is none).
    public Sprite GetFaceSprite(int id)
    {
        TileFaceEntry face = faces.Find(f => f != null && f.id == id);
        return face != null ? face.sprite : null;
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

    // Right-click menu: numbers the faces 0, 1, 2... in list order.
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

    #endregion
}