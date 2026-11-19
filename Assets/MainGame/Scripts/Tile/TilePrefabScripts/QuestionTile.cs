/*
 * QuestionTile
 * ------------
 * A tile that shows a question mark until the player moves it into the tray, then reveals its real face.
 * It uses TileFaceCover to swap the sprite and listens to the entered-tray hook of the base Tile.
 * "Hide Again On Undo" brings the question mark back if Undo sends the tile back to the board; otherwise it
 * stays revealed. "On Revealed" is an event for particles or sounds.
 */
using UnityEngine;
using UnityEngine.Events;

public class QuestionTile : Tile
{
    #region Private Variables

    [SerializeField] private TileFaceCover faceCover;
    [Tooltip("On = the question mark comes back if Undo sends the tile back to the board.")]
    [SerializeField] private bool hideAgainOnUndo;
    [SerializeField] private UnityEvent onRevealed;

    private bool revealed;

    #endregion

    #region Overridable Hooks

    // Starts hidden behind the question mark.
    protected override void OnTileInitialized()
    {
        revealed = false;
        faceCover.Capture();
        faceCover.Cover();
    }

    // The tile moved into the tray: reveal the face.
    protected override void OnEnteredTray()
    {
        if (revealed) return;

        revealed = true;
        faceCover.Reveal();
        onRevealed?.Invoke();
    }

    // Undo sent the tile back: optionally hide the face again.
    protected override void OnReturnedToBoard()
    {
        if (!hideAgainOnUndo || !revealed) return;

        revealed = false;
        faceCover.Cover();
    }

    #endregion
}