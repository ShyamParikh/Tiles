using System.Collections.Generic;
using UnityEngine;

// ---------------------------------------------------------------------------
// Settings classes. They are [Serializable] so they appear as foldouts inside
// the MahjongBoardSpawner inspector.
// ---------------------------------------------------------------------------

public enum MahjongBuiltInPattern
{
    Pyramid,
    Diamond,
    Cross,
    Frame,
    TwinPyramids,
    Block,
    Staircase
}

// The numbers are fixed on purpose: scenes saved with the old order (Aligned = 0)
// now load as Diagonal, so the half-offset stacking is on by default.
public enum MahjongStackStyle
{
    Diagonal = 0,   // odd layers shift half a tile in X and Y: each tile sits on up to 4 below
    BrickX = 1,     // odd layers shift half a tile in X: each tile sits on up to 2 below
    BrickY = 2,     // odd layers shift half a tile in Y: each tile sits on up to 2 below
    Aligned = 3,    // every tile sits directly on the tile below
    Free = 4        // random generator: upper tiles may sit on ANY half-tile position
}

// The numbers are fixed on purpose: scenes saved with the old "All" (2) now load as Half.
public enum MahjongSupportRule
{
    None = 0,         // no check, tiles may overhang or float
    AtLeastOne = 1,   // any overlap with a tile below (even a quarter of the footprint)
    Half = 2,         // at least half of the footprint rests on tiles below
    All = 3           // the whole footprint rests on tiles below
}

[System.Serializable]
public class MahjongPatternSettings
{
    public MahjongBuiltInPattern pattern = MahjongBuiltInPattern.Pyramid;
    [Min(2)] public int width = 12;    // columns of the bottom layer
    [Min(2)] public int height = 8;    // rows of the bottom layer
    [Min(1)] public int layers = 4;    // how many layers stacked
}

[System.Serializable]
public class MahjongRandomSettings
{
    [Tooltip("0 = a new random board every time. Any other number = the same board every time. The seed used is printed in the Console.")]
    public int seed = 0;

    [Min(2)] public int minColumns = 8;
    [Min(2)] public int maxColumns = 14;
    [Min(2)] public int minRows = 6;
    [Min(2)] public int maxRows = 9;
    [Min(1)] public int minLayers = 2;
    [Min(1)] public int maxLayers = 5;

    [Tooltip("Chance (0-1) that a cell of the bottom layer gets a tile. Lower = more holes.")]
    [Range(0.3f, 1f)] public float baseFillChance = 0.8f;

    [Tooltip("Chance (0-1) that a supported cell on an upper layer gets a tile.")]
    [Range(0.2f, 1f)] public float upperLayerChance = 0.65f;

    [Tooltip("Left/right symmetry")]
    public bool mirrorHorizontally = true;

    [Tooltip("Top/bottom symmetry")]
    public bool mirrorVertically = true;

    [Tooltip("The generator retries until it reaches at least this many tiles (or gives up and uses the best try).")]
    [Min(2)] public int minTiles = 40;
}

[System.Serializable]
public class MahjongStackSettings
{
    [Tooltip("Diagonal = upper tiles sit on the corner of 4 below. BrickX / BrickY = upper tiles sit between 2 below. Aligned = straight up. Free = random generator may use any half-tile position. Level assets in half-step mode ignore it.")]
    public MahjongStackStyle style = MahjongStackStyle.Diagonal;

    [Tooltip("What a tile needs underneath it. None = tiles may float. AtLeastOne = any overlap. Half = at least half of the tile rests on tiles below. All = the whole tile rests on tiles below.")]
    public MahjongSupportRule supportRule = MahjongSupportRule.Half;

    public bool StaggersX
    {
        get { return style == MahjongStackStyle.BrickX || style == MahjongStackStyle.Diagonal; }
    }

    public bool StaggersY
    {
        get { return style == MahjongStackStyle.BrickY || style == MahjongStackStyle.Diagonal; }
    }

