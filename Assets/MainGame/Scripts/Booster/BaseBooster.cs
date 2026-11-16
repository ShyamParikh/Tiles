using System;
using com.otm.currency;
using com.otm.utility;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum BoosterType
{
    Shuffle,
    Undo,
    AutoComplete
}

/// <summary>
/// Base booster. How many the player owns is stored in a Currency asset. It refills only from the editor debugger
/// or by buying with coins. With a count of 0 it can be used by paying coins instead.
/// </summary>
public abstract class BoosterBase : MonoBehaviour
{
    [Header("Booster")]
    [Tooltip("Currency asset that stores this booster's count (Default Value = starting amount). An infinite timer on it makes the booster free.")]
    [SerializeField] private Currency currency;

    [Header("Count Display (Optional)")]
    [SerializeField] private TMP_Text countText;
    [Tooltip("Shown instead of the number while an infinite timer is running.")]
    [SerializeField] private string infiniteText = "INF";
    [Tooltip("Shown only when the player has none left, e.g. the coin cost icon.")]
    [SerializeField] private GameObject outOfStockObject;
    [Tooltip("Also called with the count text, for sounds or animations.")]
    [SerializeField] private UnityEvent<string> onCountTextChanged;

    [Header("Coin Use (when the booster count is 0)")]
    [Tooltip("On = with no boosters left, pressing the booster pays coins instead.")]
    [SerializeField] private bool allowCoinUse = true;
    [Tooltip("Coins one use costs when the booster count is 0. 0 disables coin use.")]
    [SerializeField] private int coinUseCost = 50;
    [Tooltip("Shows the coin cost of one use.")]
    [SerializeField] private TMP_Text coinUseCostText;

    [Header("Shop (Optional)")]
    [Tooltip("Coins one purchase costs. Every booster has its own price.")]
    [SerializeField] private int coinPrice = 100;
    [Tooltip("How many boosters one purchase gives.")]
    [SerializeField] private int amountPerPurchase = 1;
    [Tooltip("Shows the coin price of a purchase.")]
    [SerializeField] private TMP_Text priceText;
    [Tooltip("Buys this booster with coins. The click listener is added automatically.")]
    [SerializeField] private Button buyButton;

    [Header("UI (Optional)")]
    [Tooltip("Uses the booster. The click listener is added automatically.")]
    [SerializeField] private Button button;

    private bool registered;

    public abstract BoosterType Type { get; }
    protected BoosterManager Manager { get; private set; }

    public Currency Currency => currency;
    public bool AllowCoinUse => allowCoinUse;
    public int CoinUseCost => coinUseCost;
    public int CoinPrice => coinPrice;
    public int AmountPerPurchase => amountPerPurchase;
    public int Count => currency != null ? currency.Value : 0;
    public bool IsInfinite => currency != null && currency.IsInfiniteOnGoing;
    public bool HasStock => currency != null && (currency.IsInfiniteOnGoing || currency.Value > 0);

    /// <summary>Called by BoosterManager after the currencies were initialised (Currency.Init resets its events).</summary>
    public virtual void Initialize(BoosterManager manager)
    {
        Manager = manager;

        if (currency == null)
        {
            Debug.LogError($"[BOOSTER] '{name}' has no Currency assigned!", this);
        }
        else if (!registered)
        {
            currency.OnCurrencyChange.RegisterEvent(OnCountChanged);
            currency.OnInfiniteUpdate.RegisterEvent(OnInfiniteUpdated);
            currency.OnInfiniteOver.RegisterEvent(RefreshUI);
            registered = true;
        }

        Bind(button, OnUseClicked);
        Bind(buyButton, OnBuyClicked);
        if (priceText != null) priceText.text = coinPrice.ToString();
        if (coinUseCostText != null) coinUseCostText.text = coinUseCost.ToString();
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(OnUseClicked);
        if (buyButton != null) buyButton.onClick.RemoveListener(OnBuyClicked);
        if (!registered || currency == null) return;

        currency.OnCurrencyChange.DeRegisterEvent(OnCountChanged);
        currency.OnInfiniteUpdate.DeRegisterEvent(OnInfiniteUpdated);
        currency.OnInfiniteOver.DeRegisterEvent(RefreshUI);
        registered = false;
    }

    private static void Bind(Button target, UnityAction action)
    {
        if (target == null) return;
        target.onClick.RemoveListener(action);
        target.onClick.AddListener(action);
    }

    private void OnUseClicked() { Manager.Use(this); }
    private void OnBuyClicked() { Manager.Buy(this); }
    private void OnCountChanged(int value) { RefreshUI(); }
    private void OnInfiniteUpdated(TimeSpan remaining) { RefreshUI(); }

    public void AddCount(int amount)
    {
        if (currency == null) return;

        currency.Add(amount);
        PlayerPrefsHelper.SaveData();
    }

    /// <summary>Uses one. Currency ignores spending while an infinite timer is active.</summary>
    public void Consume()
    {
        if (currency == null) return;

        currency.Add(-1);
        PlayerPrefsHelper.SaveData();
    }

    private void RefreshUI()
    {
        string text = IsInfinite ? infiniteText : Count.ToString();

        if (countText != null) countText.text = text;
        if (outOfStockObject != null) outOfStockObject.SetActive(!HasStock);
        onCountTextChanged?.Invoke(text);
    }

    public virtual void ResetForNewLevel() { }

    public abstract bool CanExecute();

    /// <summary>Runs the booster, then calls onFinished(true) when a charge was used or onFinished(false) if nothing happened.</summary>
    public abstract void Execute(Action<bool> onFinished);
}