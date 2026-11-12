using System.Collections.Generic;
using UnityEngine;

// The numbers are fixed on purpose: scenes saved with the old order (BuiltIn = 0)
// now load as Auto, and LevelAsset / Random keep their old values.
public enum MahjongSpawnSource
{
    Auto = 0,         // uses the Level Asset if one is assigned at Level Index, otherwise the built-in pattern
    LevelAsset = 1,   // always the hand-made level
    Random = 2,       // always the random generator
    BuiltIn = 3       // always the built-in pattern
}

public enum MahjongBoardPlane
{
    XY_Vertical,   // upright wall: columns = X, rows = Y, layers stack along Z
    XZ_Flat        // flat table:   columns = X, rows = Z, layers stack along Y (up)
}

public class MahjongBoardSpawner : MonoBehaviour
{
    private const string ContainerName = "__MahjongTiles";

    [Header("References")]
    [Tooltip("Drag the TilePrefab3D asset here. It must have the MahjongTile component on its root.")]
    [SerializeField] private MahjongTile tilePrefab;
    [Tooltip("Optional. Tiles are spawned under this object. Empty = this object. Keep its scale at 1.")]
    [SerializeField] private Transform boardRoot;

    [Header("What To Spawn")]
    [Tooltip("Auto = Level Asset if one is assigned at Level Index, otherwise the Built-In pattern. The other options force exactly that source.")]
    [SerializeField] private MahjongSpawnSource source = MahjongSpawnSource.Auto;
    [SerializeField] private bool generateOnStart = true;

    [Header("Built-In Pattern (Source = BuiltIn)")]
    [SerializeField] private MahjongPatternSettings patternSettings = new MahjongPatternSettings();

    [Header("Hand-Made Levels (Source = LevelAsset)")]
    [SerializeField] private MahjongLevelData[] levelAssets;
    [SerializeField] private int levelIndex = 0;
    [Tooltip("OFF: a hand-made level spawns exactly as drawn (only overlapping tiles are dropped). ON: the Support Rule also removes tiles that overhang or float.")]
    [SerializeField] private bool trimLevelAssets = false;

    [Header("Random Generator (Source = Random)")]
    [SerializeField] private MahjongRandomSettings randomSettings = new MahjongRandomSettings();

    [Header("Stacking")]
    [SerializeField] private MahjongStackSettings stackSettings = new MahjongStackSettings();

    [Header("Tile Count Limits (all sources)")]
    [Tooltip("0 = no limit. If a layout has more tiles, the outermost tiles of the top layer are removed first.")]
    [SerializeField] private int maxTileCount = 0;
    [Tooltip("Mahjong needs pairs, so an odd count loses one tile.")]
    [SerializeField] private bool forceEvenTileCount = true;

    [Header("Board Layout (world units)")]
    [Tooltip("XY_Vertical = upright wall (stack along Z). XZ_Flat = lying flat (stack along Y).")]
    [SerializeField] private MahjongBoardPlane plane = MahjongBoardPlane.XY_Vertical;
    [Tooltip("ON: measures the tile mesh (needs the MahjongTile component with Body Mesh assigned) and sets tile size and layer thickness automatically. OFF: uses the manual values below.")]
    [SerializeField] private bool autoSize = true;
    [Tooltip("Manual tile width / height along the board's columns / rows. Only used when Auto Size is OFF.")]
    [SerializeField] private Vector2 tileSize = new Vector2(1f, 1.3f);
    [Tooltip("Manual tile thickness. Only used when Auto Size is OFF.")]
    [SerializeField] private float manualLayerThickness = 0.3f;
    [Tooltip("Empty space between neighbouring tiles (world units). 0 = touching.")]
    [SerializeField] private Vector2 gap = new Vector2(0.05f, 0.05f);
    [Tooltip("Extra space between layers on top of the tile thickness. 0 = layers touch.")]
    [SerializeField] private float layerGap = 0f;
    [Tooltip("Extra sideways shift per layer along columns / rows (use this for the 3D look of the stacks in the reference image).")]
    [SerializeField] private Vector2 layerShift = Vector2.zero;
    [Tooltip("Flips which way layers stack.")]
    [SerializeField] private bool invertLayerDirection = false;
    [Tooltip("Centers the finished board on the Board Root.")]
    [SerializeField] private bool centerBoard = true;

    [Header("Per-Tile Transform")]
    [Tooltip("Added to the position of EVERY tile (in Board Root space).")]
    [SerializeField] private Vector3 tilePositionOffset = Vector3.zero;
    [Tooltip("Rotation (Euler degrees) applied to EVERY tile. Auto Size uses it, so set it first.")]
    [SerializeField] private Vector3 tileRotation = Vector3.zero;
    [Tooltip("Tick to force a scale on spawned tiles. Unticked = the prefab's own scale. Auto Size adapts to it.")]
    [SerializeField] private bool overrideScale = false;
    [SerializeField] private Vector3 tileScale = Vector3.one;