    // Layer shift in HALF-tile units (1 = half a tile). Layer 0 and every even layer stay
    // on the base grid, every odd layer is shifted on the staggered axes.
    public Vector2Int GetLayerOffset(int layer)
    {
        if (layer % 2 == 0)
        {
            return Vector2Int.zero;
        }

        int offsetX = 0;
        int offsetY = 0;

        if (StaggersX)
        {
            offsetX = 1;
        }
        if (StaggersY)
        {
            offsetY = 1;
        }
        return new Vector2Int(offsetX, offsetY);
    }
}

// ---------------------------------------------------------------------------
// Pure logic: turns settings into a list of grid positions.
// x, y = position in HALF-tile units (2 = one whole tile), z = layer (0 = bottom).
// A tile at (x, y) covers the 2 x 2 half-cells x..x+1, y..y+1.
// It never touches the scene.
// ---------------------------------------------------------------------------

public static class MahjongLayoutBuilder
{
    // ---------- Built-in patterns ----------

    public static List<Vector3Int> BuildPattern(MahjongPatternSettings settings, MahjongStackSettings stack)
    {
        return ToHalfUnits(BuildPatternGrid(settings), stack);
    }

    // Converts whole-tile cells to half-tile units and applies the layer shift.
    public static List<Vector3Int> ToHalfUnits(List<Vector3Int> wholeCells, MahjongStackSettings stack)
    {
        List<Vector3Int> result = new List<Vector3Int>(wholeCells.Count);

        for (int i = 0; i < wholeCells.Count; i++)
        {
            Vector3Int cell = wholeCells[i];
            Vector2Int offset = stack.GetLayerOffset(cell.z);
            result.Add(new Vector3Int(cell.x * 2 + offset.x, cell.y * 2 + offset.y, cell.z));
        }
        return result;
    }

    // Whole-tile cells: x = column, y = row, z = layer.
    private static List<Vector3Int> BuildPatternGrid(MahjongPatternSettings s)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        int w = s.width;
        int h = s.height;
        float cx = (w - 1) * 0.5f;
        float cy = (h - 1) * 0.5f;

