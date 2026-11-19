/*
 * TileEconomyDebugger
 * -------------------
 * An inspector debugger for the whole economy, in the style of your own EconomyDebugger (Odin buttons). At the
 * top is a live table of every currency in the economy manager's list: type a number into an Amount cell and
 * press Enter to set it (it repaints while the game runs, so you can watch values change), or use the row
 * buttons (-1, +1, +10, Reset). Below are buttons for a raw hard-drive scan, lives (fill, infinite), boosters
 * (refill all, empty all, buy with coins, infinite) and resets (economy only, or wipe everything, level
 * progress included). It works in and outside Play Mode and every change is saved through CurrencyOps.
 * The live table is drawn by hand in the editor (not as an Odin list) so its cells are real editable fields.
 */
using System;
using System.Collections.Generic;
using com.otm.currency;
using com.otm.utility;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TileEconomyDebugger : MonoBehaviour
{
    #region Private Variables

    [Tooltip("Its Currencies list fills the table.")]
    [SerializeField] private TileEconomyManager economyManager;
    [Tooltip("Used for the booster buttons and for buying with coins.")]
    [SerializeField] private BoosterManager boosterManager;
    [Tooltip("Optional: only needed for the lives buttons.")]
    [SerializeField] private RefillCurrency lifeCurrency;
    [Tooltip("'Refill All Boosters' tops every booster up to this amount.")]
    [SerializeField] private int refillBoostersTo = 5;

    #endregion

    #region Unity Callbacks

    // Makes sure saving is allowed.
    private void Awake()
    {
        PlayerPrefsHelper.CanSaveData = true;
    }

    #endregion

#if UNITY_EDITOR
    #region Live Table

    // Draws the live table by hand, so every Amount is a normal editable number field.
    [OnInspectorGUI, PropertyOrder(-10)]
    private void DrawAmounts()
    {
        GUILayout.Label("Live amounts (type a number, then press Enter)", EditorStyles.boldLabel);

        if (economyManager == null)
        {
            EditorGUILayout.HelpBox("Assign the Economy Manager above to list your currencies.", MessageType.Warning);
            return;
        }

        List<Currency> currencies = AllCurrencies();
        if (currencies.Count == 0)
        {
            EditorGUILayout.HelpBox("The Economy Manager's Currencies list is empty. Drag your currency assets into it.", MessageType.Info);
            return;
        }

        PlayerPrefsHelper.CanSaveData = true;
        foreach (Currency currency in currencies) DrawRow(currency);

        if (Application.isPlaying) Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
    }

    // One currency: name, editable amount, infinite timer, and quick buttons.
    private static void DrawRow(Currency currency)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(currency.name, GUILayout.Width(180f));

        // Delayed = the value is applied on Enter / focus loss, so typing "25" doesn't set 2 first.
        EditorGUI.BeginChangeCheck();
        int amount = EditorGUILayout.DelayedIntField(currency.Value, GUILayout.Width(90f));
        if (EditorGUI.EndChangeCheck()) CurrencyOps.Set(currency, amount);

        string infinite = currency.IsInfiniteOnGoing ? currency.InfiniteTime.ToString(@"hh\:mm\:ss") : "-";
        EditorGUILayout.LabelField(infinite, GUILayout.Width(70f));

        if (GUILayout.Button("-1", GUILayout.Width(36f))) CurrencyOps.Add(currency, -1);
        if (GUILayout.Button("+1", GUILayout.Width(36f))) CurrencyOps.Add(currency, 1);
        if (GUILayout.Button("+10", GUILayout.Width(40f))) CurrencyOps.Add(currency, 10);
        if (GUILayout.Button("Reset", GUILayout.Width(50f))) CurrencyOps.Reset(currency);
        EditorGUILayout.EndHorizontal();
    }

    #endregion
