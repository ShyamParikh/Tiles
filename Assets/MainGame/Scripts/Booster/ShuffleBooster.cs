using System;
using System.Collections.Generic;

// Swaps the spots of every tile on the board, then works out again which tiles are covered.
public class ShuffleBooster : BoosterBase
{
    public override BoosterType Type => BoosterType.Shuffle;

    public override bool CanExecute()
    {
        return Manager.Board.Tiles.Count >= 2;
    }

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
}