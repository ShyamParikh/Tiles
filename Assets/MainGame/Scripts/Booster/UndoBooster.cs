/*
 * UndoBooster
 * -----------
 * Sends the last picked tile back from the tray to its spot on the board. It remembers the latest picks (Max
 * Undo Steps, 6 by default) by listening to tilePickedUp, and forgets tiles that already merged (they are
 * gone for good) and tiles that are no longer in the tray. Executing it takes the tile out of the tray (the
 * tiles after it slide left), flies it back with the arrangement manager and tells the board manager the tile
 * is back so the covering is worked out again. The charge is taken by BoosterManager when the flight is done.
 */
using System;
using System.Collections.Generic;
using UnityEngine;

public class UndoBooster : BoosterBase
{
    #region Private Variables

    [Tooltip("How many of the latest picks can be undone.")]
    [SerializeField] private int maxUndoSteps = 6;

    private readonly List<Tile> history = new List<Tile>();

    #endregion

    #region Unity Callbacks

    // Starts listening for picks and merges.
    private void OnEnable()
    {
        TileTrayManager.tilePickedUp += OnPicked;
        TileTrayManager.startMerge += OnMerged;
    }

    // Stops listening.
    private void OnDisable()
    {
        TileTrayManager.tilePickedUp -= OnPicked;
        TileTrayManager.startMerge -= OnMerged;
    }

    #endregion

    #region History

    // Remembers a picked tile (and forgets the oldest if the history is full).
    private void OnPicked(Tile tile)
    {
        history.Remove(tile);
        history.Add(tile);
        if (history.Count > maxUndoSteps) history.RemoveAt(0);
    }

    // Merged tiles are gone for good, so they can't be undone.
    private void OnMerged(List<Tile> group)
    {
        group.ForEach(t => history.Remove(t));
    }

    // Only tiles that are still sitting in the tray can be undone.
    private void Prune()
    {
        history.RemoveAll(t => t == null || t.Slot == null);
    }

    #endregion

    #region Abstract Implementations

    // This is the undo booster.
    public override BoosterType Type => BoosterType.Undo;

    // A new level starts with an empty history.
    public override void ResetForNewLevel() { history.Clear(); }

    // There must be at least one tile to undo.
    public override bool CanExecute()
    {
        Prune();
        return history.Count > 0;
    }

    // Takes the last tile out of the tray and flies it back to the board.
    public override void Execute(Action<bool> onFinished)
    {
        Prune();
        Tile tile = history.Count > 0 ? history[history.Count - 1] : null;

        if (tile == null || !Manager.Tray.RemoveTile(tile))
        {
            onFinished(false);
            return;
        }

        history.Remove(tile);
        Manager.Arrangement.ReturnTileToBoard(tile, () => onFinished(true));
        Manager.Board.ReturnTile(tile);
    }

    #endregion
}