using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Hides its face behind a cloud until no tile is covering it any more.
/// </summary>
public class CloudyTile : Tile
{
    [Header("Cloudy Tile")]
    [Tooltip("Sprite drawn instead of the face while the tile is covered. Leave empty to hide the image completely.")]
    [SerializeField] private Sprite cloudSprite;
    [Tooltip("Off = once revealed the face stays visible. On = the cloud comes back if the tile gets covered again (Undo / Shuffle).")]
    [SerializeField] private bool hideAgainWhenCoveredAgain = false;
    [Tooltip("Fired once when the cloud lifts. Hook particles / sounds here.")]
    [SerializeField] private UnityEvent onRevealed;

    private bool revealed;
    private bool waitingForFirstState;

    protected override void OnTileInitialized()
    {
        revealed = false;
        waitingForFirstState = true;
        SetFaceCover(cloudSprite);
    }

    protected override void OnBlockedChanged(bool blocked)
    {
        // The very first update is just the board telling us the starting state, so no reveal effect for it.
        bool silent = waitingForFirstState;
        waitingForFirstState = false;

        if (!blocked)
        {
            if (revealed) return;

            revealed = true;
            ClearFaceCover();
            if (!silent) onRevealed?.Invoke();
        }
        else if (hideAgainWhenCoveredAgain && revealed)
        {
            revealed = false;
            SetFaceCover(cloudSprite);
        }
    }
}