using UnityEngine;

[System.Serializable]
public class TileLayer
{
    public int width = 10;
    public int height = 10;
    public Vector2 offset = Vector2.zero;

    [Tooltip("If off, this layer uses the level's default (Base Layer Height for layer 1, Default Layer Shift for the rest).")]
    public bool overrideShift = false;
    [Tooltip("Layer 1: height of the base layer. Other layers: distance above the previous layer.")]
    public float customShift = 0.3f;

    [HideInInspector] public bool[] grid;

    public TileLayer(int w, int h)
    {
        width = w;
        height = h;
        grid = new bool[w * h];
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