using UnityEngine;

/// <summary>A place on the board. Shuffling swaps spots between tiles.</summary>
public struct TileSpot
{
    public int layer;
    public Vector2 cell;
    public Vector3 position;
    public Quaternion rotation;
}