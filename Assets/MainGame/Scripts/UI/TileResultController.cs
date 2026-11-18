using System.Collections;
using UnityEngine;

// Listens to the game state and drives the result view.
// Put it on an object that is always active, because the view itself starts hidden.
public class TileResultController : MonoBehaviour
{
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

    private void OnEnable()
    {
        TileGameManager.stateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        TileGameManager.stateChanged -= OnStateChanged;
    }

    private void Start()
    {
        if (!initializeView) return;

        // A view has to start hidden, otherwise ShowView does nothing.
        resultView.gameObject.SetActive(false);
        resultView.Init();
    }

    private void OnStateChanged(TileGameState state)
    {
        StopAllCoroutines();

        if (state == TileGameState.Won || state == TileGameState.Lost) StartCoroutine(Show(state == TileGameState.Won));
        else resultView.HideView();   // does nothing when the view is not open
    }

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

    // Done on the first show, so it doesn't matter when the view was initialised.
    private void BindButtons(WinSubView win, LoseSubView lose)
    {
        if (bound) return;
        bound = true;

        win.Bind(gameManager.RestartLevel, gameManager.NextLevel);
        lose.Bind(gameManager.RestartLevel);
    }
}