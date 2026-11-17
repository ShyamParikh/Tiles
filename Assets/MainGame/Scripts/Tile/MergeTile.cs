using System;
using System.Collections.Generic;
using UnityEngine;

// When a set containing this tile merges, it asks for extra random sets to merge (MergeBurstManager does the work).
public class MergeBurstTile : Tile
{
    public static Action<int> burstRequested;

    [Tooltip("How many extra sets of 3 get merged.")]
    [SerializeField] private int burstSetCount = 3;

    // If a merge has several burst tiles, only the first one speaks up, so they don't stack.
    public override void OnMergeStarted(List<Tile> group)
    {
        MergeBurstTile first = group.Find(t => t is MergeBurstTile) as MergeBurstTile;
        if (first == this) burstRequested?.Invoke(burstSetCount);
    }
}