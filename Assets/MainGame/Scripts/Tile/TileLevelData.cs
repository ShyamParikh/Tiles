using System.Collections.Generic;
using UnityEngine;

// The actual ScriptableObject you will create in your project window.
// This file MUST be named TileLevelData.cs (same as the class).
[CreateAssetMenu(fileName = "NewVisualLevel", menuName = "Tile/Visual Level Data")]
public class TileLevelData : ScriptableObject
{
    public int defaultWidth = 10;
    public int defaultHeight = 10;

    [Tooltip("Height of the first (base) layer, unless that layer overrides it.")]
    public float baseLayerHeight = 0f;

    [Tooltip("Distance between a layer and the one below it, unless that layer overrides it.")]
    public float defaultLayerShift = 0.3f;

    public List<TileLayer> layers = new List<TileLayer>();

    public void AddLayer()
    {
        TileLayer layer = new TileLayer(defaultWidth, defaultHeight);
        layer.customShift = layers.Count == 0 ? baseLayerHeight : defaultLayerShift;
        layers.Add(layer);
    }

    // Layer 1 (index 0): its height from the origin.
    // Other layers: their shift relative to the previous layer.
    public float GetLayerShift(int index)
    {
        if (index < 0 || index >= layers.Count) return 0f;
        TileLayer layer = layers[index];
        if (layer.overrideShift) return layer.customShift;
        return index == 0 ? baseLayerHeight : defaultLayerShift;
    }

    // Total height of a layer (base height plus the sum of all shifts up to it).
    public float GetLayerStackPosition(int index)
    {
        float total = 0f;
        for (int i = 0; i <= index && i < layers.Count; i++)
            total += GetLayerShift(i);
        return total;
    }
}