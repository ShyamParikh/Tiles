using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Swaps a tile's face image for a cover sprite (cloud, question mark) and back. Used by CloudyTile and QuestionTile.</summary>
[Serializable]
public class TileFaceCover
{
    [Tooltip("The tile's face Image (the same Image as the Tile's 'Face Image').")]
    [SerializeField] private Image faceImage;
    [Tooltip("Sprite shown instead of the face while hidden. Empty = the image is hidden completely.")]
    [SerializeField] private Sprite coverSprite;

    private Sprite faceSprite;

    /// <summary>Remembers the real face. Call once, after the tile's face has been assigned.</summary>
    public void Capture()
    {
        if (faceImage != null) faceSprite = faceImage.sprite;
    }

    public void Cover() { Show(coverSprite); }
    public void Reveal() { Show(faceSprite); }

    private void Show(Sprite sprite)
    {
        if (faceImage == null) return;

        faceImage.sprite = sprite;
        faceImage.enabled = sprite != null;
    }
}