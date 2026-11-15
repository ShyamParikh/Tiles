using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Shows a question mark until the player moves it into the tray, then reveals its real face.
/// </summary>
public class QuestionTile : Tile
{
    [Header("Question Tile")]
    [Tooltip("Sprite drawn instead of the face while the tile is on the board. Leave empty to hide the image completely.")]
    [SerializeField] private Sprite questionSprite;
    [Tooltip("Off = if the Undo booster sends it back, the face stays visible. On = it hides again.")]
    [SerializeField] private bool hideAgainWhenReturnedToBoard = false;
    [Tooltip("Fired once when the face is revealed. Hook particles / sounds here.")]
    [SerializeField] private UnityEvent onRevealed;

    private bool revealed;

    protected override void OnTileInitialized()
    {
        revealed = false;
        SetFaceCover(questionSprite);
    }

    protected override void OnEnteredTray()
    {
        if (revealed) return;

        revealed = true;
        ClearFaceCover();
        onRevealed?.Invoke();
    }

    protected override void OnReturnedToBoard()
    {
        if (!hideAgainWhenReturnedToBoard || !revealed) return;

        revealed = false;
        SetFaceCover(questionSprite);
    }
}