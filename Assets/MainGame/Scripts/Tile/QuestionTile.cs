using UnityEngine;
using UnityEngine.Events;

/// <summary>Shows a question mark until the tile moves into the tray, then reveals its face.</summary>
public class QuestionTile : Tile
{
    [Header("Face Cover")]
    [SerializeField] private TileFaceCover faceCover;

    [Header("Question Tile")]
    [Tooltip("On = the question mark returns if Undo sends the tile back to the board.")]
    [SerializeField] private bool hideAgainOnUndo = false;
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