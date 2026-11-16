using System;
using System.Collections.Generic;
using com.otm.currency;
using com.otm.utility;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Debugger for coins, lives and booster counts. The buttons work in and outside Play Mode and every change is saved.
/// Boosters only refill from here or by buying with coins.
/// </summary>
public class TileEconomyDebugger : MonoBehaviour
{
    [Header("Economy Assets")]
    [Tooltip("Provides the coin currency.")]
    [SerializeField] private TileEconomyManager economyManager;
    [Tooltip("Provides each booster's own Currency asset.")]
    [SerializeField] private BoosterManager boosterManager;
    [Tooltip("Optional: the lives currency.")]
    [SerializeField] private RefillCurrency lifeCurrency;

    [Header("Refill")]
    [Tooltip("'Refill All Boosters' tops every booster up to this amount.")]
    [SerializeField] private int refillBoostersTo = 5;

    private Currency Coins => economyManager != null ? economyManager.CoinCurrency : null;

    private void Awake()
    {
        PlayerPrefsHelper.CanSaveData = true;
    }

    // ---------------- Diagnostics ----------------

    [Title("Hard Drive Diagnostics")]
    [Button(ButtonSizes.Large), GUIColor(1f, 1f, 0f)]
    public void PrintRawHardDriveData()
    {
        PlayerPrefsHelper.CanSaveData = true;
        Debug.Log("<color=#FFFF00>--- RAW HARD DRIVE SCAN ---</color>");

        LogRaw(Coins, "Coins");
        LogRaw(lifeCurrency, "Lives");
        foreach (BoosterType type in Enum.GetValues(typeof(BoosterType))) LogRaw(BoosterCurrency(type), type.ToString());

        Debug.Log("<color=#FFFF00>---------------------------</color>");
    }

    private static void LogRaw(Currency currency, string label)
    {
        if (currency == null) return;

        string key = currency.InfiniteEndDateKey.Replace("InfiniteEndDate", "");
        Debug.Log($"Raw {label} saved in OS: {PlayerPrefsHelper.GetInt(key, -999)}");
    }

    // ---------------- Coins ----------------

