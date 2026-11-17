using UnityEngine;

// "Change the material to this and / or the color to that." No material and no color means no change.
public struct TileLookSpec
{
    public Material material;
    public int materialSlot;
    public bool hasColor;
    public Color color;
    public int colorSlot;
}