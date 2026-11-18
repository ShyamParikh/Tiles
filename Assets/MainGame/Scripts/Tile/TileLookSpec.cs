/*
 * TileLookSpec
 * ------------
 * A tiny message that says "change the body material to this and / or the color to that". A level builds
 * two of them for every tile, one for the board and one for the tray, and hands them to the tile.
 * No material and no color means "leave it alone". A slot of -1 means every material slot.
 */
using UnityEngine;

public struct TileLookSpec
{
    #region Public Variables

    public Material material;
    public int materialSlot;
    public bool hasColor;
    public Color color;
    public int colorSlot;

    #endregion
}