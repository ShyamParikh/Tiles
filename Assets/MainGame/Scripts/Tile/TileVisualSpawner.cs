/*
 * TileVisualSpawner
 * -----------------
 * Turns a level asset into tiles on the board. The steps are: collect the painted cells; keep only whole sets
 * of 3; build the face list (fixed counts first, then auto faces in sets of 3 from a shuffled bag); assign a
 * tile type to every tile (face locks first, then fixed counts, then auto types); shuffle placement; then
 * instantiate each tile, give it its face and its board / tray looks, and announce the finished board through
 * boardGenerated. Inspector options: board plane (vertical wall or flat table), tile rotation, stacking
 * direction, grid sizing (Manual spacing, FitToDimensions, FitToBoxCollider), centering, whether placement
 * is random every load (off + the level's seed = the same layout every time), and whether the faces are random
 * every load (on = the auto faces are picked freely, so how many sets each face gets is random too, always in
 * whole sets of 3; tile types stay the same). TileGameManager calls Generate.
 */
using System;
using System.Collections.Generic;
using UnityEngine;

public enum TileBoardPlane { XZ_Flat, XY_Vertical }
public enum GridSizingMode { Manual, FitToDimensions, FitToBoxCollider }

public class TileVisualSpawner : MonoBehaviour
{
    #region Public Variables

    // Fired once the board exists, with every spawned tile.
    public static Action<List<Tile>> boardGenerated;

    #endregion

    #region Private Variables

    [Tooltip("Optional. Tiles are created under this object.")]
    [SerializeField] private Transform boardRoot;

    [Header("Level")]
    [SerializeField] private TileLevelData levelAsset;
    [Tooltip("Leave off when TileGameManager loads the levels for you.")]
    [SerializeField] private bool generateOnStart;
    [Tooltip("Off = the same layout every load (decided by the level's seed).")]
    [SerializeField] private bool randomizePlacementEveryLoad = true;
    [Tooltip("On = every load picks the faces at random, and how many sets each face gets is random too (always whole sets of 3). Tile types stay the same.")]
    [SerializeField] private bool randomizeFacesEveryLoad;

    [Header("Orientation & stacking")]
    [SerializeField] private TileBoardPlane boardPlane = TileBoardPlane.XY_Vertical;
    [SerializeField] private Vector3 tileRotation = Vector3.zero;
    [SerializeField] private bool invertStackingDirection = true;
    [SerializeField] private bool centerBoard = true;

    [Header("Grid sizing")]
    [SerializeField] private GridSizingMode sizingMode = GridSizingMode.Manual;
    [Tooltip("Manual: tile size and the gap between tiles.")]
    [SerializeField] private Vector2 manualTileSize = new Vector2(1f, 1.3f);
    [SerializeField] private Vector2 manualGap = new Vector2(0.05f, 0.05f);
    [Tooltip("FitToDimensions: the whole board is stretched to this size.")]
    [SerializeField] private Vector2 totalAreaDimensions = new Vector2(100f, 100f);
    [Tooltip("FitToBoxCollider: the board is stretched to this collider (it is switched off afterwards so it can't block clicks).")]
    [SerializeField] private BoxCollider targetBoundsBox;

    private struct TileCell { public int x, y, layer; }
    private struct Assignment { public int faceId; public string typeId; }

    private readonly List<Tile> spawnedTiles = new List<Tile>();

    #endregion

    #region Properties

    public IReadOnlyList<Tile> SpawnedTiles => spawnedTiles;
    public TileBoardPlane BoardPlane => boardPlane;
    public bool InvertStackingDirection => invertStackingDirection;

    #endregion

    #region Unity Callbacks

    // Builds the level at start if that option is on.
    private void Start()
    {
        if (generateOnStart && levelAsset != null) Generate();
    }

    #endregion

    #region Building The Board

    // Builds the level that is set in the inspector (also on the right-click menu).
    [ContextMenu("Generate Level")]
    public void Generate()
    {
        Generate(levelAsset);
    }

