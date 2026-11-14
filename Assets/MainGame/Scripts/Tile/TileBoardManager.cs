using System.Collections.Generic;
using com.otm.manager;
using UnityEngine;

public enum BoardMovableRule
{
    TopTilesMovable,     // Tiles on top can move; any tile covered above is blocked
    BottomTilesMovable   // Tiles on bottom can move; any tile covered below is blocked
}

public enum BoardTopMode
{
    AutoFromSpawner,        // Uses Spawner's BoardPlane and InvertStackingDirection automatically
    CustomDirection,        // Uses Custom Top Direction vector in board local space
    TowardsTopReference     // Whichever tile is physically closer to Top Reference Transform is on top
}

public class TileBoardManager : Manager<TileBoardManager>
{
    [Header("Scene References (Assign in Inspector)")]
    [Tooltip("Drag your TileVisualSpawner here.")]
    [SerializeField] private TileVisualSpawner spawner;

    [Header("Movable Tile Rule (Top vs Bottom)")]
    [Tooltip("TopTilesMovable = outer/top tiles move first. BottomTilesMovable = bottom/inner tiles move first.")]
    [SerializeField] private BoardMovableRule movableRule = BoardMovableRule.TopTilesMovable;

    [Header("Top & Stacking Orientation")]
    [SerializeField] private BoardTopMode topMode = BoardTopMode.AutoFromSpawner;
    [Tooltip("Flips which side is considered 'on top' (useful if XY_Vertical faces the opposite camera direction).")]
    [SerializeField] private bool invertTopDirection = false;
    [Tooltip("Used when Top Mode = CustomDirection. Example: (0,0,-1) or (0,1,0).")]
    [SerializeField] private Vector3 customTopDirection = Vector3.back;
    [Tooltip("Used when Top Mode = TowardsTopReference. Drag your Camera or a Top Marker transform here.")]
    [SerializeField] private Transform topReferenceTransform;

    [Header("Overlap Sensitivity")]
    [Tooltip("0.999 = even a tiny 1% overlap locks the tile so it wiggles instead of moving (side-by-side tiles at 1.0 do NOT block each other).")]
    [SerializeField] private float overlapThreshold = 0.999f;
    [Tooltip("Minimum height difference along the Top axis for one tile to block another.")]
    [SerializeField] private float minHeightDifference = 0.001f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    // blockerTile -> tiles that become free when blockerTile is removed
    private readonly Dictionary<Tile, List<Tile>> dependentTiles = new Dictionary<Tile, List<Tile>>();
    // blockedTile -> tiles currently blocking it
    private readonly Dictionary<Tile, List<Tile>> blockedByTiles = new Dictionary<Tile, List<Tile>>();
    private readonly Dictionary<Tile, int> blockerCount = new Dictionary<Tile, int>();

    private int remainingTiles;

    public int RemainingTiles => remainingTiles;
    public BoardMovableRule MovableRule => movableRule;

    private void OnEnable()
    {
        TileVisualSpawner.boardGenerated += OnBoardGenerated;
        TileTrayManager.tilePickedUp += OnTilePickedUp;
    }

    private void OnDisable()
    {
        TileVisualSpawner.boardGenerated -= OnBoardGenerated;
        TileTrayManager.tilePickedUp -= OnTilePickedUp;
    }

    private void Start()
    {
        if (topMode == BoardTopMode.AutoFromSpawner && spawner == null)
            Debug.LogError("[BOARD] 'Spawner' is not assigned on TileBoardManager! Assign it in the Inspector.", this);

        if (topMode == BoardTopMode.TowardsTopReference && topReferenceTransform == null)
            Debug.LogError("[BOARD] 'Top Reference Transform' is not assigned on TileBoardManager!", this);
    }

