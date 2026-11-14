using System.Collections.Generic;

/// <summary>
/// The tiles with the same face that are currently sitting in the tray.
/// TileTrayManager keeps one of these per face id in a dictionary.
/// </summary>
public class TileMergeData
{
    // How many identical tiles make a match. Used by the spawner and the editor too.
    public const int TilesPerMerge = 3;

    public readonly int faceId;
    public readonly List<Tile> tiles = new List<Tile>();

    public TileMergeData(Tile firstTile)
    {
        faceId = firstTile.FaceId;
        tiles.Add(firstTile);
    }

    public void Add(Tile tile)
    {
        tiles.Add(tile);
    }

    public bool CanMerge()
    {
        return tiles.Count >= TilesPerMerge;
    }
}