using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sends the most recently picked tile back from the tray to its board spot.
/// Remembers the last N picked tiles (default 6). Tiles that already merged are gone and cannot be undone.
/// </summary>
public class UndoBooster : BoosterBase
{
    [Header("Undo")]
    [SerializeField] private int maxUndoSteps = 6;

    private readonly List<Tile> history = new List<Tile>();
    private readonly List<Tile> boardTiles = new List<Tile>();

    public override BoosterType Type => BoosterType.Undo;

    private void OnEnable()
    {
        TileTrayManager.tilePickedUp += OnTilePickedUp;
        TileTrayManager.startMerge += OnStartMerge;
    }

    private void OnDisable()
    {
        TileTrayManager.tilePickedUp -= OnTilePickedUp;
        TileTrayManager.startMerge -= OnStartMerge;
    }

    public override void ResetForNewLevel()
    {
        history.Clear();
    }

    private void OnTilePickedUp(Tile tile)
    {
        if (tile == null) return;

        history.Remove(tile);
        history.Add(tile);

        while (history.Count > maxUndoSteps)
            history.RemoveAt(0);
    }

    private void OnStartMerge(List<Tile> tiles)
    {
        if (tiles == null) return;
        for (int i = 0; i < tiles.Count; i++) history.Remove(tiles[i]);
    }

    private void PruneHistory()
    {
        for (int i = history.Count - 1; i >= 0; i--)
        {
            Tile t = history[i];
            if (t == null || t.Slot == null) history.RemoveAt(i);
        }
    }

    public override bool CanExecute()
    {
        PruneHistory();
        return history.Count > 0;
    }

    public override void Execute(Action<bool> onFinished)
    {
        PruneHistory();
        if (history.Count == 0)
        {
            onFinished(false);
            return;
        }

        Tile tile = history[history.Count - 1];
        history.RemoveAt(history.Count - 1);

        if (!Manager.Tray.RemoveTileFromTray(tile))
        {
            onFinished(false);
            return;
        }

        // ReturnTileToBoard frees the tile's tray slot immediately, so it counts as a board tile right after.
        Manager.Arrangement.ReturnTileToBoard(tile, () => onFinished(true));

        Manager.CollectBoardTiles(boardTiles);
        Manager.Board.RebuildBlocking(boardTiles);
    }
}