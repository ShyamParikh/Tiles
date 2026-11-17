using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class ResultPanel
{
    [Tooltip("The panel object. It stays hidden until the game ends.")]
    public GameObject root;
    [Tooltip("Optional: the panel fades in with this.")]
    public CanvasGroup group;
    [Tooltip("Replays the level.")]
    public Button restartButton;
    [Tooltip("Win panel only: goes to the next level.")]
    public Button nextButton;
    [Tooltip("Called with e.g. 'LEVEL 3' when the panel opens. Hook it to a label's 'text'.")]
    public UnityEvent<string> onLevelText;
    [Tooltip("Win panel only: called with e.g. '+50' (the coin reward). Hook it to a label's 'text'.")]
    public UnityEvent<string> onRewardText;
}

// Shows the win or lose panel when the game ends and wires up their buttons.
public class TileResultScreens : MonoBehaviour
{
    [SerializeField] private TileGameManager gameManager;
    [Tooltip("Optional: lets the win panel show the coin reward.")]
    [SerializeField] private TileEconomyManager economyManager;

    [SerializeField] private ResultPanel winPanel;
    [SerializeField] private ResultPanel losePanel;

    [Tooltip("Wait before the panel appears, so the last merge can finish.")]
    [SerializeField] private float showDelay = 0.5f;
    [SerializeField] private float fadeDuration = 0.25f;
    [Tooltip("{0} = the level that was played.")]
    [SerializeField] private string levelFormat = "LEVEL {0}";
    [Tooltip("{0} = the coins rewarded for the win.")]
    [SerializeField] private string rewardFormat = "+{0}";

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
        Bind(winPanel.restartButton, gameManager.RestartLevel);
        Bind(winPanel.nextButton, gameManager.NextLevel);
        Bind(losePanel.restartButton, gameManager.RestartLevel);

        Hide(winPanel);
        Hide(losePanel);
    }

    private void OnDestroy()
    {
        Unbind(winPanel.restartButton, gameManager.RestartLevel);
        Unbind(winPanel.nextButton, gameManager.NextLevel);
        Unbind(losePanel.restartButton, gameManager.RestartLevel);
    }

    private void OnStateChanged(TileGameState state)
    {
        StopAllCoroutines();
        Hide(winPanel);
        Hide(losePanel);

        if (state == TileGameState.Won) StartCoroutine(Show(winPanel));
        else if (state == TileGameState.Lost) StartCoroutine(Show(losePanel));
    }

    private IEnumerator Show(ResultPanel panel)
    {
        yield return new WaitForSecondsRealtime(showDelay);

        panel.root.SetActive(true);
        panel.onLevelText?.Invoke(string.Format(levelFormat, gameManager.PlayedLevelNumber));
        if (economyManager != null) panel.onRewardText?.Invoke(string.Format(rewardFormat, economyManager.WinReward));

        if (panel.group == null) yield break;
        panel.group.alpha = 0f;
        panel.group.DOFade(1f, fadeDuration).SetUpdate(true).SetLink(panel.root);
    }

    private static void Hide(ResultPanel panel)
    {
        if (panel.root == null) return;

        if (panel.group != null) panel.group.DOKill();
        panel.root.SetActive(false);
    }

    private static void Bind(Button button, UnityAction action)
    {
        if (button != null) button.onClick.AddListener(action);
    }

    private static void Unbind(Button button, UnityAction action)
    {
        if (button != null) button.onClick.RemoveListener(action);
    }
}