    // Builds the given level: clears the old board, then creates and places every tile.
    public void Generate(TileLevelData level)
    {
        Clear();

        if (level == null || level.library == null)
        {
            Debug.LogError("[SPAWNER] The level or its Tile Library is missing.", level);
            return;
        }

        level.MigrateLegacyData();
        levelAsset = level;
        TileLibrary library = level.library;

        int seed = level.seed != 0 ? level.seed : StableHash(level.name);
        System.Random contentRng = new System.Random(seed);
        System.Random placementRng = new System.Random(randomizePlacementEveryLoad ? Guid.NewGuid().GetHashCode() : seed + 1);

        // With the option on, faces use their own fresh random, so they differ on every load.
        System.Random faceRng = randomizeFacesEveryLoad ? new System.Random(Guid.NewGuid().GetHashCode()) : contentRng;

        // Only whole sets of 3 can be played, so a leftover cell or two is dropped.
        List<TileCell> cells = CollectCells(level);
        int usable = cells.Count / TileRules.SetSize * TileRules.SetSize;
        if (usable == 0) return;

        List<Assignment> assignments = BuildAssignments(level, library, usable, contentRng, placementRng, faceRng, randomizeFacesEveryLoad);
        if (assignments == null) return;

        Vector2Int max = MaxLayerSize(level);
        Vector2 step = StepSize(max);
        Vector2 start = centerBoard ? new Vector2(step.x * (1 - max.x) / 2f, step.y * (1 - max.y) / 2f) : Vector2.zero;
        Transform parent = boardRoot != null ? boardRoot : transform;
        Quaternion rotation = Quaternion.Euler(tileRotation);
        float stackSign = invertStackingDirection ? -1f : 1f;

        for (int i = 0; i < assignments.Count; i++)
        {
            TileCell cell = cells[i];
            Assignment a = assignments[i];

            Vector2 offset = level.layers[cell.layer].offset;
            Vector2 cellPosition = new Vector2(cell.x + offset.x, cell.y + offset.y);
            float u = start.x + cellPosition.x * step.x;
            float v = start.y + cellPosition.y * step.y;
            float stack = level.GetLayerStackPosition(cell.layer) * stackSign;

            Tile tile = Instantiate(library.GetTilePrefab(a.typeId), parent);
            tile.transform.localPosition = boardPlane == TileBoardPlane.XZ_Flat ? new Vector3(u, stack, v) : new Vector3(u, -v, stack);
            tile.transform.localRotation = rotation;
            tile.name = $"Tile_{cell.x}_{cell.y}_L{cell.layer}_F{a.faceId}";
            tile.Init(a.faceId, library.GetFaceSprite(a.faceId), cell.layer, cellPosition);
            tile.SetLevelLooks(level.ResolveLook(a.faceId, a.typeId, false), level.ResolveLook(a.faceId, a.typeId, true));

            spawnedTiles.Add(tile);
        }

        if (Application.isPlaying) boardGenerated?.Invoke(new List<Tile>(spawnedTiles));
    }

    // Every painted cell of every layer.
    private static List<TileCell> CollectCells(TileLevelData level)
    {
        List<TileCell> cells = new List<TileCell>();
        for (int z = 0; z < level.layers.Count; z++)
        {
            TileLayer layer = level.layers[z];
            for (int y = 0; y < layer.height; y++)
            {
                for (int x = 0; x < layer.width; x++)
                {
                    if (layer.GetCell(x, y)) cells.Add(new TileCell { x = x, y = y, layer = z });
                }
            }
        }
        return cells;
    }

    // The biggest width and height among the layers.
    private static Vector2Int MaxLayerSize(TileLevelData level)
    {
        Vector2Int max = Vector2Int.one;
        foreach (TileLayer layer in level.layers)
        {
            max.x = Mathf.Max(max.x, layer.width);
            max.y = Mathf.Max(max.y, layer.height);
        }
        return max;
    }

    // How far apart neighbouring cells are, in world units.
    private Vector2 StepSize(Vector2Int max)
    {
        if (sizingMode == GridSizingMode.Manual) return manualTileSize + manualGap;
        if (sizingMode == GridSizingMode.FitToDimensions) return new Vector2(totalAreaDimensions.x / max.x, totalAreaDimensions.y / max.y);
        if (targetBoundsBox == null) return Vector2.one;

        Vector3 size = Vector3.Scale(targetBoundsBox.size, targetBoundsBox.transform.lossyScale);
        targetBoundsBox.enabled = false;
        return new Vector2(size.x / max.x, (boardPlane == TileBoardPlane.XZ_Flat ? size.z : size.y) / max.y);
    }

    #endregion

    #region Choosing Faces And Tile Types

    // Combines faces and tile types into one shuffled list of assignments.
    private static List<Assignment> BuildAssignments(TileLevelData level, TileLibrary library, int total, System.Random contentRng, System.Random placementRng, System.Random faceRng, bool randomFaceAmounts)
    {
        List<int> faces = BuildFaceList(level, library, total, faceRng, randomFaceAmounts);
        string[] types = faces == null ? null : AssignTypes(level, library, faces, contentRng, placementRng);
        if (types == null) return null;

        List<Assignment> result = new List<Assignment>(faces.Count);
        for (int i = 0; i < faces.Count; i++) result.Add(new Assignment { faceId = faces[i], typeId = types[i] });

        TileRules.Shuffle(result, placementRng);
        return result;
    }

