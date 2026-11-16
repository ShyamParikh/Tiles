using UnityEngine;

/// <summary>A material and / or color change for a tile body. No material and no color means "no change".</summary>
public struct TileLookSpec
{
    public Material material;
    public int materialSlot;
    public bool hasColor;
    public Color color;
    public int colorSlot;
}