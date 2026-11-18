using com.otm.ui;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// The lose panel: level text and a Restart button.
public class LoseSubView : SubView
{
    [SerializeField] private Button restartButton;
    [SerializeField] private TMP_Text levelText;

    public void Bind(UnityAction onRestart)
    {
        restartButton.onClick.AddListener(onRestart);
    }

    public void SetTexts(string level)
    {
        if (levelText != null) levelText.text = level;
    }
}