    // Faces with a fixed count go first. The rest is filled in sets of 3 from the "auto" faces.
    // With randomAmounts on, every set picks any auto face freely, so the amount per face is random.
    private static List<int> BuildFaceList(TileLevelData level, TileLibrary library, int total, System.Random rng, bool randomAmounts)
    {
        List<FacePoolEntry> entries = level.facePoolEntries.FindAll(e => e != null && library.GetFaceSprite(e.faceId) != null);
        if (level.facePoolEntries.Count == 0)
        {
            foreach (int id in library.GetAllFaceIds()) entries.Add(new FacePoolEntry { faceId = id });
        }
        if (entries.Count == 0)
        {
            Debug.LogError($"[SPAWNER] No faces with a sprite found for level '{level.name}'.", level);
            return null;
        }

        List<int> faces = new List<int>(total);
        List<int> autoIds = new List<int>();
        foreach (FacePoolEntry entry in entries)
        {
            if (entry.tileCount <= 0)
            {
                autoIds.Add(entry.faceId);
                continue;
            }

            int count = Mathf.Min(entry.tileCount / TileRules.SetSize * TileRules.SetSize, total - faces.Count);
            for (int k = 0; k < count; k++) faces.Add(entry.faceId);
        }

        int sets = (total - faces.Count) / TileRules.SetSize;
        if (sets == 0) return faces;

        if (autoIds.Count == 0) entries.ForEach(e => autoIds.Add(e.faceId));
        if (level.maxDifferentFaces > 0 && level.maxDifferentFaces < autoIds.Count)
        {
            TileRules.Shuffle(autoIds, rng);
            autoIds.RemoveRange(level.maxDifferentFaces, autoIds.Count - level.maxDifferentFaces);
        }

        List<int> bag = new List<int>();
        for (int s = 0; s < sets; s++)
        {
            // Random amounts: any face can be drawn any number of times. Otherwise the bag spreads the faces evenly.
            int face;
            if (randomAmounts)
            {
                face = autoIds[rng.Next(autoIds.Count)];
            }
            else
            {
                face = Draw(bag, autoIds, rng);
            }

            for (int k = 0; k < TileRules.SetSize; k++) faces.Add(face);
        }
        return faces;
    }

    // Face locks first, then fixed tile-type counts, then the auto types fill whatever is left.
    private static string[] AssignTypes(TileLevelData level, TileLibrary library, List<int> faces, System.Random contentRng, System.Random placementRng)
    {
        List<string> libraryIds = library.GetTileTypeIds();
        if (libraryIds.Count == 0)
        {
            Debug.LogError($"[SPAWNER] Tile Library '{library.name}' has no tile prefabs!", library);
            return null;
        }

        List<TilePoolEntry> pool = level.tilePoolEntries.FindAll(e => e != null && libraryIds.Contains(e.tileTypeId));
        if (pool.Count == 0) pool.Add(new TilePoolEntry { tileTypeId = libraryIds[0] });

        string[] types = new string[faces.Count];
        List<int> free = new List<int>();
        for (int i = 0; i < faces.Count; i++)
        {
            FacePoolEntry face = level.GetFaceEntry(faces[i]);
            if (face != null && libraryIds.Contains(face.lockedTileTypeId)) types[i] = face.lockedTileTypeId;
            else free.Add(i);
        }

        TileRules.Shuffle(free, placementRng);
        int cursor = 0;
        List<string> autoTypes = new List<string>();

        foreach (TilePoolEntry entry in pool)
        {
            if (entry.tileCount <= 0)
            {
                autoTypes.Add(entry.tileTypeId);
                continue;
            }

            // Tiles already given this type through a face lock count towards its quota.
            int quota = entry.tileCount - Array.FindAll(types, t => t == entry.tileTypeId).Length;
            for (int k = 0; k < quota && cursor < free.Count; k++) types[free[cursor++]] = entry.tileTypeId;
        }

        if (autoTypes.Count == 0) pool.ForEach(e => autoTypes.Add(e.tileTypeId));

        List<string> bag = new List<string>();
        while (cursor < free.Count) types[free[cursor++]] = Draw(bag, autoTypes, contentRng);
        return types;
    }

    // Takes the next item from a shuffled bag, refilling the bag from the source when it runs out.
    private static T Draw<T>(List<T> bag, List<T> source, System.Random rng)
    {
        if (bag.Count == 0)
        {
            bag.AddRange(source);
            TileRules.Shuffle(bag, rng);
        }

        T item = bag[bag.Count - 1];
        bag.RemoveAt(bag.Count - 1);
        return item;
    }

    // A number from text that is always the same (unlike string.GetHashCode), used as the default seed.
    private static int StableHash(string text)
    {
        int hash = 17;
        foreach (char c in text) hash = unchecked(hash * 31 + c);
        return hash;
    }

    #endregion

    #region Clearing

    // Destroys every spawned tile (also on the right-click menu).
    [ContextMenu("Clear Board")]
    public void Clear()
    {
        foreach (Tile tile in spawnedTiles)
        {
            if (tile == null) continue;
            if (Application.isPlaying) Destroy(tile.gameObject);
            else DestroyImmediate(tile.gameObject);
        }
        spawnedTiles.Clear();
    }

    #endregion
}