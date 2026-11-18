/*
 * LoseSubView
 * -----------
 * The lose panel, a SubView of TileResultView. It holds a Restart button (try the level again) and a level
 * text. It contains no game logic: the controller calls Bind once to connect the button to the game manager,
 * and SetTexts to fill the label before the panel opens.
 */
using com.otm.ui;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoseSubView : SubView
{
    #region Private Variables

    [SerializeField] private Button restartButton;
    [SerializeField] private TMP_Text levelText;

    #endregion

    #region Methods

    // Connects the button.
    public void Bind(UnityAction onRestart)
    {
        restartButton.onClick.AddListener(onRestart);
    }

    // Fills the label (skipped if it isn't assigned).
    public void SetTexts(string level)
    {
        if (levelText != null) levelText.text = level;
    }

    #endregion
}