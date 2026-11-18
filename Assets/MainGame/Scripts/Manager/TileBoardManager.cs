/*
 * TileBoardManager
 * ----------------
 * Keeps track of the tiles lying on the board and which of them are covered by another tile. Two tiles
 * overlap when their cells are closer than the overlap threshold, and the higher one covers the lower one.
 * "Higher" depends on the movable rule (the usual top-tiles rule, or its reverse) and on what counts as "on
 * top": the spawner's own stacking direction, a custom direction, or whichever tile is closer to a reference
 * object. It writes Covered and CoverCount on the tiles and dims or frees them. Picking a tile up frees the
 * tiles under it. Rebuild runs again after Shuffle, and ReturnTile is used by Undo. Boosters read Tiles, and
 * the game manager reads RemainingTiles to detect a win.
 */
using System.Collections.Generic;
using com.otm.manager;
using UnityEngine;

public enum BoardMovableRule { TopTilesMovable, BottomTilesMovable }
public enum BoardTopMode { AutoFromSpawner, CustomDirection, TowardsTopReference }

public class TileBoardManager : Manager<TileBoardManager>
{
    #region Private Variables

    [SerializeField] private TileVisualSpawner spawner;

    [Header("Which tiles can be picked")]
    [Tooltip("TopTilesMovable = the usual rule (only uncovered tiles). BottomTilesMovable flips it.")]
    [SerializeField] private BoardMovableRule movableRule;

    [Header("What counts as 'on top'")]
    [SerializeField] private BoardTopMode topMode;
    [Tooltip("Flips which side is 'on top'.")]
    [SerializeField] private bool invertTopDirection;
    [Tooltip("Used by CustomDirection, e.g. (0,0,-1).")]
    [SerializeField] private Vector3 customTopDirection = Vector3.back;
    [Tooltip("Used by TowardsTopReference: the tile closer to this is on top.")]
    [SerializeField] private Transform topReferenceTransform;

    [Header("Overlap")]
    [Tooltip("In cells. 0.999 = even a tiny overlap covers the tile. Side-by-side tiles (exactly 1 apart) never do.")]
    [SerializeField] private float overlapThreshold = 0.999f;
    [SerializeField] private float minHeightDifference = 0.001f;

    private readonly List<Tile> tiles = new List<Tile>();

    #endregion

    #region Properties

    // The tiles currently lying on the board (not in the tray).
    public IReadOnlyList<Tile> Tiles => tiles;
    public int RemainingTiles => tiles.Count;

    #endregion

    #region Unity Callbacks

    // Starts listening for a new board and for picked-up tiles.
    private void OnEnable()
    {
        TileVisualSpawner.boardGenerated += OnBoardGenerated;
        TileTrayManager.tilePickedUp += OnTilePickedUp;
    }

    // Stops listening.
    private void OnDisable()
    {
        TileVisualSpawner.boardGenerated -= OnBoardGenerated;
        TileTrayManager.tilePickedUp -= OnTilePickedUp;
    }

    #endregion

    #region Board Changes

    // A new board was built: take its tiles and work out who covers whom.
    private void OnBoardGenerated(List<Tile> generated)
    {
        tiles.Clear();
        tiles.AddRange(generated);
        Rebuild();
    }

    // Undo put a tile back on the board.
    public void ReturnTile(Tile tile)
    {
        tiles.Add(tile);
        Rebuild();
    }

    // A tile was picked up: remove it and uncover the tiles that were lying under it.
    private void OnTilePickedUp(Tile tile)
    {
        tiles.Remove(tile);

        foreach (Tile lower in tile.Covered)
        {
            if (lower != null && --lower.CoverCount <= 0) lower.SetBlocked(false);
        }
    }

    #endregion

    #region Covering

    // Works out again who covers whom. Call it whenever tiles changed their spot (Shuffle).
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

        foreach (Tile tile in tiles) tile.SetBlocked(tile.CoverCount > 0);
    }

    // True when 'upper' sits higher than 'lower' and their cells overlap.
    private bool Covers(Tile upper, Tile lower, Vector3 axis)
    {
        float height = Score(upper, axis) - Score(lower, axis);
        if (movableRule == BoardMovableRule.BottomTilesMovable) height = -height;

        Vector2 delta = upper.CellPosition - lower.CellPosition;
        return height > minHeightDifference && Mathf.Abs(delta.x) < overlapThreshold && Mathf.Abs(delta.y) < overlapThreshold;
    }

    // The direction that counts as "up" for the board.
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

    // How high a tile is. Uses the resting position, so it stays right while tiles are animating.
    private float Score(Tile tile, Vector3 axis)
    {
        if (topMode == BoardTopMode.TowardsTopReference && topReferenceTransform != null)
        {
            float distance = Vector3.Distance(tile.transform.position, topReferenceTransform.position);
            return invertTopDirection ? distance : -distance;
        }
        return Vector3.Dot(tile.RestLocalPosition, axis);
    }

    #endregion
}