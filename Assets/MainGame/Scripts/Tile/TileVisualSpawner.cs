using System;
using System.Collections.Generic;
using UnityEngine;

public enum TileBoardPlane
{
    XZ_Flat,
    XY_Vertical
}

public enum GridSizingMode
{
    Manual,
    FitToDimensions,
    FitToBoxCollider
}

public class TileVisualSpawner : MonoBehaviour
{
    public static Action<List<Tile>> boardGenerated;

    [Header("References")]
    [Tooltip("Optional. Tiles will be grouped under this object.")]
    [SerializeField] private Transform boardRoot;

    [Header("Level Data")]
    [SerializeField] private TileLevelData levelAsset;
    [Tooltip("Leave off when TileGameManager loads the levels for you.")]
    [SerializeField] private bool generateOnStart = false;

    [Header("Randomisation")]
    [SerializeField] private bool randomizePlacementEveryLoad = true;

    [Header("Orientation & Stacking")]
    [SerializeField] private TileBoardPlane boardPlane = TileBoardPlane.XY_Vertical;
    [SerializeField] private Vector3 tileRotation = Vector3.zero;
    [SerializeField] private bool invertStackingDirection = true;

    [Header("Grid Sizing")]
    [SerializeField] private GridSizingMode sizingMode = GridSizingMode.Manual;
    [Header("  ↳ If Manual:")]
    [SerializeField] private Vector2 manualTileSize = new Vector2(1f, 1.3f);
    [SerializeField] private Vector2 manualGap = new Vector2(0.05f, 0.05f);
    [Header("  ↳ If Fit To Dimensions:")]
    [SerializeField] private Vector2 totalAreaDimensions = new Vector2(100f, 100f);
    [Header("  ↳ If Fit To Box Collider:")]
    [SerializeField] private BoxCollider targetBoundsBox;

    [Header("Positioning")]
    [SerializeField] private bool centerBoard = true;

    private struct TileCell
    {
        public int x, y, layer;
    }

    private struct Assignment
    {
        public int faceId;
        public string typeId;
    }

    private readonly List<Tile> spawnedTiles = new List<Tile>();

    public IReadOnlyList<Tile> SpawnedTiles => spawnedTiles;
    public TileBoardPlane BoardPlane => boardPlane;
    public bool InvertStackingDirection => invertStackingDirection;

    private void Start()
    {
        if (generateOnStart && levelAsset != null) Generate();
    }

    [ContextMenu("Generate Level")]
    public void Generate()
    {
        Generate(levelAsset);
    }

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

        List<TileCell> cells = CollectCells(level);
        int usable = cells.Count / TileRules.SetSize * TileRules.SetSize;
        if (usable != cells.Count)
            Debug.LogWarning($"[SPAWNER] Level '{level.name}' has {cells.Count} tiles, not a multiple of {TileRules.SetSize}. The last {cells.Count - usable} are left out.", level);
        if (usable == 0) return;

        List<Assignment> assignments = BuildAssignments(level, library, usable, contentRng, placementRng);
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
            Assignment assignment = assignments[i];
            Tile prefab = library.GetTilePrefab(assignment.typeId);
            if (prefab == null)
            {
                Debug.LogError($"[SPAWNER] No tile prefab for type '{assignment.typeId}'.", level);
                continue;
            }

            Vector2 offset = level.layers[cell.layer].offset;
            Vector2 cellPosition = new Vector2(cell.x + offset.x, cell.y + offset.y);
            float u = start.x + cellPosition.x * step.x;
            float v = start.y + cellPosition.y * step.y;
            float stack = level.GetLayerStackPosition(cell.layer) * stackSign;

            Tile tile = Instantiate(prefab, parent);
            tile.transform.localPosition = boardPlane == TileBoardPlane.XZ_Flat ? new Vector3(u, stack, v) : new Vector3(u, -v, stack);
            tile.transform.localRotation = rotation;
            tile.name = $"Tile_{cell.x}_{cell.y}_L{cell.layer}_F{assignment.faceId}";
            tile.Init(assignment.faceId, library.GetFaceSprite(assignment.faceId), cell.layer, cellPosition);
            tile.SetLevelLooks(level.ResolveBoardLook(assignment.faceId, assignment.typeId), level.ResolveTrayLook(assignment.faceId, assignment.typeId));

