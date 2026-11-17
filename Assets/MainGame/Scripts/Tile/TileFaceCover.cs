using System;
using UnityEngine;
using UnityEngine.UI;

// Swaps a tile's face image for a cover sprite (cloud, question mark) and back.
// Cloudy and question tiles both use it, so the swapping is written once.
[Serializable]
public class TileFaceCover
{
    [Tooltip("The tile's face image (the same one the Tile uses).")]
    [SerializeField] private Image faceImage;
    [Tooltip("Shown instead of the face while hidden. Empty = the image is hidden completely.")]
    [SerializeField] private Sprite coverSprite;

    private Sprite faceSprite;

    // Remember the real face. Call this once, after the tile has its face.
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