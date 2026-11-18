/*
 * BoosterManager
 * --------------
 * The one entry point for boosters. Use(booster) runs a booster only if the game is Playing, nothing else is
 * running, the tray isn't locked and the booster has something to do. Only one booster runs at a time and the
 * tray is locked while it runs; a level restart or the end of the level cancels it. The charge is only taken
 * once the booster really worked: with stock it uses one, with a count of 0 it pays the booster's coin cost
 * instead (if allowed and affordable), otherwise it fires boosterEmpty (a good moment to open a shop). Buy
 * pays the booster's own coin price and adds its purchase amount, in any game state. At start it asks the
 * economy manager to initialise the currencies before the boosters listen to them. Other scripts read the
 * board, tray and arrangement managers through it.
 */
using System;
using System.Collections.Generic;
using com.otm.manager;
using UnityEngine;

public class BoosterManager : Manager<BoosterManager>
{
    #region Public Variables

    public static Action<BoosterBase> boosterUsed;
    // Pressed with 0 stock and not enough coins: a good moment to open your shop.
    public static Action<BoosterBase> boosterEmpty;
    public static Action<BoosterBase> boosterUnavailable;
    public static Action<BoosterBase, int> boosterPaidWithCoins;
    public static Action<BoosterBase> boosterPurchased;
    public static Action<BoosterBase> boosterPurchaseFailed;

    #endregion

    #region Private Variables

    [Header("Scene references")]
    [SerializeField] private TileGameManager gameManager;
    [SerializeField] private TileBoardManager boardManager;
    [SerializeField] private TileTrayManager trayManager;
    [SerializeField] private TileArrangementManager arrangementManager;
    [Tooltip("Starts up the booster currencies and holds the coin currency.")]
    [SerializeField] private TileEconomyManager economyManager;

    [Header("Boosters")]
    [SerializeField] private List<BoosterBase> boosters = new List<BoosterBase>();

    private bool isBusy;
    private int token;

    #endregion

    #region Properties

    public bool IsBusy => isBusy;
    public TileBoardManager Board => boardManager;
    public TileTrayManager Tray => trayManager;
    public TileArrangementManager Arrangement => arrangementManager;

    #endregion

    #region Unity Callbacks

    // Starts listening for level changes.
    private void OnEnable()
    {
        TileGameManager.levelStarted += OnLevelStarted;
        TileGameManager.stateChanged += OnStateChanged;
    }

    // Stops listening.
    private void OnDisable()
    {
        TileGameManager.levelStarted -= OnLevelStarted;
        TileGameManager.stateChanged -= OnStateChanged;
    }

    // Initialises the currencies first, then every booster.
    private void Start()
    {
        if (economyManager != null) economyManager.EnsureInitialized();
        boosters.ForEach(b => { if (b != null) b.Initialize(this); });
    }

    #endregion

    #region Looking Up

    // The booster of this type (null if there is none).
    public BoosterBase GetBooster(BoosterType type)
    {
        return boosters.Find(b => b != null && b.Type == type);
    }

    #endregion

    #region Using Boosters

    // Uses the booster of this type.
    public bool Use(BoosterType type) { return Use(GetBooster(type)); }

    // With a count of 0 the booster is paid for with coins instead, if the player can afford it.
    public bool Use(BoosterBase booster)
    {
        if (booster == null || isBusy || !gameManager.IsPlaying || trayManager.IsLocked) return false;

        bool withCoins = !booster.HasStock;
        if (withCoins && !CanPayWithCoins(booster))
        {
            boosterEmpty?.Invoke(booster);
            return false;
        }
        if (!booster.CanExecute())
        {
            boosterUnavailable?.Invoke(booster);
            return false;
        }

        SetBusy(true);
        int myToken = ++token;

        booster.Execute(success =>
        {
            if (myToken != token) return;   // cancelled by a restart or the end of the level

            if (success) Pay(booster, withCoins);
            SetBusy(false);
        });
        return true;
    }

    // Can the player pay this booster's coin cost?
    private bool CanPayWithCoins(BoosterBase booster)
    {
        return booster.AllowCoinUse && booster.CoinUseCost > 0 && economyManager != null
            && CurrencyOps.CanAfford(economyManager.CoinCurrency, booster.CoinUseCost);
    }

    // Takes the charge: coins or one booster.
    private void Pay(BoosterBase booster, bool withCoins)
    {
        if (withCoins)
        {
            CurrencyOps.Spend(economyManager.CoinCurrency, booster.CoinUseCost);
            boosterPaidWithCoins?.Invoke(booster, booster.CoinUseCost);
        }
        else
        {
            booster.Consume();
        }
        boosterUsed?.Invoke(booster);
    }

    // Locks or unlocks the tray around a running booster.
    private void SetBusy(bool busy)
    {
        isBusy = busy;
        trayManager.SetExternalLock(busy);
    }

    // Cancels a running booster.
    private void Cancel()
    {
        token++;
        if (isBusy) SetBusy(false);
    }

    #endregion

    #region Buying Boosters

    // Buys the booster of this type with coins.
    public bool Buy(BoosterType type) { return Buy(GetBooster(type)); }

    // Pays the booster's own coin price and adds its purchase amount. Works in any game state.
    public bool Buy(BoosterBase booster)
    {
        if (booster == null) return false;

        if (economyManager == null || !CurrencyOps.Spend(economyManager.CoinCurrency, booster.CoinPrice))
        {
            boosterPurchaseFailed?.Invoke(booster);
            return false;
        }

        booster.AddCount(booster.AmountPerPurchase);
        boosterPurchased?.Invoke(booster);
        return true;
    }

    #endregion

    #region Level Events

    // A new level cancels any booster and resets their state.
    private void OnLevelStarted(int levelNumber)
    {
        Cancel();
        boosters.ForEach(b => { if (b != null) b.ResetForNewLevel(); });
    }

    // Leaving the Playing state cancels any booster.
    private void OnStateChanged(TileGameState state)
    {
        if (state != TileGameState.Playing) Cancel();
    }

    #endregion
}