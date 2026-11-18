using System;
using UnityEngine;

// Swaps a tile's face sprite for a cover sprite (cloud, question mark) and back.
// Cloudy and question tiles both use it, so the swapping is written once.
[Serializable]
public class TileFaceCover
{
    [Tooltip("The tile's face SpriteRenderer (the same one the Tile uses).")]
    [SerializeField] private SpriteRenderer faceRenderer;
    [Tooltip("Shown instead of the face while hidden. Empty = the face is hidden completely.")]
    [SerializeField] private Sprite coverSprite;

    private Sprite faceSprite;

    // Remember the real face. Call this once, after the tile has its face.
    public void Capture()
    {
        if (faceRenderer != null) faceSprite = faceRenderer.sprite;
    }

    public void Cover() { Show(coverSprite); }
    public void Reveal() { Show(faceSprite); }

    private void Show(Sprite sprite)
    {
        if (faceRenderer == null) return;

        faceRenderer.sprite = sprite;
        faceRenderer.enabled = sprite != null;
    }
}