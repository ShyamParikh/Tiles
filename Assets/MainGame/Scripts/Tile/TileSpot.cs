/*
 * TileSpot
 * --------
 * One place on the board: its layer, its cell, and the exact local position and rotation there.
 * Shuffling the board never moves tiles at random. It swaps these spots between tiles, so the layout
 * always stays valid. Tile.Spot reads a tile's spot and Tile.SetSpot gives it a new one.
 */
using UnityEngine;

public struct TileSpot
{
    #region Public Variables

    public int layer;
    public Vector2 cell;
    public Vector3 position;
    public Quaternion rotation;

    #endregion
}