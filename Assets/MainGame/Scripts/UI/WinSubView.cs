using com.otm.ui;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// The win panel: level text, coin reward text, and Restart / Next buttons.
public class WinSubView : SubView
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text rewardText;

    public void Bind(UnityAction onRestart, UnityAction onNext)
    {
        restartButton.onClick.AddListener(onRestart);
        nextButton.onClick.AddListener(onNext);
    }

    public void SetTexts(string level, string reward)
    {
        if (levelText != null) levelText.text = level;
        if (rewardText != null) rewardText.text = reward;
    }
}