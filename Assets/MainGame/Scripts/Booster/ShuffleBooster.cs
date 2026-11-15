using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shuffles the positions of every tile still on the board (tiles in the tray are untouched),
/// then rebuilds which tiles are covered.
/// </summary>
public class ShuffleBooster : BoosterBase
{
    private struct Placement
    {
        public int layer;
        public Vector2 cell;
        public Vector3 localPosition;
        public Quaternion localRotation;
    }

    private readonly List<Tile> boardTiles = new List<Tile>();

    public override BoosterType Type => BoosterType.Shuffle;

    public override bool CanExecute()
    {
        Manager.CollectBoardTiles(boardTiles);
        return boardTiles.Count >= 2;
    }

    public override void Execute(Action<bool> onFinished)
    {
        Manager.CollectBoardTiles(boardTiles);
        if (boardTiles.Count < 2)
        {
            onFinished(false);
            return;
        }

        List<Placement> placements = new List<Placement>(boardTiles.Count);
        for (int i = 0; i < boardTiles.Count; i++)
        {
            Tile t = boardTiles[i];
            placements.Add(new Placement
            {
                layer = t.LayerIndex,
                cell = t.CellPosition,
                localPosition = t.RestLocalPosition,
                localRotation = t.RestLocalRotation
            });
        }

        for (int i = placements.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            Placement temp = placements[i];
            placements[i] = placements[j];
            placements[j] = temp;
        }

        for (int i = 0; i < boardTiles.Count; i++)
        {
            Placement p = placements[i];
            boardTiles[i].SetBoardPlacement(p.layer, p.cell, p.localPosition, p.localRotation);
        }

        Manager.Board.RebuildBlocking(boardTiles);
        Manager.Arrangement.PlayShuffle(boardTiles, () => onFinished(true));
    }
}