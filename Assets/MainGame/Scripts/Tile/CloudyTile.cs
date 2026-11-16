using UnityEngine;
using UnityEngine.Events;

/// <summary>Hides its face behind a cloud until no tile covers it any more.</summary>
public class CloudyTile : Tile
{
    [Header("Face Cover")]
    [SerializeField] private TileFaceCover faceCover;

    [Header("Cloudy Tile")]
    [Tooltip("Off = once revealed it stays revealed. On = the cloud returns if the tile is covered again (Undo / Shuffle).")]
    [SerializeField] private bool coverAgain = false;
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
        bool silent = firstUpdate;   // the first update only reports the starting state
        firstUpdate = false;

        if (blocked)
        {
            if (!coverAgain || !revealed) return;
            revealed = false;
            faceCover.Cover();
            return;
        }

        if (revealed) return;
        revealed = true;
        faceCover.Reveal();
        if (!silent) onRevealed?.Invoke();
    }
}