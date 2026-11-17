using com.otm.currency;
using com.otm.utility;
using UnityEngine;

// Every currency change in the game goes through here, so saving is never forgotten.
public static class CurrencyOps
{
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

    public static void AddInfinite(Currency currency, int seconds)
    {
        if (currency == null || seconds <= 0) return;

        PlayerPrefsHelper.CanSaveData = true;
        currency.AddInfinite(seconds);
        SaveAndNotify(currency);
    }

    // An infinite timer makes everything free.
    public static bool CanAfford(Currency currency, int amount)
    {
        return currency != null && (amount <= 0 || currency.IsInfiniteOnGoing || currency.Value >= amount);
    }

    public static bool Spend(Currency currency, int amount)
    {
        if (!CanAfford(currency, amount)) return false;

        Add(currency, -amount);
        return true;
    }

    // The explicit notify makes labels refresh even when the change happened outside Play Mode.
    private static void SaveAndNotify(Currency currency)
    {
        PlayerPrefsHelper.SaveData();
        currency.OnCurrencyChange?.Invoke(currency.Value);
    }
}