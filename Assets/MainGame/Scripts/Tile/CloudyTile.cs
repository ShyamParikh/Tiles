/*
 * CloudyTile
 * ----------
 * A tile that hides its face behind a cloud until nothing is covering it any more. It uses TileFaceCover to
 * swap the face for the cloud sprite. The first covered-status update at spawn only reports how the board
 * started, so it never fires the reveal effect. "Cover Again" brings the cloud back if the tile gets covered
 * again (after Undo or Shuffle); otherwise a revealed face stays revealed. "On Revealed" is an event for
 * particles or sounds. All of the cloud logic lives in this file, the base Tile only calls the hook.
 */
using UnityEngine;
using UnityEngine.Events;

public class CloudyTile : Tile
{
    #region Private Variables

    [SerializeField] private TileFaceCover faceCover;
    [Tooltip("Off = once revealed it stays revealed. On = the cloud comes back if the tile gets covered again (Undo / Shuffle).")]
    [SerializeField] private bool coverAgain;
    [SerializeField] private UnityEvent onRevealed;

    private bool revealed;
    private bool firstUpdate;

    #endregion

    #region Overridable Hooks

    // Starts hidden behind the cloud.
    protected override void OnTileInitialized()
    {
        revealed = false;
        firstUpdate = true;
        faceCover.Capture();
        faceCover.Cover();
    }

    // Reveals the face when the tile becomes free, and (optionally) hides it again when covered.
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

    #endregion
}