    private void OnBoardGenerated(List<Tile> tiles)
    {
        dependentTiles.Clear();
        blockedByTiles.Clear();
        blockerCount.Clear();
        remainingTiles = 0;

        Vector3 topAxis = ResolveLocalTopAxis();

        foreach (Tile tile in tiles)
        {
            if (tile == null) continue;

            dependentTiles[tile] = new List<Tile>();
            blockedByTiles[tile] = new List<Tile>();
            blockerCount[tile] = 0;
            remainingTiles++;
        }

        for (int i = 0; i < tiles.Count; i++)
        {
            Tile candidate = tiles[i];
            if (candidate == null) continue;
            float candidateHeight = GetTileTopScore(candidate, topAxis);

            for (int j = 0; j < tiles.Count; j++)
            {
                Tile other = tiles[j];
                if (other == null || i == j) continue;

                float otherHeight = GetTileTopScore(other, topAxis);

                bool isBlockedByOther = movableRule == BoardMovableRule.TopTilesMovable
                    ? otherHeight > candidateHeight + minHeightDifference   // 'other' is above 'candidate'
                    : otherHeight < candidateHeight - minHeightDifference;  // 'other' is below 'candidate'

                if (!isBlockedByOther) continue;
                if (!Overlaps(candidate, other)) continue;

                dependentTiles[other].Add(candidate);
                blockedByTiles[candidate].Add(other);
                blockerCount[candidate]++;
            }
        }

        int freeCount = 0;
        int blockedTotal = 0;

        foreach (Tile tile in tiles)
        {
            if (tile == null) continue;
            bool blocked = blockerCount[tile] > 0;
            tile.SetBlocked(blocked);

            if (blocked) blockedTotal++;
            else freeCount++;
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[BOARD] Generated {remainingTiles} tiles | Rule: {movableRule} | TopAxis: {topAxis} | Movable: {freeCount} | Blocked: {blockedTotal}");
            if (remainingTiles > 0 && freeCount == 0)
            {
                Debug.LogError("[BOARD] ALL TILES ARE BLOCKED! Check Movable Rule / Top Mode / Invert Top Direction on TileBoardManager.");
            }
        }
    }

    private Vector3 ResolveLocalTopAxis()
    {
        Vector3 axis = Vector3.up;

        if (topMode == BoardTopMode.AutoFromSpawner && spawner != null)
        {
            axis = spawner.BoardPlane == TileBoardPlane.XZ_Flat ? Vector3.up : Vector3.forward;
            if (spawner.InvertStackingDirection) axis = -axis;
        }
        else if (topMode == BoardTopMode.CustomDirection)
        {
            axis = customTopDirection.sqrMagnitude > 0.0001f ? customTopDirection.normalized : Vector3.back;
        }

        if (invertTopDirection) axis = -axis;
        return axis;
    }

    private float GetTileTopScore(Tile tile, Vector3 localTopAxis)
    {
        if (topMode == BoardTopMode.TowardsTopReference && topReferenceTransform != null)
        {
            float dist = Vector3.Distance(tile.transform.position, topReferenceTransform.position);
            return invertTopDirection ? dist : -dist;
        }

        return Vector3.Dot(tile.transform.localPosition, localTopAxis);
    }

    private bool Overlaps(Tile a, Tile b)
    {
        Vector2 delta = a.CellPosition - b.CellPosition;
        return Mathf.Abs(delta.x) < overlapThreshold && Mathf.Abs(delta.y) < overlapThreshold;
    }

    private void OnTilePickedUp(Tile tile)
    {
        remainingTiles--;

        if (!dependentTiles.TryGetValue(tile, out List<Tile> freedCandidates)) return;

        foreach (Tile candidate in freedCandidates)
        {
            if (candidate == null) continue;

            blockedByTiles[candidate].Remove(tile);
            blockerCount[candidate]--;
            if (blockerCount[candidate] <= 0)
            {
                candidate.SetBlocked(false);
                if (enableDebugLogs) Debug.Log($"[BOARD] Tile '{candidate.name}' is now UNBLOCKED and movable.");
            }
        }
    }

    public string GetBlockerNames(Tile tile)
    {
        if (tile == null || !blockedByTiles.TryGetValue(tile, out List<Tile> list) || list.Count == 0)
            return "None";

        List<string> names = new List<string>(list.Count);
        foreach (Tile t in list)
        {
            if (t != null) names.Add($"{t.name} (L{t.LayerIndex})");
        }
        return string.Join(", ", names);
    }
}