        switch (s.pattern)
        {
            case MahjongBuiltInPattern.Pyramid:
                for (int l = 0; l < s.layers; l++)
                {
                    bool any = false;
                    for (int y = l; y < h - l; y++)
                    {
                        for (int x = l; x < w - l; x++)
                        {
                            result.Add(new Vector3Int(x, y, l));
                            any = true;
                        }
                    }
                    if (!any)
                    {
                        break;
                    }
                }
                break;

            case MahjongBuiltInPattern.Diamond:
                for (int l = 0; l < s.layers; l++)
                {
                    float threshold = 1f - (float)l / s.layers;
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            float d = Mathf.Abs(x - cx) / (w * 0.5f) + Mathf.Abs(y - cy) / (h * 0.5f);
                            if (d <= threshold)
                            {
                                result.Add(new Vector3Int(x, y, l));
                            }
                        }
                    }
                }
                break;

            case MahjongBuiltInPattern.Cross:
                float armHalf = Mathf.Max(1f, Mathf.Min(w, h) / 6f);
                for (int l = 0; l < s.layers; l++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            float dx = Mathf.Abs(x - cx);
                            float dy = Mathf.Abs(y - cy);
                            bool horizontalArm = dy <= armHalf && dx <= w * 0.5f - l;
                            bool verticalArm = dx <= armHalf && dy <= h * 0.5f - l;
                            if (horizontalArm || verticalArm)
                            {
                                result.Add(new Vector3Int(x, y, l));
                            }
                        }
                    }
                }
                break;

            case MahjongBuiltInPattern.Frame:
                // A ring whose thickness equals "layers". Upper layers keep only the outer part of the ring.
                for (int l = 0; l < s.layers; l++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            int edge = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y));
                            if (edge >= l && edge <= s.layers - 1)
                            {
                                result.Add(new Vector3Int(x, y, l));
                            }
                        }
                    }
                }
                break;

            case MahjongBuiltInPattern.TwinPyramids:
                // Two pyramids with a gap in the middle.
                int halfW = (w - 1) / 2;
                for (int l = 0; l < s.layers; l++)
                {
                    bool any = false;
                    for (int y = l; y < h - l; y++)
                    {
                        for (int x = l; x < halfW - l; x++)
                        {
                            result.Add(new Vector3Int(x, y, l));            // left pyramid
                            result.Add(new Vector3Int(w - 1 - x, y, l));    // right pyramid (mirror)
                            any = true;
                        }
                    }
                    if (!any)
                    {
                        break;
                    }
                }
                break;

            case MahjongBuiltInPattern.Block:
                // Full rectangle, every layer the same size.
                // The support check trims it into a tight pyramid.
                for (int l = 0; l < s.layers; l++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            result.Add(new Vector3Int(x, y, l));
                        }
                    }
                }
                break;

            case MahjongBuiltInPattern.Staircase:
                // Each layer loses one more column on the left side.
                for (int l = 0; l < s.layers; l++)
                {
                    bool any = false;
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = l; x < w; x++)
                        {
                            result.Add(new Vector3Int(x, y, l));
                            any = true;
                        }
                    }
                    if (!any)
                    {
                        break;
                    }
                }
                break;
        }

        return result;
    }

    // ---------- Footprint / support ----------

    // "covered" holds (halfCellX, halfCellY, layer) for every accepted tile.
    // A tile at (x, y) covers 4 half-cells: x..x+1, y..y+1.

    public static void AddCoverage(int x, int y, int layer, HashSet<Vector3Int> covered)
    {
        for (int offsetX = 0; offsetX < 2; offsetX++)
        {
            for (int offsetY = 0; offsetY < 2; offsetY++)
            {
                covered.Add(new Vector3Int(x + offsetX, y + offsetY, layer));
            }
        }
    }

    // How many of the 4 footprint cells (0-4) are covered on the given layer.
    private static int CountCoveredCells(int x, int y, int layer, HashSet<Vector3Int> covered)
    {
        int count = 0;
        for (int offsetX = 0; offsetX < 2; offsetX++)
        {
            for (int offsetY = 0; offsetY < 2; offsetY++)
            {
                if (covered.Contains(new Vector3Int(x + offsetX, y + offsetY, layer)))
                {
                    count++;
                }
            }
        }
        return count;
    }

    public static bool HasSupport(int x, int y, int layer, MahjongSupportRule rule, HashSet<Vector3Int> covered)
    {
        if (layer <= 0 || rule == MahjongSupportRule.None)
        {
            return true;
        }

        int supportedCells = CountCoveredCells(x, y, layer - 1, covered);

        if (rule == MahjongSupportRule.All)
        {
            return supportedCells == 4;
        }
        if (rule == MahjongSupportRule.Half)
        {
            return supportedCells >= 2;
        }
        return supportedCells > 0;
    }

    // Free of other tiles on the same layer, and supported from below.
    public static bool CanPlace(int x, int y, int layer, MahjongSupportRule rule, HashSet<Vector3Int> covered)
    {
        if (CountCoveredCells(x, y, layer, covered) > 0)
        {
            return false;
        }
        return HasSupport(x, y, layer, rule, covered);
    }

    private static int CompareByLayerThenPosition(Vector3Int a, Vector3Int b)
    {
        int byLayer = a.z.CompareTo(b.z);
        if (byLayer != 0)
        {
            return byLayer;
        }

        int byY = a.y.CompareTo(b.y);
        if (byY != 0)
        {
            return byY;
        }
        return a.x.CompareTo(b.x);
    }

    // Removes overlapping and unsupported tiles, bottom layer first
    // (a removed tile also removes anything that rested only on it).
    public static List<Vector3Int> RemoveUnsupported(List<Vector3Int> layout, MahjongStackSettings stack)
    {
        List<Vector3Int> sorted = new List<Vector3Int>(layout);
        sorted.Sort(CompareByLayerThenPosition);

        HashSet<Vector3Int> covered = new HashSet<Vector3Int>();
        List<Vector3Int> result = new List<Vector3Int>();

        for (int i = 0; i < sorted.Count; i++)
        {
            Vector3Int p = sorted[i];
            if (CanPlace(p.x, p.y, p.z, stack.supportRule, covered))
            {
                AddCoverage(p.x, p.y, p.z, covered);
                result.Add(p);
            }
        }
        return result;
    }

    // Only removes tiles that overlap another tile on the same layer. Support is NOT checked,
    // so a hand-made level spawns exactly as it was drawn.
    public static List<Vector3Int> RemoveOverlaps(List<Vector3Int> layout)
    {
        List<Vector3Int> sorted = new List<Vector3Int>(layout);
        sorted.Sort(CompareByLayerThenPosition);

        HashSet<Vector3Int> covered = new HashSet<Vector3Int>();
        List<Vector3Int> result = new List<Vector3Int>();

        for (int i = 0; i < sorted.Count; i++)
        {
            Vector3Int p = sorted[i];
            if (CountCoveredCells(p.x, p.y, p.z, covered) == 0)
            {
                AddCoverage(p.x, p.y, p.z, covered);
                result.Add(p);
            }
        }
        return result;
    }

    // ---------- Random generator ----------

    public static List<Vector3Int> BuildRandom(MahjongRandomSettings s, MahjongStackSettings stack, System.Random rng)
    {
        // The random generator always needs some support rule, otherwise upper tiles would float.
        MahjongSupportRule rule = stack.supportRule;
        if (rule == MahjongSupportRule.None)
        {
            rule = MahjongSupportRule.AtLeastOne;
        }

        List<Vector3Int> best = new List<Vector3Int>();

        for (int attempt = 0; attempt < 30; attempt++)
        {
            int cols = rng.Next(s.minColumns, Mathf.Max(s.minColumns, s.maxColumns) + 1);
            int rows = rng.Next(s.minRows, Mathf.Max(s.minRows, s.maxRows) + 1);
            int layerCount = rng.Next(s.minLayers, Mathf.Max(s.minLayers, s.maxLayers) + 1);

            int maxX = (cols - 1) * 2;
            int maxY = (rows - 1) * 2;

            List<Vector3Int> candidate = new List<Vector3Int>();
            HashSet<Vector3Int> covered = new HashSet<Vector3Int>();

            for (int layer = 0; layer < layerCount; layer++)
            {
                Dictionary<Vector2Int, bool> decisions = new Dictionary<Vector2Int, bool>();

                float chance = s.upperLayerChance;
                if (layer == 0)
                {
                    chance = s.baseFillChance;
                }

                Vector2Int offset = stack.GetLayerOffset(layer);

                // Whole-tile steps, or half-tile steps on upper layers in Free style.
                int step = 2;
                if (layer > 0 && stack.style == MahjongStackStyle.Free)
                {
                    step = 1;
                }

                int countBefore = candidate.Count;

                for (int y = offset.y; y <= maxY; y += step)
                {
                    for (int x = offset.x; x <= maxX; x += step)
                    {
                        if (!CanPlace(x, y, layer, rule, covered))
                        {
                            continue;
                        }
                        if (!Decide(s, x, y, maxX, maxY, chance, decisions, rng))
                        {
                            continue;
                        }

                        candidate.Add(new Vector3Int(x, y, layer));
                        AddCoverage(x, y, layer, covered);
                    }
                }

                if (candidate.Count == countBefore)
                {
                    break;
                }
            }

            if (candidate.Count > best.Count)
            {
                best = candidate;
            }
            if (candidate.Count >= s.minTiles)
            {
                return candidate;
            }
        }

        return best;
    }

    // One dice roll per group of mirrored positions (the mirror of x is maxX - x in half units).
    private static bool Decide(MahjongRandomSettings s, int x, int y, int maxX, int maxY,
                               float chance, Dictionary<Vector2Int, bool> decisions, System.Random rng)
    {
        int keyX = x;
        int keyY = y;

        if (s.mirrorHorizontally)
        {
            keyX = Mathf.Min(x, maxX - x);
        }
        if (s.mirrorVertically)
        {
            keyY = Mathf.Min(y, maxY - y);
        }

        Vector2Int key = new Vector2Int(keyX, keyY);

        bool value;
        if (!decisions.TryGetValue(key, out value))
        {
            value = rng.NextDouble() < chance;
            decisions[key] = value;
        }
        return value;
    }
}