    [Header("Debug")]
    [SerializeField] private bool logInfo = true;

    private readonly List<MahjongTile> spawnedTiles = new List<MahjongTile>();
    private readonly List<Vector3Int> spawnedGridPositions = new List<Vector3Int>();

    // Used by the sorting method in FinalizeLayout.
    private float sortCenterX;
    private float sortCenterY;

    public int LastSeed { get; private set; }
    public int TileCount { get { return spawnedTiles.Count; } }

    // Both lists have the same order: SpawnedTiles[i] sits at LayoutPositions[i]
    // (x, y = position in HALF-tile units, so 2 = one whole tile; z = layer).
    public IReadOnlyList<MahjongTile> SpawnedTiles { get { return spawnedTiles; } }
    public IReadOnlyList<Vector3Int> LayoutPositions { get { return spawnedGridPositions; } }

    private Transform Parent
    {
        get
        {
            if (boardRoot != null)
            {
                return boardRoot;
            }
            return transform;
        }
    }

    private void Start()
    {
        if (generateOnStart)
        {
            Generate();
        }
    }

    // ---------- Public API ----------

    [ContextMenu("Generate")]
    public void Generate()
    {
        Clear();

        if (tilePrefab == null)
        {
            Debug.LogError("MahjongBoardSpawner: assign the Tile Prefab (it needs the MahjongTile component).");
            return;
        }

        List<Vector3Int> layout = null;
        MahjongSpawnSource activeSource = ResolveSource();

        switch (activeSource)
        {
            case MahjongSpawnSource.BuiltIn:
                layout = RemoveOverhangs(MahjongLayoutBuilder.BuildPattern(patternSettings, stackSettings));
                break;

            case MahjongSpawnSource.LevelAsset:
                List<Vector3Int> fromAsset = BuildFromAsset();
                if (fromAsset != null)
                {
                    if (trimLevelAssets)
                    {
                        layout = RemoveOverhangs(fromAsset);
                    }
                    else
                    {
                        layout = MahjongLayoutBuilder.RemoveOverlaps(fromAsset);
                    }
                }
                break;

            case MahjongSpawnSource.Random:
                layout = MahjongLayoutBuilder.BuildRandom(randomSettings, stackSettings, CreateRng());
                break;
        }

        if (layout == null || layout.Count == 0)
        {
            Debug.LogWarning("MahjongBoardSpawner: the layout is empty, nothing to spawn.");
            return;
        }

        FinalizeLayout(layout);
        Spawn(layout);
    }

    [ContextMenu("Generate Random Level")]
    public void GenerateRandom()
    {
        source = MahjongSpawnSource.Random;
        Generate();
    }

    [ContextMenu("Next Level Asset")]
    public void NextLevel()
    {
        if (levelAssets == null || levelAssets.Length == 0)
        {
            return;
        }
        source = MahjongSpawnSource.LevelAsset;
        levelIndex = (levelIndex + 1) % levelAssets.Length;
        Generate();
    }

    [ContextMenu("Previous Level Asset")]
    public void PreviousLevel()
    {
        if (levelAssets == null || levelAssets.Length == 0)
        {
            return;
        }
        source = MahjongSpawnSource.LevelAsset;
        levelIndex = (levelIndex - 1 + levelAssets.Length) % levelAssets.Length;
        Generate();
    }

    public void GenerateLevel(int index)
    {
        source = MahjongSpawnSource.LevelAsset;
        levelIndex = index;
        Generate();
    }

    public void SetPattern(MahjongBuiltInPattern newPattern)
    {
        source = MahjongSpawnSource.BuiltIn;
        patternSettings.pattern = newPattern;
        Generate();
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        Transform parent = Parent;
        Transform old = parent.Find(ContainerName);

        while (old != null)
        {
            old.name = ContainerName + "_Removed"; // so Find() cannot return it again
            if (Application.isPlaying)
            {
                old.gameObject.SetActive(false);
                Destroy(old.gameObject);
            }
            else
            {
                DestroyImmediate(old.gameObject);
            }
            old = parent.Find(ContainerName);
        }

        spawnedTiles.Clear();
        spawnedGridPositions.Clear();
    }

    // ---------- Layout sources ----------

