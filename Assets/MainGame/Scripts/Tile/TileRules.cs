using System.Collections.Generic;
using UnityEngine;

// Small helpers that several scripts share.
public static class TileRules
{
    // How many identical tiles make a match.
    public const int SetSize = 3;

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

    // Sorts the tiles by face and returns every group of equal faces as (start index, length).
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
}