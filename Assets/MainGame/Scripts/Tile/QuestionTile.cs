using UnityEngine;
using UnityEngine.Events;

// Shows a question mark until the tile moves into the tray, then reveals its face.
public class QuestionTile : Tile
{
    [SerializeField] private TileFaceCover faceCover;
    [Tooltip("On = the question mark comes back if Undo sends the tile back to the board.")]
    [SerializeField] private bool hideAgainOnUndo;
    [SerializeField] private UnityEvent onRevealed;

    private bool revealed;

    protected override void OnTileInitialized()
    {
        revealed = false;
        faceCover.Capture();
        faceCover.Cover();
    }

    protected override void OnEnteredTray()
    {
        if (revealed) return;

        revealed = true;
        faceCover.Reveal();
        onRevealed?.Invoke();
    }

    protected override void OnReturnedToBoard()
    {
        if (!hideAgainOnUndo || !revealed) return;

        revealed = false;
        faceCover.Cover();
    }
}