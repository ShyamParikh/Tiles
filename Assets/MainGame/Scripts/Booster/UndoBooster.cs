using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Sends the last picked tile back from the tray to its board spot (remembers the last N picks).</summary>
public class UndoBooster : BoosterBase
{
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

    /// <summary>Merged tiles are gone and cannot be undone.</summary>
    private void OnMerged(List<Tile> group)
    {
        foreach (Tile tile in group) history.Remove(tile);
    }

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
        if (history.Count == 0)
        {
            onFinished(false);
            return;
        }

        Tile tile = history[history.Count - 1];
        history.RemoveAt(history.Count - 1);

        if (!Manager.Tray.RemoveTile(tile))
        {
            onFinished(false);
            return;
        }

        Manager.Arrangement.ReturnTileToBoard(tile, () => onFinished(true));
        Manager.Board.ReturnTile(tile);
    }
}