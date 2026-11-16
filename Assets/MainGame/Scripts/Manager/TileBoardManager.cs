using System.Collections.Generic;
using com.otm.manager;
using UnityEngine;

public enum BoardMovableRule
{
    TopTilesMovable,
    BottomTilesMovable
}

public enum BoardTopMode
{
    AutoFromSpawner,
    CustomDirection,
    TowardsTopReference
}

/// <summary>Knows which tiles are on the board and which of them are covered.</summary>
public class TileBoardManager : Manager<TileBoardManager>
{
    [Header("Scene References (Assign in Inspector)")]
    [SerializeField] private TileVisualSpawner spawner;

    [Header("Movable Tile Rule")]
    [Tooltip("TopTilesMovable = outer/top tiles move first. BottomTilesMovable = bottom/inner tiles move first.")]
    [SerializeField] private BoardMovableRule movableRule = BoardMovableRule.TopTilesMovable;

    [Header("Top & Stacking Orientation")]
    [SerializeField] private BoardTopMode topMode = BoardTopMode.AutoFromSpawner;
    [SerializeField] private bool invertTopDirection = false;
    [Tooltip("Used when Top Mode = CustomDirection. Example: (0,0,-1) or (0,1,0).")]
    [SerializeField] private Vector3 customTopDirection = Vector3.back;
    [Tooltip("Used when Top Mode = TowardsTopReference. The tile closer to it is on top.")]
    [SerializeField] private Transform topReferenceTransform;

    [Header("Overlap Sensitivity")]
    [Tooltip("0.999 = even a tiny overlap blocks the tile (side-by-side tiles at 1.0 do not).")]
    [SerializeField] private float overlapThreshold = 0.999f;
    [SerializeField] private float minHeightDifference = 0.001f;

    private readonly List<Tile> tiles = new List<Tile>();

    /// <summary>The tiles currently lying on the board (not in the tray).</summary>
    public IReadOnlyList<Tile> Tiles => tiles;
    public int RemainingTiles => tiles.Count;

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
        if (topMode == BoardTopMode.AutoFromSpawner && spawner == null) Debug.LogError("[BOARD] 'Spawner' is not assigned!", this);
        if (topMode == BoardTopMode.TowardsTopReference && topReferenceTransform == null) Debug.LogError("[BOARD] 'Top Reference Transform' is not assigned!", this);
    }

    private void OnBoardGenerated(List<Tile> generated)
    {
        tiles.Clear();
        tiles.AddRange(generated);
        Rebuild();
    }

    /// <summary>Undo: a tile is back on the board.</summary>
    public void ReturnTile(Tile tile)
    {
        tiles.Add(tile);
        Rebuild();
    }

    /// <summary>Recalculates which tiles cover which. Call after tiles changed their spot (Shuffle).</summary>
    public void Rebuild()
    {
        Vector3 axis = TopAxis();

        foreach (Tile tile in tiles)
        {
            tile.Covered.Clear();
            tile.CoverCount = 0;
        }

        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles.Count; j++)
            {
                if (i == j || !Covers(tiles[j], tiles[i], axis)) continue;
                tiles[j].Covered.Add(tiles[i]);
                tiles[i].CoverCount++;
            }
        }

        bool anyFree = false;
        foreach (Tile tile in tiles)
        {
            tile.SetBlocked(tile.CoverCount > 0);
            anyFree |= tile.CoverCount == 0;
        }

        if (tiles.Count > 0 && !anyFree) Debug.LogError("[BOARD] ALL TILES ARE BLOCKED! Check Movable Rule / Top Mode / Invert Top Direction.", this);
    }

    private void OnTilePickedUp(Tile tile)
    {
        tiles.Remove(tile);

        foreach (Tile lower in tile.Covered)
        {
            if (lower != null && --lower.CoverCount <= 0) lower.SetBlocked(false);
        }
    }

    private bool Covers(Tile upper, Tile lower, Vector3 axis)
    {
        float height = Score(upper, axis) - Score(lower, axis);
        if (movableRule == BoardMovableRule.BottomTilesMovable) height = -height;

        Vector2 delta = upper.CellPosition - lower.CellPosition;
        return height > minHeightDifference && Mathf.Abs(delta.x) < overlapThreshold && Mathf.Abs(delta.y) < overlapThreshold;
    }

    private Vector3 TopAxis()
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

        return invertTopDirection ? -axis : axis;
    }

    /// <summary>Uses the rest position, so it stays correct while tiles are animating.</summary>
    private float Score(Tile tile, Vector3 axis)
    {
        if (topMode == BoardTopMode.TowardsTopReference && topReferenceTransform != null)
        {
            float distance = Vector3.Distance(tile.transform.position, topReferenceTransform.position);
            return invertTopDirection ? distance : -distance;
        }
        return Vector3.Dot(tile.RestLocalPosition, axis);
    }
}