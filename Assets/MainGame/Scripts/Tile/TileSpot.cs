using UnityEngine;

// One spot on the board. Shuffling just swaps these between tiles.
public struct TileSpot
{
    public int layer;
    public Vector2 cell;
    public Vector3 position;
    public Quaternion rotation;
}