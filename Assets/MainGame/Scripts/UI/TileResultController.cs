/*
 * TileResultController
 * --------------------
 * Listens to the game state and drives the result view. When the state becomes Won or Lost it waits a short
 * delay (so the last merge can finish), fills in the level text and the coin reward text, and tells the view to
 * show the win or lose panel; for every other state it hides the view. It also connects the Restart and Next
 * buttons to the game manager the first time the panels are shown, so it doesn't matter when the view was
 * initialised. Put it on an object that is always active, because the view itself starts hidden. Leave
 * "Initialize View" on unless your own UI manager already calls Init on your views. The show routine is a
 * coroutine, kept at the bottom of the file.
 */
using System.Collections;
using UnityEngine;

public class TileResultController : MonoBehaviour
{
    #region Private Variables

    [SerializeField] private TileGameManager gameManager;
    [Tooltip("Optional: lets the win panel show the coin reward.")]
    [SerializeField] private TileEconomyManager economyManager;
    [SerializeField] private TileResultView resultView;

    [Tooltip("On = this hides the view and calls Init on it. Turn it off if your UI manager already initialises your views.")]
    [SerializeField] private bool initializeView = true;
    [Tooltip("Wait before the panel appears, so the last merge can finish.")]
    [SerializeField] private float showDelay = 0.5f;
    [Tooltip("{0} = the level that was played.")]
    [SerializeField] private string levelFormat = "LEVEL {0}";
    [Tooltip("{0} = the coins rewarded for the win.")]
    [SerializeField] private string rewardFormat = "+{0}";

    private bool bound;

    #endregion

    #region Unity Callbacks

    // Starts listening for state changes.
    private void OnEnable()
    {
        TileGameManager.stateChanged += OnStateChanged;
    }

    // Stops listening.
    private void OnDisable()
    {
        TileGameManager.stateChanged -= OnStateChanged;
    }

    // Hides and initialises the view (a view has to start hidden, otherwise ShowView does nothing).
    private void Start()
    {
        if (!initializeView) return;

        resultView.gameObject.SetActive(false);
        resultView.Init();
    }

    #endregion

    #region State Handling

    // Won or Lost shows a panel after a delay; any other state hides the view.
    private void OnStateChanged(TileGameState state)
    {
        StopAllCoroutines();

        if (state == TileGameState.Won || state == TileGameState.Lost) StartCoroutine(Show(state == TileGameState.Won));
        else resultView.HideView();   // does nothing when the view is not open
    }

    // Connects the buttons once, on the first show.
    private void BindButtons(WinSubView win, LoseSubView lose)
    {
        if (bound) return;
        bound = true;

        win.Bind(gameManager.RestartLevel, gameManager.NextLevel);
        lose.Bind(gameManager.RestartLevel);
    }

    #endregion

    #region Coroutines

    // Waits, fills in the texts and shows the win or lose panel.
    private IEnumerator Show(bool won)
    {
        yield return new WaitForSecondsRealtime(showDelay);

        WinSubView win = resultView.GetSubView<WinSubView>();
        LoseSubView lose = resultView.GetSubView<LoseSubView>();
        if (win == null || lose == null)
        {
            Debug.LogError("[RESULT] The view has no Win / Lose SubView. Tick 'Has Subview' on it and add both to Sub Views.", this);
            yield break;
        }

        BindButtons(win, lose);

        string level = string.Format(levelFormat, gameManager.PlayedLevelNumber);
        if (won) win.SetTexts(level, economyManager != null ? string.Format(rewardFormat, economyManager.WinReward) : "");
        else lose.SetTexts(level);

        resultView.ShowResult(won);
    }

    #endregion
}