    // Turns the Source dropdown into the source that is really used.
    // Auto prefers the hand-made level when one is assigned at Level Index.
    private MahjongSpawnSource ResolveSource()
    {
        if (source != MahjongSpawnSource.Auto)
        {
            return source;
        }

        if (HasLevelAssetSelected())
        {
            if (logInfo)
            {
                Debug.Log("MahjongBoardSpawner: Auto source -> using Level Asset " + levelIndex + ".");
            }
            return MahjongSpawnSource.LevelAsset;
        }

        if (logInfo)
        {
            Debug.Log("MahjongBoardSpawner: Auto source -> no Level Asset assigned, using the Built-In pattern.");
        }
        return MahjongSpawnSource.BuiltIn;
    }

    private bool HasLevelAssetSelected()
    {
        if (levelAssets == null || levelAssets.Length == 0)
        {
            return false;
        }

        int index = Mathf.Clamp(levelIndex, 0, levelAssets.Length - 1);
        return levelAssets[index] != null;
    }

    private List<Vector3Int> BuildFromAsset()
    {
        if (levelAssets == null || levelAssets.Length == 0)
        {
            Debug.LogError("MahjongBoardSpawner: no level assets assigned.");
            return null;
        }

        levelIndex = Mathf.Clamp(levelIndex, 0, levelAssets.Length - 1);
        MahjongLevelData data = levelAssets[levelIndex];
        if (data == null)
        {
            Debug.LogError("MahjongBoardSpawner: the level asset slot is empty.");
            return null;
        }

        return data.ToPositions(stackSettings);
    }

    private System.Random CreateRng()
    {
        LastSeed = randomSettings.seed;
        if (LastSeed == 0)
        {
            LastSeed = System.Environment.TickCount;
        }
        if (LastSeed == 0)
        {
            LastSeed = 1;
        }
        Debug.Log("MahjongBoardSpawner: random seed = " + LastSeed);
        return new System.Random(LastSeed);
    }

    // Removes tiles that overlap, overhang or float, according to the Stack Settings.
    private List<Vector3Int> RemoveOverhangs(List<Vector3Int> layout)
    {
        int before = layout.Count;
        List<Vector3Int> result = MahjongLayoutBuilder.RemoveUnsupported(layout, stackSettings);

        if (logInfo && result.Count < before)
        {
            Debug.Log("MahjongBoardSpawner: removed " + (before - result.Count) +
                      " tiles that overlapped another tile or had too little underneath them. " +
                      "Set Support Rule to AtLeastOne or None to keep more.");
        }
        return result;
    }

    // ---------- Positions ----------

    // Position of a tile in tile units (the stored x, y are half-tile units).
    private Vector2 CellPosition(Vector3Int p)
    {
        return new Vector2(p.x * 0.5f, p.y * 0.5f);
    }

