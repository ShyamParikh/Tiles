/*
 * BoosterBase
 * -----------
 * The shared part of every booster. How many the player owns is a Currency asset (its Default Value is the
 * starting stock); the count only goes up through the debugger or a coin purchase, and an infinite timer on the
 * currency makes the booster free. With a count of 0 the booster can be used by paying coins instead (Coin Use
 * Cost). Each booster has its own coin price and purchase amount for the shop. It shows its count in a text
 * field (or "INF"), can show an out-of-stock object, and wires its use button and buy button by itself. It
 * listens to the currency through a CurrencyWatcher. A real booster only has to write CanExecute and Execute:
 * the manager decides when it may run and takes the charge once it really worked.
 */
using System;
using com.otm.currency;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum BoosterType { Shuffle, Undo, AutoComplete }

public abstract class BoosterBase : MonoBehaviour
{
    #region Private Variables

    [Header("Booster")]
    [Tooltip("The currency that stores this booster's count. Its Default Value is the starting stock. An infinite timer on it makes the booster free.")]
    [SerializeField] private Currency currency;

    [Header("Count display (all optional)")]
    [SerializeField] private TMP_Text countText;
    [Tooltip("Shown instead of the number while an infinite timer runs.")]
    [SerializeField] private string infiniteText = "INF";
    [Tooltip("Only visible when the player has none left, e.g. a coin cost icon.")]
    [SerializeField] private GameObject outOfStockObject;
    [Tooltip("Also receives the count text, for sounds or animations.")]
    [SerializeField] private UnityEvent<string> onCountTextChanged;

    [Header("Coin use (when the count is 0)")]
    [SerializeField] private bool allowCoinUse = true;
    [Tooltip("Coins one use costs. 0 turns coin use off.")]
    [SerializeField] private int coinUseCost = 50;
    [SerializeField] private TMP_Text coinUseCostText;

    [Header("Shop")]
    [Tooltip("Coins one purchase costs. Every booster has its own price.")]
    [SerializeField] private int coinPrice = 100;
    [Tooltip("How many boosters one purchase gives.")]
    [SerializeField] private int amountPerPurchase = 1;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    [Header("Use button")]
    [SerializeField] private Button button;

    private CurrencyWatcher watcher;

    #endregion

    #region Properties

    protected BoosterManager Manager { get; private set; }

    public Currency Currency => currency;
    public bool AllowCoinUse => allowCoinUse;
    public int CoinUseCost => coinUseCost;
    public int CoinPrice => coinPrice;
    public int AmountPerPurchase => amountPerPurchase;
    public int Count => currency != null ? currency.Value : 0;
    public bool IsInfinite => currency != null && currency.IsInfiniteOnGoing;
    public bool HasStock => IsInfinite || Count > 0;

    #endregion

    #region Unity Callbacks

    // Removes the button listeners and stops watching the currency.
    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(OnUseClicked);
        if (buyButton != null) buyButton.onClick.RemoveListener(OnBuyClicked);
        if (watcher != null) watcher.Dispose();
    }

    #endregion

    #region Setup

    // Called by BoosterManager once the currencies are initialised (Init wipes a currency's listeners).
    public void Initialize(BoosterManager manager)
    {
        Manager = manager;

        if (currency == null)
        {
            Debug.LogError($"[BOOSTER] '{name}' has no Currency assigned!", this);
            return;
        }

        watcher = new CurrencyWatcher(currency, RefreshUI);
        if (button != null) button.onClick.AddListener(OnUseClicked);
        if (buyButton != null) buyButton.onClick.AddListener(OnBuyClicked);
        if (priceText != null) priceText.text = coinPrice.ToString();
        if (coinUseCostText != null) coinUseCostText.text = coinUseCost.ToString();
        RefreshUI();
    }

    #endregion

    #region Buttons

    // The use button was pressed.
    private void OnUseClicked() { Manager.Use(this); }

    // The buy button was pressed.
    private void OnBuyClicked() { Manager.Buy(this); }

    #endregion

    #region Count And Display

    // Gives the player more of this booster (rewards, shop, debug).
    public void AddCount(int amount) { CurrencyOps.Add(currency, amount); }

    // Uses one. Currency ignores spending while an infinite timer is active.
    public void Consume() { CurrencyOps.Add(currency, -1); }

    // Updates the count text, the out-of-stock object and the event.
    private void RefreshUI()
    {
        string text = IsInfinite ? infiniteText : Count.ToString();

        if (countText != null) countText.text = text;
        if (outOfStockObject != null) outOfStockObject.SetActive(!HasStock);
        onCountTextChanged?.Invoke(text);
    }

    #endregion

    #region Abstract Members

    // Which booster this is.
    public abstract BoosterType Type { get; }

    // Called when a new level starts, so a booster can clear its own state.
    public virtual void ResetForNewLevel() { }

    // Is there anything for the booster to do right now?
    public abstract bool CanExecute();

    // Do the work, then call onFinished(true) if a charge should be taken, or onFinished(false) if nothing happened.
    public abstract void Execute(Action<bool> onFinished);

    #endregion
}