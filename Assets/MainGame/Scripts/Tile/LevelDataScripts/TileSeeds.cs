/*
 * TileSeeds
 * ---------
 * Seed saving for levels. A TileSeedEntry is one saved board layout: three numbers (content, face and
 * placement) that rebuild exactly the same faces, tile types and positions, plus a label, a Use tick and a
 * Random Amounts flag. A level keeps a list of them (TileLevelData.savedSeeds). With Use Saved Seeds on,
 * TileSeedCycler hands out the ticked seeds one after another and wraps round, so every load of the level
 * takes the next one. A seed only gives the same board while the level asset (layers, pools, face locks)
 * stays unchanged. TileVisualSpawner asks the cycler for a seed, TileLevelEditor draws and saves them.
 */
using System;
using System.Collections.Generic;

[Serializable]
public class TileSeedEntry
{
    #region Public Variables

    public string label = "";
    // On = the cycle uses this seed.
    public bool use = true;
    // Drives the tile types (and the faces too when Face Seed is the same number).
    public int contentSeed;
    // Drives which faces appear and how many sets each gets.
    public int faceSeed;
    // Drives where the tiles are placed.
    public int placementSeed;
    // On = every set picks any face freely, so the amount per face is random.
    public bool randomFaceAmounts;

    #endregion

    #region Methods

    // A new entry with three random numbers.
    public static TileSeedEntry CreateRandom(string newLabel)
    {
        TileSeedEntry entry = new TileSeedEntry();
        entry.label = newLabel;
        entry.contentSeed = Guid.NewGuid().GetHashCode();
        entry.faceSeed = Guid.NewGuid().GetHashCode();
        entry.placementSeed = Guid.NewGuid().GetHashCode();
        entry.randomFaceAmounts = true;
        return entry;
    }

    // An independent copy, so the spawner never edits the saved entry.
    public TileSeedEntry Copy()
    {
        TileSeedEntry copy = new TileSeedEntry();
        copy.label = label;
        copy.use = use;
        copy.contentSeed = contentSeed;
        copy.faceSeed = faceSeed;
        copy.placementSeed = placementSeed;
        copy.randomFaceAmounts = randomFaceAmounts;
        return copy;
    }

    // True when the other entry builds exactly the same board.
    public bool SameLayout(TileSeedEntry other)
    {
        return other != null && contentSeed == other.contentSeed && faceSeed == other.faceSeed
            && placementSeed == other.placementSeed && randomFaceAmounts == other.randomFaceAmounts;
    }

    #endregion
}

public class TileSeedCycler
{
    #region Private Variables

    private readonly Dictionary<TileLevelData, int> cursors = new Dictionary<TileLevelData, int>();

    #endregion

    #region Methods

    // The next ticked seed of the level, or null when seeds are off or none is ticked.
    public TileSeedEntry Next(TileLevelData level)
    {
        if (level == null || !level.useSavedSeeds)
        {
            return null;
        }

        List<TileSeedEntry> chosen = level.GetSelectedSeeds();
        if (chosen.Count == 0)
        {
            return null;
        }

        int cursor;
        cursors.TryGetValue(level, out cursor);   // stays 0 for a level that has no cursor yet
        TileSeedEntry entry = chosen[cursor % chosen.Count];
        cursors[level] = (cursor + 1) % chosen.Count;
        return entry;
    }

    // Starts every level's cycle from its first seed again.
    public void Reset()
    {
        cursors.Clear();
    }

    #endregion
}