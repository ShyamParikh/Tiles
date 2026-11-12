/*using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalCardUI : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject checkMark;

    public void Configure(int amount, Sprite Icon)
    {
        amountText.text = amount.ToString();
        iconImage.sprite = Icon;
    }

    public void SetAmount(int amount)
    {
        amountText.text = amount.ToString();
        CardAnimation();
    }

    private void CardAnimation()
    {
        LeanTween.cancel(gameObject);
        transform.localScale = Vector3.one;

        LeanTween.scale(gameObject, Vector3.one * 1.2f, 0.5f).setEasePunch().setOnComplete(() =>
        {
            LeanTween.scale(gameObject, Vector3.one, 0.5f).setEasePunch();
        });
    }

    public void Complete()
    {
        checkMark.SetActive(true);
        amountText.gameObject.SetActive(false);
    }
}*/