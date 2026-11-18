/*
 * MergeBurstTile
 * --------------
 * A tile that, when the set it belongs to merges, asks for extra random sets to be merged too. It does not do
 * the merging itself: it raises the static burstRequested event with the number of extra sets, and
 * MergeBurstManager listens and runs the effect. If one merge contains several burst tiles, only the first
 * one speaks up, so three burst tiles do not triple the burst. Set the count in the inspector (3 by default).
 */
using System;
using System.Collections.Generic;
using UnityEngine;

public class MergeBurstTile : Tile
{
    #region Public Variables

    // Fired with how many extra sets should be merged.
    public static Action<int> burstRequested;

    #endregion

    #region Private Variables

    [Tooltip("How many extra sets of 3 get merged.")]
    [SerializeField] private int burstSetCount = 3;

    #endregion

    #region Overridable Hooks

    // The set started merging: the first burst tile in it asks for the burst.
    public override void OnMergeStarted(List<Tile> group)
    {
        MergeBurstTile first = group.Find(t => t is MergeBurstTile) as MergeBurstTile;
        if (first == this) burstRequested?.Invoke(burstSetCount);
    }

    #endregion
}