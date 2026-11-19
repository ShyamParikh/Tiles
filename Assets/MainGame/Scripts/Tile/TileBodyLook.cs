/*
 * TileSpriteLook
 * --------------
 * Handles the body color and material of a sprite tile, in a plain class that Tile owns. It replaces
 * TileBodyLook: a sprite has one material and one color, so there are no material slots, no color property
 * names and no material copies. SetColor tints the body (and the extra sprite parts), ClearColor goes back
 * to the colors the prefab had, SetBlocked dims the body when the tile is covered, and SetMaterial /
 * ResetMaterial swap the sprite material (for example a flash or a gray-out shader). The level's board and
 * tray looks still go through the same calls, a slot number from the level is simply ignored.
 */
using System.Collections.Generic;
using UnityEngine;

public class TileSpriteLook
{
    #region Private Variables

    private readonly List<SpriteRenderer> parts = new List<SpriteRenderer>();
    private readonly List<Color> originalColors = new List<Color>();
    private readonly SpriteRenderer body;
    private readonly Material originalMaterial;
    private readonly Color dimColor;

    private bool hasColor;
    private bool blocked;
    private bool materialReplaced;
    private Color color;

    #endregion

    #region Setup

    // Reads the renderers once. dimColor is what a covered tile is multiplied by (white = no dimming).
    public TileSpriteLook(SpriteRenderer body, List<SpriteRenderer> extras, Color dimColor)
    {
        this.body = body;
        this.dimColor = dimColor;

        if (body != null)
        {
            originalMaterial = body.sharedMaterial;
            AddPart(body);
        }

        foreach (SpriteRenderer extra in extras)
        {
            if (extra != null && extra != body)
            {
                AddPart(extra);
            }
        }
    }

    // Remembers one renderer and the color it started with.
    private void AddPart(SpriteRenderer renderer)
    {
        parts.Add(renderer);
        originalColors.Add(renderer.color);
    }

    #endregion

    #region Colors

    // Tints the body and the extra parts.
    public void SetColor(Color value)
    {
        hasColor = true;
        color = value;
        Apply();
    }

    // Goes back to the colors the prefab had.
    public void ClearColor()
    {
        if (!hasColor)
        {
            return;
        }

        hasColor = false;
        Apply();
    }

    // Dims the body when the tile is covered, restores it when it is free.
    public void SetBlocked(bool value)
    {
        blocked = value;
        Apply();
    }

    // Pushes the current color (and the dimming of covered tiles) onto the renderers.
    private void Apply()
    {
        bool dim = blocked && dimColor != Color.white;

        for (int i = 0; i < parts.Count; i++)
        {
            Color shown = hasColor ? color : originalColors[i];
            if (dim)
            {
                shown = shown * dimColor;
            }
            parts[i].color = shown;
        }
    }

    #endregion

    #region Material

    // Replaces the body sprite material.
    public void SetMaterial(Material material)
    {
        if (material == null || body == null)
        {
            return;
        }

        body.sharedMaterial = material;
        materialReplaced = true;
    }

    // Puts the prefab's own material back. Does nothing if none was replaced.
    public void ResetMaterial()
    {
        if (!materialReplaced || body == null)
        {
            return;
        }

        materialReplaced = false;
        body.sharedMaterial = originalMaterial;
    }

    #endregion
}