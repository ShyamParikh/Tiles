/*
 * TileRules
 * ---------
 * A small toolbox of helpers that several scripts share, so none of them repeats the same code.
 *   SetSize  - how many identical tiles make a match (3). Spawning, the tray, the editor warnings and the
 *              boosters all read this one number. The merge animation is still written for exactly three.
 *   Shuffle  - a shuffle that takes a seeded random, so the same level seed gives the same result.
 *   FaceRuns - sorts tiles by face and reports each group of equal faces. Auto Complete and the merge
 *              burst use it to find complete sets.
 */
using System.Collections.Generic;
using UnityEngine;

public static class TileRules
{
    #region Public Variables

    // How many identical tiles make a match.
    public const int SetSize = 3;

    #endregion

    #region Methods

    // Shuffles a list or an array in place with the given random.
    public static void Shuffle<T>(IList<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    // Sorts tiles by face and returns every group of equal faces as (start index, length).
    public static List<Vector2Int> FaceRuns(List<Tile> tiles)
    {
        tiles.Sort((a, b) => a.FaceId.CompareTo(b.FaceId));

        List<Vector2Int> runs = new List<Vector2Int>();
        for (int start = 0; start < tiles.Count;)
        {
            int end = start + 1;
            while (end < tiles.Count && tiles[end].FaceId == tiles[start].FaceId) end++;
            runs.Add(new Vector2Int(start, end - start));
            start = end;
        }
        return runs;
    }

    #endregion
}