    // Middle of the layout in tile units.
    private void GetCenter(List<Vector3Int> layout, out float centerX, out float centerY)
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (int i = 0; i < layout.Count; i++)
        {
            Vector2 c = CellPosition(layout[i]);
            minX = Mathf.Min(minX, c.x);
            maxX = Mathf.Max(maxX, c.x);
            minY = Mathf.Min(minY, c.y);
            maxY = Mathf.Max(maxY, c.y);
        }
        centerX = (minX + maxX) * 0.5f;
        centerY = (minY + maxY) * 0.5f;
    }

    // Layer first, then distance from the center, then x, then y.
    private int CompareForFinalLayout(Vector3Int a, Vector3Int b)
    {
        int byLayer = a.z.CompareTo(b.z);
        if (byLayer != 0)
        {
            return byLayer;
        }

        Vector2 positionA = CellPosition(a);
        Vector2 positionB = CellPosition(b);
        float distanceA = (positionA.x - sortCenterX) * (positionA.x - sortCenterX) +
                          (positionA.y - sortCenterY) * (positionA.y - sortCenterY);
        float distanceB = (positionB.x - sortCenterX) * (positionB.x - sortCenterX) +
                          (positionB.y - sortCenterY) * (positionB.y - sortCenterY);

        int byDistance = distanceA.CompareTo(distanceB);
        if (byDistance != 0)
        {
            return byDistance;
        }

        int byX = a.x.CompareTo(b.x);
        if (byX != 0)
        {
            return byX;
        }
        return a.y.CompareTo(b.y);
    }

    // Sorts the layout and applies the tile limits.
    // Removing from the END removes the outermost tiles of the TOP layer first,
    // so the shape stays neat and nothing is ever left floating.
    private void FinalizeLayout(List<Vector3Int> layout)
    {
        int before = layout.Count;

        GetCenter(layout, out sortCenterX, out sortCenterY);
        layout.Sort(CompareForFinalLayout);

        while (maxTileCount > 0 && layout.Count > maxTileCount)
        {
            layout.RemoveAt(layout.Count - 1);
        }

        if (forceEvenTileCount && layout.Count % 2 != 0)
        {
            layout.RemoveAt(layout.Count - 1);
        }

        if (layout.Count < before)
        {
            Debug.LogWarning("MahjongBoardSpawner: the layout had " + before + " tiles but " + layout.Count +
                             " were spawned (Max Tile Count / even count). Set Max Tile Count to 0 and turn off Force Even Tile Count to keep every tile.");
        }
    }

    // ---------- Sizes ----------

    // Works out the tile footprint and thickness in the BOARD's axes.
    private void ResolveSizes(Quaternion rotation, out Vector2 size, out float thickness)
    {
        size = tileSize;
        thickness = manualLayerThickness;

        if (!autoSize)
        {
            return;
        }

        if (!tilePrefab.HasBodyMesh)
        {
            Debug.LogWarning("MahjongBoardSpawner: Auto Size is ON but the MahjongTile has no Body Mesh assigned. Using the manual values.");
            return;
        }

        Vector3 body = tilePrefab.GetBodySize(); // along the tile's own axes

        if (overrideScale)
        {
            Vector3 rootScale = tilePrefab.transform.localScale;
            body.x *= Ratio(tileScale.x, rootScale.x);
            body.y *= Ratio(tileScale.y, rootScale.y);
            body.z *= Ratio(tileScale.z, rootScale.z);
        }

        // Rotate into board space: the size along each board axis = |rotation matrix| * body size.
        Matrix4x4 m = Matrix4x4.Rotate(rotation);
        float boardSizeX = Mathf.Abs(m.m00) * body.x + Mathf.Abs(m.m01) * body.y + Mathf.Abs(m.m02) * body.z;
        float boardSizeY = Mathf.Abs(m.m10) * body.x + Mathf.Abs(m.m11) * body.y + Mathf.Abs(m.m12) * body.z;
        float boardSizeZ = Mathf.Abs(m.m20) * body.x + Mathf.Abs(m.m21) * body.y + Mathf.Abs(m.m22) * body.z;

        if (plane == MahjongBoardPlane.XY_Vertical)
        {
            size = new Vector2(boardSizeX, boardSizeY);
            thickness = boardSizeZ;
        }
        else
        {
            size = new Vector2(boardSizeX, boardSizeZ);
            thickness = boardSizeY;
        }
    }

    private static float Ratio(float a, float b)
    {
        if (Mathf.Abs(b) < 0.000001f)
        {
            return 1f;
        }
        return Mathf.Abs(a / b);
    }

    // ---------- Spawning ----------

    private void Spawn(List<Vector3Int> layout)
    {
        Quaternion rotation = Quaternion.Euler(tileRotation);

        Vector2 size;
        float thickness;
        ResolveSizes(rotation, out size, out thickness);

        if (logInfo)
        {
            string sizeSource = "(manual)";
            if (autoSize)
            {
                sizeSource = "(measured)";
            }
            Debug.Log("MahjongBoardSpawner: tile footprint = " + size + ", tile thickness = " + thickness +
                      " " + sizeSource + ", tiles = " + layout.Count +
                      ", stack style = " + stackSettings.style +
                      ", support rule = " + stackSettings.supportRule);
        }

        Transform container = new GameObject(ContainerName).transform;
        container.SetParent(Parent, false);

        float centerX = 0f;
        float centerY = 0f;
        if (centerBoard)
        {
            GetCenter(layout, out centerX, out centerY);
        }

        float stepX = size.x + gap.x;
        float stepY = size.y + gap.y;
        float layerStep = thickness + layerGap;

        float stackSign = 1f;
        if (plane == MahjongBoardPlane.XY_Vertical)
        {
            stackSign = -1f;
        }
        if (invertLayerDirection)
        {
            stackSign = -stackSign;
        }

        for (int i = 0; i < layout.Count; i++)
        {
            Vector3Int p = layout[i];
            Vector2 cell = CellPosition(p); // half-tile positions already included

            float u = (cell.x - centerX) * stepX + p.z * layerShift.x;   // along columns
            float v = (cell.y - centerY) * stepY + p.z * layerShift.y;   // along rows
            float depth = p.z * layerStep * stackSign;                   // along the stacking axis

            Vector3 local;
            if (plane == MahjongBoardPlane.XY_Vertical)
            {
                local = new Vector3(u, v, depth);
            }
            else
            {
                local = new Vector3(u, depth, v);
            }

            MahjongTile tile = Instantiate(tilePrefab, container);
            tile.transform.localPosition = local + tilePositionOffset;
            tile.transform.localRotation = rotation;
            if (overrideScale)
            {
                tile.transform.localScale = tileScale;
            }
            tile.name = "Tile_" + p.x + "_" + p.y + "_" + p.z;

            spawnedTiles.Add(tile);
            spawnedGridPositions.Add(p);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }
}