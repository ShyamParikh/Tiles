/*
 * TrayPose
 * --------
 * Describes how a tile looks once it sits in the tray and how it gets there: its size, an optional rotation
 * inside the slot, and the duration and ease of the grow-and-turn animation that plays while the tile flies in.
 * With Fit To Slot on, the size is worked out from the slot: the tile is scaled so its body fills Fit Fill of the
 * slot's width and height (it never grows past its board size while Only Shrink is on), whatever size the
 * board, the camera zoom or the slot is. Scale is then an extra multiplier on top of the fitted size (leave it
 * at 1,1,1). With Fit To Slot off, Scale alone is the size as a multiple of the board size. TileTrayManager owns
 * one of these and hands it to TileArrangementManager, which asks ScaleFor and plays the animation.
 */
using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class TrayPose
{
    #region Public Variables

    [Tooltip("Size multiplier. With Fit To Slot off: the size in the tray as a multiple of the size on the board. With it on: an extra multiplier on top of the fitted size (leave it at 1,1,1). Different X / Y / Z values squash or stretch.")]
    public Vector3 scale = Vector3.one;

    [Tooltip("On = a tile is scaled to fit its tray slot. The slot needs a Size Rect, an Art sprite or a Manual World Size.")]
    public bool fitToSlot = true;
    [Tooltip("How much of the slot the tile fills when fitting. 0.9 = a 10% margin.")]
    [Range(0.1f, 1.5f)] public float fitFill = 0.9f;
    [Tooltip("On = the tile only gets smaller than on the board, never bigger.")]
    public bool onlyShrink = true;

    [Tooltip("On = tiles turn to the rotation below when they reach the tray. Off = they keep the rotation they had on the board.")]
    public bool setRotation;
    [Tooltip("Rotation of a tile inside its slot, in degrees (the slot's own axes).")]
    public Vector3 rotation = Vector3.zero;

    [Tooltip("How long a tile takes to grow and turn into this pose. It happens while the tile flies in.")]
    public float duration = 0.3f;
    public Ease ease = Ease.OutQuad;

    #endregion

    #region Methods

    // The size multiplier for this tile in this slot, measured against its current size on screen.
    public Vector3 ScaleFor(Tile tile, TraySlot slot)
    {
        if (!fitToSlot || tile == null || slot == null)
        {
            return scale;
        }

        Vector2 slotSize = slot.WorldSize;
        Vector3 tileSize = tile.WorldBounds.size;
        if (slotSize.x <= 0f || slotSize.y <= 0f || tileSize.x <= 0f || tileSize.y <= 0f)
        {
            return scale;
        }

        float fit = Mathf.Min(slotSize.x / tileSize.x, slotSize.y / tileSize.y) * fitFill;
        if (onlyShrink)
        {
            fit = Mathf.Min(fit, 1f);
        }
        return scale * fit;
    }

    #endregion
}