            spawnedTiles.Add(tile);
        }

        if (Application.isPlaying) boardGenerated?.Invoke(new List<Tile>(spawnedTiles));
    }

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

    private Vector2 StepSize(Vector2Int max)
    {
        if (sizingMode == GridSizingMode.Manual) return manualTileSize + manualGap;
        if (sizingMode == GridSizingMode.FitToDimensions) return new Vector2(totalAreaDimensions.x / max.x, totalAreaDimensions.y / max.y);
        if (targetBoundsBox == null) return Vector2.one;

        Vector3 size = Vector3.Scale(targetBoundsBox.size, targetBoundsBox.transform.lossyScale);
        targetBoundsBox.enabled = false;   // measured; it must not intercept tile clicks
        return new Vector2(size.x / max.x, (boardPlane == TileBoardPlane.XZ_Flat ? size.z : size.y) / max.y);
    }

    // ---------------- Face and tile type assignment ----------------

    private static List<Assignment> BuildAssignments(TileLevelData level, TileLibrary library, int total, System.Random contentRng, System.Random placementRng)
    {
        List<int> faces = BuildFaceList(level, library, total, contentRng);
        string[] types = faces == null ? null : AssignTypes(level, library, faces, contentRng, placementRng);
        if (types == null) return null;

        List<Assignment> result = new List<Assignment>(faces.Count);
        for (int i = 0; i < faces.Count; i++) result.Add(new Assignment { faceId = faces[i], typeId = types[i] });

        TileRules.Shuffle(result, placementRng);
        return result;
    }

    /// <summary>Fixed face counts first, then the rest in sets of 3 from the auto faces.</summary>
    private static List<int> BuildFaceList(TileLevelData level, TileLibrary library, int total, System.Random rng)
    {
        List<FacePoolEntry> entries = new List<FacePoolEntry>();
        foreach (FacePoolEntry entry in level.facePoolEntries)
        {
            if (entry != null && library.GetFaceSprite(entry.faceId) != null) entries.Add(entry);
        }
        if (level.facePoolEntries.Count == 0)
        {
            foreach (int id in library.GetAllFaceIds()) entries.Add(new FacePoolEntry { faceId = id });
        }
        if (entries.Count == 0)
        {
            Debug.LogError($"[SPAWNER] No valid faces with sprites found for level '{level.name}'.", level);
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

        if (autoIds.Count == 0)
        {
            foreach (FacePoolEntry entry in entries) autoIds.Add(entry.faceId);
        }
        if (level.maxDifferentFaces > 0 && level.maxDifferentFaces < autoIds.Count)
        {
            TileRules.Shuffle(autoIds, rng);
            autoIds.RemoveRange(level.maxDifferentFaces, autoIds.Count - level.maxDifferentFaces);
        }

        List<int> bag = new List<int>();
        for (int s = 0; s < sets; s++)
        {
            int face = Draw(bag, autoIds, rng);
            for (int k = 0; k < TileRules.SetSize; k++) faces.Add(face);
        }
        return faces;
    }

    /// <summary>Face locks first, then fixed tile-type counts, then the auto tile types.</summary>
    private static string[] AssignTypes(TileLevelData level, TileLibrary library, List<int> faces, System.Random contentRng, System.Random placementRng)
    {
        List<string> libraryIds = library.GetTileTypeIds();
        if (libraryIds.Count == 0)
        {
            Debug.LogError($"[SPAWNER] Tile Library '{library.name}' has no tile prefabs!", library);
            return null;
        }

        List<TilePoolEntry> pool = new List<TilePoolEntry>();
        foreach (TilePoolEntry entry in level.tilePoolEntries)
        {
            if (entry != null && libraryIds.Contains(entry.tileTypeId)) pool.Add(entry);
        }
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

            int quota = entry.tileCount - CountOf(types, entry.tileTypeId);
            for (int k = 0; k < quota && cursor < free.Count; k++) types[free[cursor++]] = entry.tileTypeId;
        }

        if (autoTypes.Count == 0)
        {
            foreach (TilePoolEntry entry in pool) autoTypes.Add(entry.tileTypeId);
        }

        List<string> bag = new List<string>();
        while (cursor < free.Count) types[free[cursor++]] = Draw(bag, autoTypes, contentRng);
        return types;
    }

    private static int CountOf(string[] types, string id)
    {
        int count = 0;
        foreach (string type in types)
        {
            if (type == id) count++;
        }
        return count;
    }

    /// <summary>Takes the next item from a shuffled bag, refilling the bag from the source when empty.</summary>
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

    private static int StableHash(string text)
    {
        int hash = 17;
        if (string.IsNullOrEmpty(text)) return hash;
        for (int i = 0; i < text.Length; i++) hash = unchecked(hash * 31 + text[i]);
        return hash;
    }

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
}