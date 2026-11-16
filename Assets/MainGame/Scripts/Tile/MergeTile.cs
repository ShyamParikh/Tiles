using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>When a set containing this tile merges, it asks for extra random sets to merge (see MergeBurstManager).</summary>
public class MergeBurstTile : Tile
{
    public static Action<int> burstRequested;

    [Tooltip("How many extra sets of 3 get merged when this tile is merged.")]
    [SerializeField] private int burstSetCount = 3;

    /// <summary>Only the first burst tile of a merge fires, so 3 burst tiles do not triple the burst.</summary>
    protected override void OnMergeStarted(List<Tile> group)
    {
        for (int i = 0; i < group.Count; i++)
        {
            MergeBurstTile burst = group[i] as MergeBurstTile;
            if (burst == null) continue;

            if (burst == this) burstRequested?.Invoke(burstSetCount);
            return;
        }
    }
}