    [Title("Coin Controls")]
    [Button(ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
    public void AddCoins(int amount = 500)
    {
        Change(Coins, amount);
    }

    [Button(ButtonSizes.Medium), GUIColor(1f, 0.4f, 0.4f)]
    public void RemoveCoins(int amount = 100)
    {
        Change(Coins, -amount);
    }

    // ---------------- Lives ----------------

    [Title("Life Controls")]
    [Button(ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
    public void AddLife()
    {
        Change(lifeCurrency, 1);
    }

    [Button(ButtonSizes.Medium), GUIColor(1f, 0.4f, 0.4f)]
    public void RemoveLife()
    {
        Change(lifeCurrency, -1);
    }

    // ---------------- Boosters ----------------

    [Title("Booster Controls")]
    [Button(ButtonSizes.Medium), GUIColor(0.2f, 1f, 0.4f)]
    public void AddBooster(BoosterType booster, int amount = 1)
    {
        Change(BoosterCurrency(booster), amount);
        Debug.Log($"<color=#32CD32>[ECONOMY] Added {amount} to {booster}!</color>");
    }

    [Button(ButtonSizes.Medium), GUIColor(1f, 0.4f, 0.4f)]
    public void RemoveBooster(BoosterType booster, int amount = 1)
    {
        Change(BoosterCurrency(booster), -amount);
        Debug.Log($"<color=#FF4500>[ECONOMY] Removed {amount} from {booster}!</color>");
    }

    [Button(ButtonSizes.Large), GUIColor(0.2f, 1f, 0.4f)]
    public void RefillAllBoosters()
    {
        foreach (BoosterType type in Enum.GetValues(typeof(BoosterType)))
        {
            Currency currency = BoosterCurrency(type);
            if (currency != null) Change(currency, refillBoostersTo - currency.Value);
        }
        Debug.Log($"<color=#32CD32>[ECONOMY] Every booster topped up to {refillBoostersTo}.</color>");
    }

    [Button(ButtonSizes.Medium), GUIColor(1f, 0.6f, 0.2f)]
    public void EmptyAllBoosters()
    {
        foreach (BoosterType type in Enum.GetValues(typeof(BoosterType)))
        {
            Currency currency = BoosterCurrency(type);
            if (currency != null) Change(currency, -currency.Value);
        }
    }

    [Button(ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
    public void BuyBoosterWithCoins(BoosterType booster)
    {
        bool bought = boosterManager != null && boosterManager.Buy(booster);
        Debug.Log(bought ? $"[ECONOMY] Bought {booster} with coins." : $"[ECONOMY] Could not buy {booster} (not enough coins or missing reference).");
    }

    // ---------------- Infinite ----------------

    [Title("Infinite Mode Controls")]
    [Button(ButtonSizes.Large), GUIColor(0.8f, 0.2f, 1f)]
    public void GrantInfiniteLivesForDuration(int infiniteTimer = 3600)
    {
        if (lifeCurrency == null) return;

        PlayerPrefsHelper.CanSaveData = true;
        lifeCurrency.AddInfinite(infiniteTimer);

        int missingLives = lifeCurrency.DefaultValue - lifeCurrency.Value;
        if (missingLives > 0) lifeCurrency.Add(missingLives);

        PlayerPrefsHelper.SaveData();
        Notify(lifeCurrency);
        Debug.Log($"<color=#DDA0DD>[ECONOMY] Granted {infiniteTimer / 60} minutes of Infinite Lives & refilled lives!</color>");
    }

    [Button(ButtonSizes.Medium), GUIColor(0.8f, 0.2f, 1f)]
    public void GrantInfiniteBooster(BoosterType booster, int infiniteTimer = 600)
    {
        Currency currency = BoosterCurrency(booster);
        if (currency == null) return;

        PlayerPrefsHelper.CanSaveData = true;
        currency.AddInfinite(infiniteTimer);
        PlayerPrefsHelper.SaveData();
        Notify(currency);
    }

    // ---------------- Reset ----------------

    [Title("Special Controls")]
    [Button(ButtonSizes.Large), GUIColor(1f, 0.5f, 0f)]
    public void ResetEconomyData()
    {
        PlayerPrefsHelper.CanSaveData = true;

        foreach (Currency currency in AllCurrencies())
        {
            currency.Delete();
            Notify(currency);
        }
        if (lifeCurrency != null) lifeCurrency.OnInfiniteOver?.Invoke();

        PlayerPrefsHelper.SaveData();
        Debug.Log("<color=#FFA500>[ECONOMY] Currencies reset to their Default Values.</color>");
    }

    [Button(ButtonSizes.Large), GUIColor(1f, 0f, 0f)]
    public void WipeAllSavedData()
    {
        ResetEconomyData();

        PlayerPrefsHelper.DeleteAllkey();
        PlayerPrefsHelper.SaveData();
        PlayerPrefs.Save();
        Debug.Log("<color=#FF0000>[ECONOMY] ALL SAVED DATA WIPED (level progress included)!</color>");
    }

    // ---------------- Helpers ----------------

    /// <summary>Adds (or removes) an amount, never going below zero, and saves.</summary>
    private static void Change(Currency currency, int amount)
    {
        if (currency == null) return;

        amount = Mathf.Max(amount, -currency.Value);
        if (amount == 0) return;

        PlayerPrefsHelper.CanSaveData = true;
        currency.Add(amount);
        PlayerPrefsHelper.SaveData();
    }

    private static void Notify(Currency currency)
    {
        currency.OnCurrencyChange?.Invoke(currency.Value);
    }

    private Currency BoosterCurrency(BoosterType type)
    {
        BoosterBase booster = boosterManager != null ? boosterManager.GetBooster(type) : null;
        return booster != null ? booster.Currency : null;
    }

    private List<Currency> AllCurrencies()
    {
        List<Currency> all = new List<Currency>();
        if (Coins != null) all.Add(Coins);
        if (lifeCurrency != null) all.Add(lifeCurrency);

        foreach (BoosterType type in Enum.GetValues(typeof(BoosterType)))
        {
            Currency currency = BoosterCurrency(type);
            if (currency != null) all.Add(currency);
        }
        return all;
    }
}