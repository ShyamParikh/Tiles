/*
 * ShuffleBooster
 * --------------
 * Swaps the spots of every tile on the board (tiles in the tray are untouched), then asks the board manager to
 * work out again which tiles are covered and plays the gather-spin-spread animation. The tiles keep their faces
 * and types, only their positions change, so the layout stays valid. It needs at least two tiles on the board.
 * The charge is taken by BoosterManager when the animation finishes.
 */
using System;
using System.Collections.Generic;

public class ShuffleBooster : BoosterBase
{
    #region Abstract Implementations

    // This is the shuffle booster.
    public override BoosterType Type => BoosterType.Shuffle;

    // There must be at least two tiles to swap.
    public override bool CanExecute()
    {
        return Manager.Board.Tiles.Count >= 2;
    }

    // Shuffles the spots, rebuilds the covering, and plays the animation.
    public override void Execute(Action<bool> onFinished)
    {
        List<Tile> tiles = new List<Tile>(Manager.Board.Tiles);

        TileSpot[] spots = new TileSpot[tiles.Count];
        for (int i = 0; i < spots.Length; i++) spots[i] = tiles[i].Spot;
        TileRules.Shuffle(spots, new Random());

        for (int i = 0; i < spots.Length; i++) tiles[i].SetSpot(spots[i]);

        Manager.Board.Rebuild();
        Manager.Arrangement.PlayShuffle(tiles, () => onFinished(true));
    }

    #endregion
}