using System;
using System.Collections.Generic;
using UnityEngine;

// Sends the last picked tile back from the tray to its spot on the board.
public class UndoBooster : BoosterBase
{
    [Tooltip("How many of the latest picks can be undone.")]
    [SerializeField] private int maxUndoSteps = 6;

    private readonly List<Tile> history = new List<Tile>();

    public override BoosterType Type => BoosterType.Undo;

    private void OnEnable()
    {
        TileTrayManager.tilePickedUp += OnPicked;
        TileTrayManager.startMerge += OnMerged;
    }

    private void OnDisable()
    {
        TileTrayManager.tilePickedUp -= OnPicked;
        TileTrayManager.startMerge -= OnMerged;
    }

    public override void ResetForNewLevel() { history.Clear(); }

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

    public override bool CanExecute()
    {
        Prune();
        return history.Count > 0;
    }

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
}