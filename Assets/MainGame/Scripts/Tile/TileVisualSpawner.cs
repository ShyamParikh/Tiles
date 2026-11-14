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

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private struct TileCell
    {
        public int x;
        public int y;
        public int layer;

        public TileCell(int x, int y, int layer)
        {
            this.x = x;
            this.y = y;
            this.layer = layer;
        }
    }

    private struct SpawnAssignment
    {
        public int faceId;
        public string tileTypeId;

        public SpawnAssignment(int faceId, string tileTypeId)
        {
            this.faceId = faceId;
            this.tileTypeId = tileTypeId;
        }
    }

    private readonly List<Tile> spawnedTiles = new List<Tile>();

    public IReadOnlyList<Tile> SpawnedTiles => spawnedTiles;
    public TileBoardPlane BoardPlane => boardPlane;
    public bool InvertStackingDirection => invertStackingDirection;
    public Transform BoardRootTransform => boardRoot != null ? boardRoot : transform;

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

        if (level == null)
        {
            Debug.LogError("[SPAWNER] No level asset given.");
            return;
        }
        level.MigrateLegacyData();
        levelAsset = level;

        TileLibrary library = level.library;
        if (library == null)
        {
            Debug.LogError($"[SPAWNER] Level '{level.name}' has no Tile Library assigned.", level);
            return;
        }

        int contentSeed = level.seed != 0 ? level.seed : StableHash(level.name);
        System.Random contentRng = new System.Random(contentSeed);
        System.Random placementRng = randomizePlacementEveryLoad
            ? new System.Random(Guid.NewGuid().GetHashCode())
            : new System.Random(contentSeed + 1);

        List<TileCell> cells = new List<TileCell>();
        for (int z = 0; z < level.layers.Count; z++)
        {
            TileLayer layer = level.layers[z];
            for (int y = 0; y < layer.height; y++)
            {
                for (int x = 0; x < layer.width; x++)
                {
                    if (layer.GetCell(x, y)) cells.Add(new TileCell(x, y, z));
                }
            }
        }

        int perSet = TileMergeData.TilesPerMerge;
        int usable = (cells.Count / perSet) * perSet;
        if (usable != cells.Count)
        {
            Debug.LogWarning($"[SPAWNER] Level '{level.name}' has {cells.Count} tiles, which is not a multiple of {perSet}. The last {cells.Count - usable} tile(s) are left out.", level);
        }
        if (usable == 0)
        {
            Debug.LogWarning($"[SPAWNER] Level '{level.name}' has 0 usable tiles to spawn.", level);
            return;
        }

        List<SpawnAssignment> assignments = BuildSpawnAssignments(level, library, usable, contentRng, placementRng);
        if (assignments == null || assignments.Count == 0) return;

        int spawnCount = Mathf.Min(usable, assignments.Count);

        Transform parent = boardRoot != null ? boardRoot : transform;
        Quaternion customRotation = Quaternion.Euler(tileRotation);

        int maxW = 1;
        int maxH = 1;
        foreach (var layer in level.layers)
        {
            maxW = Mathf.Max(maxW, layer.width);
            maxH = Mathf.Max(maxH, layer.height);
        }

        float stepX = 1f;
        float stepY = 1f;

        if (sizingMode == GridSizingMode.Manual)
        {
            stepX = manualTileSize.x + manualGap.x;
            stepY = manualTileSize.y + manualGap.y;
        }
        else if (sizingMode == GridSizingMode.FitToDimensions)
        {
            stepX = totalAreaDimensions.x / maxW;
            stepY = totalAreaDimensions.y / maxH;
        }
        else if (sizingMode == GridSizingMode.FitToBoxCollider && targetBoundsBox != null)
        {
            float sizeU = targetBoundsBox.size.x * targetBoundsBox.transform.lossyScale.x;
            float sizeV = boardPlane == TileBoardPlane.XZ_Flat ?
                          targetBoundsBox.size.z * targetBoundsBox.transform.lossyScale.z :
                          targetBoundsBox.size.y * targetBoundsBox.transform.lossyScale.y;

            stepX = sizeU / maxW;
            stepY = sizeV / maxH;

            if (targetBoundsBox.enabled)
            {
                targetBoundsBox.enabled = false;
                if (enableDebugLogs) Debug.Log("[SPAWNER] Disabled targetBoundsBox collider after measuring so it cannot intercept tile clicks.");
            }
        }

        float startU = 0f;
        float startV = 0f;
        if (centerBoard)
        {
            float boardWidth = maxW * stepX;
            float boardHeight = maxH * stepY;

            startU = -(boardWidth / 2f) + (stepX / 2f);
            startV = -(boardHeight / 2f) + (stepY / 2f);
        }

        float stackSign = invertStackingDirection ? -1f : 1f;

        float[] layerHeights = new float[level.layers.Count];
        for (int i = 0; i < layerHeights.Length; i++)
            layerHeights[i] = level.GetLayerStackPosition(i);

        for (int i = 0; i < spawnCount; i++)
        {
            TileCell cell = cells[i];
            TileLayer layer = level.layers[cell.layer];
            SpawnAssignment assignment = assignments[i];

            Tile prefab = library.GetTilePrefab(assignment.tileTypeId);
            if (prefab == null)
            {
                Debug.LogError($"[SPAWNER] Could not resolve tile prefab for ID '{assignment.tileTypeId}'.", level);
                continue;
            }

            float cellU = cell.x + layer.offset.x;
            float cellV = cell.y + layer.offset.y;

            float posU = startU + (cellU * stepX);
            float posV = startV + (cellV * stepY);
            float posStack = layerHeights[cell.layer] * stackSign;

            Vector3 spawnPos = boardPlane == TileBoardPlane.XZ_Flat
                ? new Vector3(posU, posStack, posV)
                : new Vector3(posU, -posV, posStack);

            Tile tile = Instantiate(prefab, parent);
            tile.transform.localPosition = spawnPos;
            tile.transform.localRotation = customRotation;
            tile.name = $"Tile_{cell.x}_{cell.y}_L{cell.layer}_F{assignment.faceId}";

            library.TryGetFaceSprite(assignment.faceId, out Sprite faceSprite);
            tile.Init(assignment.faceId, faceSprite, cell.layer, new Vector2(cellU, cellV));

            spawnedTiles.Add(tile);
        }

        if (enableDebugLogs && spawnedTiles.Count > 0)
        {
            Tile sample = spawnedTiles[0];
            int colLayer = sample.TileCollider != null ? sample.TileCollider.gameObject.layer : sample.gameObject.layer;
            Debug.Log($"[SPAWNER] Spawned {spawnedTiles.Count} tiles | Plane: {boardPlane} | Inverted: {invertStackingDirection} | Collider Layer: {colLayer} ('{LayerMask.LayerToName(colLayer)}')");
        }

        if (Application.isPlaying)
            boardGenerated?.Invoke(new List<Tile>(spawnedTiles));
    }

    private List<SpawnAssignment> BuildSpawnAssignments(
        TileLevelData level,
        TileLibrary library,
        int totalUsableTiles,
        System.Random contentRng,
        System.Random placementRng)
    {
        int perSet = TileMergeData.TilesPerMerge;

        // 1. Collect valid face pool entries (or fallback to all library faces if empty)
        List<FacePoolEntry> validFaceEntries = new List<FacePoolEntry>();
        Dictionary<int, string> lockedTileByFace = new Dictionary<int, string>();

        if (level.facePoolEntries != null && level.facePoolEntries.Count > 0)
        {
            foreach (FacePoolEntry entry in level.facePoolEntries)
            {
                if (entry == null || !library.TryGetFaceSprite(entry.faceId, out _)) continue;
                validFaceEntries.Add(entry);
                if (!string.IsNullOrEmpty(entry.lockedTileTypeId))
                    lockedTileByFace[entry.faceId] = entry.lockedTileTypeId;
            }
        }
        else
        {
            foreach (int id in library.GetAllFaceIds())
                validFaceEntries.Add(new FacePoolEntry { faceId = id, tileCount = 0, lockedTileTypeId = "" });
        }

        if (validFaceEntries.Count == 0)
        {
            Debug.LogError($"[SPAWNER] No valid faces with sprites found for level '{level.name}'.", level);
            return null;
        }

        // 2. Allocate exact fixed face counts first, then fill remaining sets from Auto (0) faces
        List<int> faceList = new List<int>(totalUsableTiles);
        List<int> autoFaceIds = new List<int>();

        foreach (FacePoolEntry entry in validFaceEntries)
        {
            if (entry.tileCount > 0)
            {
                int usableForFace = (entry.tileCount / perSet) * perSet;
                if (usableForFace != entry.tileCount)
                {
                    Debug.LogWarning($"[SPAWNER] Face {entry.faceId} count ({entry.tileCount}) is not a multiple of {perSet}; rounded to {usableForFace}.", level);
                }

                int remainingCapacity = totalUsableTiles - faceList.Count;
                int toAdd = Mathf.Min(usableForFace, remainingCapacity);

                for (int k = 0; k < toAdd; k++)
                    faceList.Add(entry.faceId);

                if (toAdd < usableForFace)
                {
                    Debug.LogWarning($"[SPAWNER] Fixed face counts exceeded board capacity ({totalUsableTiles}). Trimmed Face {entry.faceId}.", level);
                }
            }
            else
            {
                autoFaceIds.Add(entry.faceId);
            }
        }

        int remainingTilesForFaces = totalUsableTiles - faceList.Count;
        if (remainingTilesForFaces > 0)
        {
            // If no faces were set to Auto (0), fall back to all pool faces so the board fills completely
            if (autoFaceIds.Count == 0)
            {
                foreach (FacePoolEntry entry in validFaceEntries)
                    autoFaceIds.Add(entry.faceId);
            }

            if (level.maxDifferentFaces > 0 && level.maxDifferentFaces < autoFaceIds.Count)
            {
                Shuffle(autoFaceIds, contentRng);
                autoFaceIds.RemoveRange(level.maxDifferentFaces, autoFaceIds.Count - level.maxDifferentFaces);
            }

            int remainingSets = remainingTilesForFaces / perSet;
            List<int> bag = new List<int>();

            for (int s = 0; s < remainingSets; s++)
            {
                if (bag.Count == 0)
                {
                    bag.AddRange(autoFaceIds);
                    Shuffle(bag, contentRng);
                }

                int pickedFace = bag[bag.Count - 1];
                bag.RemoveAt(bag.Count - 1);

                for (int k = 0; k < perSet; k++)
                    faceList.Add(pickedFace);
            }
        }

        // 3. Assign 3D Tile Prefab IDs (respecting Face->Tile locks first, then Tile Pool fixed counts, then Auto)
        List<string> allLibraryTypeIds = library.GetTileTypeIds();
        if (allLibraryTypeIds.Count == 0)
        {
            Debug.LogError($"[SPAWNER] Tile Library '{library.name}' has no tile prefabs!", library);
            return null;
        }

        List<TilePoolEntry> validTileEntries = new List<TilePoolEntry>();
        if (level.tilePoolEntries != null && level.tilePoolEntries.Count > 0)
        {
            foreach (TilePoolEntry entry in level.tilePoolEntries)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.tileTypeId) && allLibraryTypeIds.Contains(entry.tileTypeId))
                    validTileEntries.Add(entry);
            }
        }
        if (validTileEntries.Count == 0)
        {
            validTileEntries.Add(new TilePoolEntry { tileTypeId = allLibraryTypeIds[0], tileCount = 0 });
        }

        string[] assignedTileTypes = new string[faceList.Count];
        Dictionary<string, int> consumedCounts = new Dictionary<string, int>();
        List<int> unlockedIndices = new List<int>();

        // Pass A: Apply locked tile types for specific faces (e.g., Coins -> Gold)
        for (int i = 0; i < faceList.Count; i++)
        {
            int fId = faceList[i];
            if (lockedTileByFace.TryGetValue(fId, out string lockedType) && allLibraryTypeIds.Contains(lockedType))
            {
                assignedTileTypes[i] = lockedType;
                consumedCounts.TryGetValue(lockedType, out int used);
                consumedCounts[lockedType] = used + 1;
            }
            else
            {
                unlockedIndices.Add(i);
            }
        }

        // Shuffle unlocked slots so fixed and auto tile types distribute randomly across the board
        Shuffle(unlockedIndices, placementRng);
        int cursor = 0;

        // Pass B: Fulfill remaining fixed counts from Tile Pool entries
        List<string> autoTileTypeIds = new List<string>();
        foreach (TilePoolEntry entry in validTileEntries)
        {
            if (entry.tileCount > 0)
            {
                consumedCounts.TryGetValue(entry.tileTypeId, out int alreadyUsedByLocks);
                int remainingQuota = Mathf.Max(0, entry.tileCount - alreadyUsedByLocks);

                for (int k = 0; k < remainingQuota && cursor < unlockedIndices.Count; k++)
                {
                    assignedTileTypes[unlockedIndices[cursor++]] = entry.tileTypeId;
                }
            }
            else
            {
                autoTileTypeIds.Add(entry.tileTypeId);
            }
        }

        // Pass C: Fill any remaining unlocked slots with Auto (0) tile types
        if (cursor < unlockedIndices.Count)
        {
            if (autoTileTypeIds.Count == 0)
            {
                foreach (TilePoolEntry entry in validTileEntries)
                    autoTileTypeIds.Add(entry.tileTypeId);
            }

            List<string> tileBag = new List<string>();
            while (cursor < unlockedIndices.Count)
            {
                if (tileBag.Count == 0)
                {
                    tileBag.AddRange(autoTileTypeIds);
                    Shuffle(tileBag, contentRng);
                }

                string pickedType = tileBag[tileBag.Count - 1];
                tileBag.RemoveAt(tileBag.Count - 1);
                assignedTileTypes[unlockedIndices[cursor++]] = pickedType;
            }
        }

        // 4. Combine into final assignments and shuffle spatial placement across the board
        List<SpawnAssignment> result = new List<SpawnAssignment>(faceList.Count);
        for (int i = 0; i < faceList.Count; i++)
        {
            result.Add(new SpawnAssignment(faceList[i], assignedTileTypes[i]));
        }

        Shuffle(result, placementRng);
        return result;
    }

    private static void Shuffle<T>(List<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private static int StableHash(string text)
    {
        int hash = 17;
        if (string.IsNullOrEmpty(text)) return hash;

        for (int i = 0; i < text.Length; i++)
            hash = unchecked(hash * 31 + text[i]);
        return hash;
    }

    [ContextMenu("Clear Board")]
    public void Clear()
    {
        for (int i = spawnedTiles.Count - 1; i >= 0; i--)
        {
            if (spawnedTiles[i] != null)
            {
                if (Application.isPlaying) Destroy(spawnedTiles[i].gameObject);
                else DestroyImmediate(spawnedTiles[i].gameObject);
            }
        }
        spawnedTiles.Clear();
    }
}