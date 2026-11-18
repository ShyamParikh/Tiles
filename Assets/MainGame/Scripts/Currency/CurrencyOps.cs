/*
 * CurrencyOps
 * -----------
 * Every currency change in the game goes through this class, so saving is never forgotten. It wraps your
 * Currency asset: add, set, reset, infinite timers, can-afford and spend. A currency never drops below zero,
 * and while an infinite timer runs, spending is free. Every change is saved straight away, which is why a
 * reduced booster count stays reduced. Boosters, the economy manager, offers and the debugger all use it.
 */
using com.otm.currency;
using com.otm.utility;
using UnityEngine;

public static class CurrencyOps
{
    #region Changing Amounts

    // Adds an amount (negative takes away) and saves. A currency never drops below zero.
    public static void Add(Currency currency, int amount)
    {
        if (currency == null) return;

        amount = Mathf.Max(amount, -currency.Value);
        if (amount == 0) return;

        PlayerPrefsHelper.CanSaveData = true;
        currency.Add(amount);
        PlayerPrefsHelper.SaveData();
    }

    // Sets an exact amount (not below zero) and saves.
    public static void Set(Currency currency, int value)
    {
        if (currency == null) return;

        PlayerPrefsHelper.CanSaveData = true;
        currency.SetValue(Mathf.Max(0, value));
        SaveAndNotify(currency);
    }

    // Back to the asset's Default Value. Any infinite timer is cleared too.
    public static void Reset(Currency currency)
    {
        if (currency == null) return;

        PlayerPrefsHelper.CanSaveData = true;
        currency.Delete();
        SaveAndNotify(currency);
    }

    // Starts (or extends) an infinite timer on the currency.
    public static void AddInfinite(Currency currency, int seconds)
    {
        if (currency == null || seconds <= 0) return;

        PlayerPrefsHelper.CanSaveData = true;
        currency.AddInfinite(seconds);
        SaveAndNotify(currency);
    }

    #endregion

    #region Affording And Spending

    // True if the player has enough. An infinite timer makes everything free.
    public static bool CanAfford(Currency currency, int amount)
    {
        return currency != null && (amount <= 0 || currency.IsInfiniteOnGoing || currency.Value >= amount);
    }

    // Takes the amount away if the player can afford it. Returns whether it worked.
    public static bool Spend(Currency currency, int amount)
    {
        if (!CanAfford(currency, amount)) return false;

        Add(currency, -amount);
        return true;
    }

    #endregion

    #region Private Helpers

    // Saves, and tells listeners so labels refresh even when the change happened outside Play Mode.
    private static void SaveAndNotify(Currency currency)
    {
        PlayerPrefsHelper.SaveData();
        currency.OnCurrencyChange?.Invoke(currency.Value);
    }

    #endregion
}