/*
 * WinSubView
 * ----------
 * The win panel, a SubView of TileResultView. It holds a Restart button (replay the level), a Next button (go to
 * the next level), a level text and a coin reward text. It contains no game logic: the controller calls Bind
 * once to connect the buttons to the game manager, and SetTexts to fill the labels before the panel opens.
 */
using com.otm.ui;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WinSubView : SubView
{
    #region Private Variables

    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text rewardText;

    #endregion

    #region Methods

    // Connects the buttons.
    public void Bind(UnityAction onRestart, UnityAction onNext)
    {
        restartButton.onClick.AddListener(onRestart);
        nextButton.onClick.AddListener(onNext);
    }

    // Fills the labels (a label that isn't assigned is skipped).
    public void SetTexts(string level, string reward)
    {
        if (levelText != null) levelText.text = level;
        if (rewardText != null) rewardText.text = reward;
    }

    #endregion
}