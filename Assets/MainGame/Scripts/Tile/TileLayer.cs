/*
 * TileLayer
 * ---------
 * One painted grid of tiles. A level is a stack of these. It stores which cells hold a tile (grid), the
 * layer's size, an offset in cells (half a cell on upper layers gives the interlocking look), and an optional
 * own height (overrideShift + customShift). TileLevelEditor paints the grid, TileVisualSpawner reads it,
 * and TileLevelData works out the layer's height from the shifts.
 */
using UnityEngine;

[System.Serializable]
public class TileLayer
{
    #region Public Variables

    public int width = 10;
    public int height = 10;

    [Tooltip("Shifts the whole layer, in cells. Half a cell on upper layers gives the interlocking look.")]
    public Vector2 offset;
    [Tooltip("Off = this layer uses the level's default height.")]
    public bool overrideShift;
    [Tooltip("Layer 1: height of the base. Other layers: distance above the layer below.")]
    public float customShift = 0.3f;

    [HideInInspector] public bool[] grid;

    #endregion

    #region Setup

    // Creates an empty layer of the given size.
    public TileLayer(int width, int height)
    {
        this.width = width;
        this.height = height;
        grid = new bool[width * height];
    }

    #endregion

    #region Cells

    // Is there a tile at this cell? Cells outside the grid are always empty.
    public bool GetCell(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height && grid[y * width + x];
    }

    // Puts a tile on (or takes it off) this cell.
    public void SetCell(int x, int y, bool value)
    {
        if (x >= 0 && x < width && y >= 0 && y < height) grid[y * width + x] = value;
    }

    // How many cells of this layer hold a tile.
    public int CountCells()
    {
        int count = 0;
        if (grid == null) return count;

        foreach (bool cell in grid)
        {
            if (cell) count++;
        }
        return count;
    }

    #endregion
}