#endif

    #region Diagnostics

    // Prints what is really saved on the device for every currency.
    [Title("Hard drive diagnostics")]
    [Button(ButtonSizes.Large), GUIColor(1f, 1f, 0f)]
    public void PrintRawHardDriveData()
    {
        PlayerPrefsHelper.CanSaveData = true;
        foreach (Currency currency in AllCurrencies())
        {
            string key = currency.InfiniteEndDateKey.Replace("InfiniteEndDate", "");
            Debug.Log($"Raw {currency.name} saved in OS: {PlayerPrefsHelper.GetInt(key, -999)}");
        }
    }

    #endregion

    #region Lives

    // Fills the lives to their default (full) amount.
    [Title("Lives")]
    [Button(ButtonSizes.Medium), GUIColor(0.2f, 1f, 0.4f)]
    public void FillLives()
    {
        if (lifeCurrency != null) CurrencyOps.Set(lifeCurrency, lifeCurrency.DefaultValue);
    }

    // Gives infinite lives for some seconds and fills them up.
    [Button(ButtonSizes.Medium), GUIColor(0.8f, 0.2f, 1f)]
    public void GrantInfiniteLives(int seconds = 3600)
    {
        CurrencyOps.AddInfinite(lifeCurrency, seconds);
        FillLives();
    }

    #endregion

    #region Boosters

    // Tops every booster up to the refill amount.
    [Title("Boosters")]
    [Button(ButtonSizes.Large), GUIColor(0.2f, 1f, 0.4f)]
    public void RefillAllBoosters()
    {
        ForEachBooster(c => { if (c.Value < refillBoostersTo) CurrencyOps.Set(c, refillBoostersTo); });
    }

    // Sets every booster to 0 (to test the empty flow).
    [Button(ButtonSizes.Medium), GUIColor(1f, 0.6f, 0.2f)]
    public void EmptyAllBoosters()
    {
        ForEachBooster(c => CurrencyOps.Set(c, 0));
    }

    // Buys one purchase of a booster with coins.
    [Button(ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
    public void BuyBoosterWithCoins(BoosterType booster)
    {
        bool bought = boosterManager != null && boosterManager.Buy(booster);
        Debug.Log(bought ? $"Bought {booster} with coins." : $"Could not buy {booster} (not enough coins or missing reference).");
    }

    // Gives a booster for free for some seconds.
    [Button(ButtonSizes.Medium), GUIColor(0.8f, 0.2f, 1f)]
    public void GrantInfiniteBooster(BoosterType booster, int seconds = 600)
    {
        BoosterBase target = boosterManager != null ? boosterManager.GetBooster(booster) : null;
        if (target != null) CurrencyOps.AddInfinite(target.Currency, seconds);
    }

    #endregion

    #region Reset

    // Puts every currency back on its Default Value.
    [Title("Reset")]
    [Button(ButtonSizes.Large), GUIColor(1f, 0.5f, 0f)]
    public void ResetEconomyData()
    {
        foreach (Currency currency in AllCurrencies()) CurrencyOps.Reset(currency);
        if (lifeCurrency != null) lifeCurrency.OnInfiniteOver?.Invoke();
    }

    // Wipes every saved value (careful: level progress too).
    [Button(ButtonSizes.Large), GUIColor(1f, 0f, 0f)]
    public void WipeAllSavedData()
    {
        ResetEconomyData();
        PlayerPrefsHelper.DeleteAllkey();
        PlayerPrefsHelper.SaveData();
        PlayerPrefs.Save();
    }

    #endregion

    #region Helpers

    // Every currency in the economy manager's list.
    private List<Currency> AllCurrencies()
    {
        List<Currency> all = new List<Currency>();
        if (economyManager == null) return all;

        foreach (Currency currency in economyManager.Currencies)
        {
            if (currency != null) all.Add(currency);
        }
        return all;
    }

    // Runs an action on the currency of every booster.
    private void ForEachBooster(Action<Currency> action)
    {
        foreach (BoosterType type in Enum.GetValues(typeof(BoosterType)))
        {
            BoosterBase booster = boosterManager != null ? boosterManager.GetBooster(type) : null;
            if (booster != null && booster.Currency != null) action(booster.Currency);
        }
    }

    #endregion
}