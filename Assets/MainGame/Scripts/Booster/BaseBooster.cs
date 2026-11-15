using System;
using com.otm.currency;
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
/// Base class for every booster. The amount the player owns is stored in a Currency asset
/// (e.g. Currency_Booster_Shuffle). Child classes only implement CanExecute and Execute.
/// </summary>
public abstract class BoosterBase : MonoBehaviour
{
    [Header("Booster")]
    [Tooltip("The Currency asset that stores how many of this booster the player owns. Its Default Value is the starting amount. " +
             "If an infinite timer is active on it, the booster is free to use.")]
    [SerializeField] private Currency currency;
    [Tooltip("Shown in the count label while the booster is infinite.")]
    [SerializeField] private string infiniteText = "INF";

    [Header("UI (Optional)")]
    [Tooltip("The button that triggers this booster. The click listener is added automatically.")]
    [SerializeField] private Button button;
    [Tooltip("Called with the new count as text. Hook this to your label's 'text' property (TMP_Text.text works).")]
    [SerializeField] private UnityEvent<string> onCountTextChanged;

    private bool eventsRegistered;

    public abstract BoosterType Type { get; }

    protected BoosterManager Manager { get; private set; }

    public Currency Currency => currency;
    public int Count => currency != null ? currency.Value : 0;
    public bool IsInfinite => currency != null && currency.IsInfiniteOnGoing;
    public bool HasStock => currency != null && (currency.IsInfiniteOnGoing || currency.Value > 0);

    /// <summary>Called by BoosterManager AFTER the currencies were initialised (Currency.Init resets its events).</summary>
    public virtual void Initialize(BoosterManager manager)
    {
        Manager = manager;

        if (currency == null)
        {
            Debug.LogError($"[BOOSTER] '{name}' ({Type}) has no Currency assigned!", this);
        }
        else if (!eventsRegistered)
        {
            currency.OnCurrencyChange.RegisterEvent(OnCurrencyChanged);
            currency.OnInfiniteUpdate.RegisterEvent(OnInfiniteUpdated);
            currency.OnInfiniteOver.RegisterEvent(OnInfiniteOver);
            eventsRegistered = true;
        }

        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
            button.onClick.AddListener(OnButtonClicked);
        }

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(OnButtonClicked);

        if (eventsRegistered && currency != null)
        {
            currency.OnCurrencyChange.DeRegisterEvent(OnCurrencyChanged);
            currency.OnInfiniteUpdate.DeRegisterEvent(OnInfiniteUpdated);
            currency.OnInfiniteOver.DeRegisterEvent(OnInfiniteOver);
        }
        eventsRegistered = false;
    }

    private void OnButtonClicked()
    {
        if (Manager != null) Manager.TryUse(this);
    }

    private void OnCurrencyChanged(int newValue) { RefreshUI(); }
    private void OnInfiniteUpdated(TimeSpan remaining) { RefreshUI(); }
    private void OnInfiniteOver() { RefreshUI(); }

    /// <summary>Gives the player more of this booster (rewards, shop, debug).</summary>
    public void AddCount(int amount)
    {
        if (currency == null) return;
        currency.Add(amount);
    }

    /// <summary>Uses one. Does nothing while an infinite timer is active (Currency ignores spending then).</summary>
    public void Consume()
    {
        if (currency == null) return;
        currency.Add(-1);
    }

    protected void RefreshUI()
    {
        string text = IsInfinite ? infiniteText : Count.ToString();
        onCountTextChanged?.Invoke(text);
    }

    /// <summary>Called by BoosterManager when a new level starts.</summary>
    public virtual void ResetForNewLevel() { }

    /// <summary>True when the booster has something to do right now.</summary>
    public abstract bool CanExecute();

    /// <summary>
    /// Runs the booster. Call onFinished(true) when done (a charge is used) or onFinished(false) if nothing happened.
    /// </summary>
    public abstract void Execute(Action<bool> onFinished);
}