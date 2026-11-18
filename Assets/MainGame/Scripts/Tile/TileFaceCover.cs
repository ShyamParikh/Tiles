/*
 * TileFaceCover
 * -------------
 * Swaps a tile's face sprite for a cover sprite (a cloud, a question mark) and back. Cloudy and Question tiles
 * both own one, so the swapping code is written once. Assign the tile's face SpriteRenderer (the same one the
 * Tile uses) and the cover sprite on the prefab. Call Capture once after the tile has its face, then Cover to
 * hide the face and Reveal to show it. An empty cover sprite hides the face completely.
 */
using System;
using UnityEngine;

[Serializable]
public class TileFaceCover
{
    #region Private Variables

    [Tooltip("The tile's face SpriteRenderer (the same one the Tile uses).")]
    [SerializeField] private SpriteRenderer faceRenderer;
    [Tooltip("Shown instead of the face while hidden. Empty = the face is hidden completely.")]
    [SerializeField] private Sprite coverSprite;

    private Sprite faceSprite;

    #endregion

    #region Methods

    // Remembers the real face. Call this once, after the tile has its face.
    public void Capture()
    {
        if (faceRenderer != null) faceSprite = faceRenderer.sprite;
    }

    // Shows the cover instead of the face.
    public void Cover() { Show(coverSprite); }

    // Shows the real face again.
    public void Reveal() { Show(faceSprite); }

    // Puts a sprite on the face renderer (or hides it when the sprite is empty).
    private void Show(Sprite sprite)
    {
        if (faceRenderer == null) return;

        faceRenderer.sprite = sprite;
        faceRenderer.enabled = sprite != null;
    }

    #endregion
}