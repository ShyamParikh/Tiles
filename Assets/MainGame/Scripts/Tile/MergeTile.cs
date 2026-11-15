using UnityEngine;

/// <summary>
/// When a set of 3 containing this tile merges, SpecialTileManager also merges extra random sets from the board.
/// The tile itself only stores how many; the manager does the work.
/// </summary>
public class MergeBurstTile : Tile
{
    [Header("Merge Burst Tile")]
    [Tooltip("How many extra sets of 3 get merged (randomly, from the board) when this tile is merged.")]
    [SerializeField] private int burstSetCount = 3;

    public int BurstSetCount => Mathf.Max(0, burstSetCount);
}