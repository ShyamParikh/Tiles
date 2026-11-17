using UnityEngine;

// One painted grid of tiles. A level is a stack of these.
[System.Serializable]
public class TileLayer
{
    public int width = 10;
    public int height = 10;

    [Tooltip("Shifts the whole layer, in cells. Half a cell on upper layers gives the interlocking look.")]
    public Vector2 offset;
    [Tooltip("Off = this layer uses the level's default height.")]
    public bool overrideShift;
    [Tooltip("Layer 1: height of the base. Other layers: distance above the layer below.")]
    public float customShift = 0.3f;

    [HideInInspector] public bool[] grid;

    public TileLayer(int width, int height)
    {
        this.width = width;
        this.height = height;
        grid = new bool[width * height];
    }

    public bool GetCell(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height && grid[y * width + x];
    }

    public void SetCell(int x, int y, bool value)
    {
        if (x >= 0 && x < width && y >= 0 && y < height) grid[y * width + x] = value;
    }

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
}