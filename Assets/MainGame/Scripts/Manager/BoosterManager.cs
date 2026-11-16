using System;
using System.Collections.Generic;
using com.otm.manager;
using UnityEngine;

/// <summary>Entry point for all boosters: one at a time, tray locked while running, cancelled on restart / level end.</summary>
public class BoosterManager : Manager<BoosterManager>
{
    [Header("Scene References (Assign in Inspector)")]
    [SerializeField] private TileGameManager gameManager;
    [SerializeField] private TileBoardManager boardManager;
    [SerializeField] private TileTrayManager trayManager;
    [SerializeField] private TileArrangementManager arrangementManager;
    [Tooltip("Initialises the booster currencies and pays for purchases and coin use.")]
    [SerializeField] private TileEconomyManager economyManager;

    [Header("Boosters (Assign in Inspector)")]
    [SerializeField] private List<BoosterBase> boosters = new List<BoosterBase>();

    public static Action<BoosterBase> boosterUsed;
    [Tooltip("Fired when a booster is pressed with 0 stock and the player cannot pay its coin cost (open a shop popup from this).")]
    public static Action<BoosterBase> boosterEmpty;
    public static Action<BoosterBase> boosterUnavailable;
    public static Action<BoosterBase, int> boosterPaidWithCoins;
    public static Action<BoosterBase> boosterPurchased;
    public static Action<BoosterBase> boosterPurchaseFailed;
    public static Action<bool> busyChanged;

    private bool isBusy;
    private int executionToken;

    public bool IsBusy => isBusy;
    public TileBoardManager Board => boardManager;
    public TileTrayManager Tray => trayManager;
    public TileArrangementManager Arrangement => arrangementManager;

    private void OnEnable()
    {
        TileGameManager.levelStarted += OnLevelStarted;
        TileGameManager.stateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        TileGameManager.levelStarted -= OnLevelStarted;
        TileGameManager.stateChanged -= OnStateChanged;
    }

    private void Start()
    {
        if (gameManager == null || boardManager == null || trayManager == null || arrangementManager == null)
            Debug.LogError("[BOOSTER] Game, Board, Tray and Arrangement managers must be assigned!", this);

        if (economyManager != null) economyManager.EnsureInitialized();
        else Debug.LogWarning("[BOOSTER] No Economy Manager assigned: counts may not save, and coin use and purchases are impossible.", this);

        foreach (BoosterBase booster in boosters)
        {
            if (booster != null) booster.Initialize(this);
        }
    }

    public BoosterBase GetBooster(BoosterType type)
    {
        foreach (BoosterBase booster in boosters)
        {
            if (booster != null && booster.Type == type) return booster;
        }
        return null;
    }

    public bool Use(BoosterType type)
    {
        return Use(GetBooster(type));
    }

    /// <summary>Uses a booster. With a count of 0 it pays the booster's coin cost instead, if the player can afford it.</summary>
    public bool Use(BoosterBase booster)
    {
        if (booster == null || isBusy || !gameManager.IsPlaying || trayManager.IsLocked) return false;

        bool payWithCoins = !booster.HasStock;
        if (payWithCoins && !CanPayWithCoins(booster))
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
        int token = ++executionToken;

        booster.Execute(success =>
        {
            if (token != executionToken) return;   // cancelled by a restart or level end

            if (success) Pay(booster, payWithCoins);
            SetBusy(false);
        });
        return true;
    }

    private bool CanPayWithCoins(BoosterBase booster)
    {
        return booster.AllowCoinUse && booster.CoinUseCost > 0 && economyManager != null
            && economyManager.CanAfford(economyManager.CoinCurrency, booster.CoinUseCost);
    }

    /// <summary>The charge is only taken once the booster really worked.</summary>
    private void Pay(BoosterBase booster, bool withCoins)
    {
        if (withCoins)
        {
            economyManager.Spend(economyManager.CoinCurrency, booster.CoinUseCost);
            boosterPaidWithCoins?.Invoke(booster, booster.CoinUseCost);
        }
        else
        {
            booster.Consume();
        }
        boosterUsed?.Invoke(booster);
    }

    public bool Buy(BoosterType type)
    {
        return Buy(GetBooster(type));
    }

    /// <summary>Pays the booster's own coin price and adds its purchase amount. Works in any game state.</summary>
    public bool Buy(BoosterBase booster)
    {
        if (booster == null || economyManager == null) return false;

        if (!economyManager.Spend(economyManager.CoinCurrency, booster.CoinPrice))
        {
            boosterPurchaseFailed?.Invoke(booster);
            return false;
        }

        booster.AddCount(booster.AmountPerPurchase);
        boosterPurchased?.Invoke(booster);
        return true;
    }

    private void SetBusy(bool busy)
    {
        isBusy = busy;
        trayManager.SetExternalLock(busy);
        busyChanged?.Invoke(busy);
    }

    private void Cancel()
    {
        executionToken++;
        if (isBusy) SetBusy(false);
    }

    private void OnLevelStarted(int levelNumber)
    {
        Cancel();
        foreach (BoosterBase booster in boosters)
        {
            if (booster != null) booster.ResetForNewLevel();
        }
    }

    private void OnStateChanged(TileGameState state)
    {
        if (state != TileGameState.Playing) Cancel();
    }
}