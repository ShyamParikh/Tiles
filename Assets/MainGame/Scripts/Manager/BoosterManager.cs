using System;
using System.Collections.Generic;
using com.otm.manager;
using UnityEngine;

// The one entry point for boosters. Only one runs at a time, the tray is locked while it runs,
// and a restart or level end cancels it. The charge is only taken once the booster really worked.
public class BoosterManager : Manager<BoosterManager>
{
    [Header("Scene references")]
    [SerializeField] private TileGameManager gameManager;
    [SerializeField] private TileBoardManager boardManager;
    [SerializeField] private TileTrayManager trayManager;
    [SerializeField] private TileArrangementManager arrangementManager;
    [Tooltip("Starts up the booster currencies and holds the coin currency.")]
    [SerializeField] private TileEconomyManager economyManager;

    [Header("Boosters")]
    [SerializeField] private List<BoosterBase> boosters = new List<BoosterBase>();

    public static Action<BoosterBase> boosterUsed;
    // Pressed with 0 stock and not enough coins: a good moment to open your shop.
    public static Action<BoosterBase> boosterEmpty;
    public static Action<BoosterBase> boosterUnavailable;
    public static Action<BoosterBase, int> boosterPaidWithCoins;
    public static Action<BoosterBase> boosterPurchased;
    public static Action<BoosterBase> boosterPurchaseFailed;

    private bool isBusy;
    private int token;

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
        // The currencies must be started before the boosters listen to them.
        if (economyManager != null) economyManager.EnsureInitialized();
        boosters.ForEach(b => { if (b != null) b.Initialize(this); });
    }

    public BoosterBase GetBooster(BoosterType type)
    {
        return boosters.Find(b => b != null && b.Type == type);
    }

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

    private bool CanPayWithCoins(BoosterBase booster)
    {
        return booster.AllowCoinUse && booster.CoinUseCost > 0 && economyManager != null
            && CurrencyOps.CanAfford(economyManager.CoinCurrency, booster.CoinUseCost);
    }

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

    private void SetBusy(bool busy)
    {
        isBusy = busy;
        trayManager.SetExternalLock(busy);
    }

    private void Cancel()
    {
        token++;
        if (isBusy) SetBusy(false);
    }

    private void OnLevelStarted(int levelNumber)
    {
        Cancel();
        boosters.ForEach(b => { if (b != null) b.ResetForNewLevel(); });
    }

    private void OnStateChanged(TileGameState state)
    {
        if (state != TileGameState.Playing) Cancel();
    }
}