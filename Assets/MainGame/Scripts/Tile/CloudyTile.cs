using UnityEngine;
using UnityEngine.Events;

// Hides its face behind a cloud until nothing is covering it any more.
public class CloudyTile : Tile
{
    [SerializeField] private TileFaceCover faceCover;
    [Tooltip("Off = once revealed it stays revealed. On = the cloud comes back if the tile gets covered again (Undo / Shuffle).")]
    [SerializeField] private bool coverAgain;
    [SerializeField] private UnityEvent onRevealed;

    private bool revealed;
    private bool firstUpdate;

    protected override void OnTileInitialized()
    {
        revealed = false;
        firstUpdate = true;
        faceCover.Capture();
        faceCover.Cover();
    }

    protected override void OnBlockedChanged(bool blocked)
    {
        // The very first update only tells us how the board started, so no reveal effect for it.
        bool silent = firstUpdate;
        firstUpdate = false;

        if (blocked)
        {
            if (!coverAgain || !revealed) return;
            revealed = false;
            faceCover.Cover();
        }
        else if (!revealed)
        {
            revealed = true;
            faceCover.Reveal();
            if (!silent) onRevealed?.Invoke();
        }
    }
}