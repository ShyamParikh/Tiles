using System.Collections.Generic;
using UnityEngine;

// This is just a raw C# class, safely tucked in this file to avoid Native Class errors.
[System.Serializable]
public class MahjongLayer
{
    public int width = 10;
    public int height = 10;
    
    // Offset for staggering layers to create the 3D interlocking effect
    public Vector2 offset = Vector2.zero; 
    
    [HideInInspector]
    public bool[] grid;

    public MahjongLayer(int w, int h)
    {
        width = w;
        height = h;
        grid = new bool[w * h];
    }

    public bool GetCell(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        return grid[y * width + x];
    }

    public void SetCell(int x, int y, bool value)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        grid[y * width + x] = value;
    }
}

// The actual ScriptableObject you will create in your project window.
[CreateAssetMenu(fileName = "NewVisualLevel", menuName = "Mahjong/Visual Level Data")]
public class MahjongLevelData : ScriptableObject
{
    public int defaultWidth = 10;
    public int defaultHeight = 10;
    
    public List<MahjongLayer> layers = new List<MahjongLayer>();

    public void AddLayer()
    {
        layers.Add(new MahjongLayer(defaultWidth, defaultHeight));
    }
}