using System;
using System.Collections.Generic;
using com.otm.currency;
using com.otm.manager;
using com.otm.utility;
using UnityEngine;

/// <summary>Owns every Currency asset: initialises them, saves after changes, grants / spends, pays the win reward and runs offers.</summary>
public class TileEconomyManager : Manager<TileEconomyManager>
{
    [Header("Currencies (Assign in Inspector)")]
    [Tooltip("Every Currency / RefillCurrency asset the game uses (coins, lives, boosters...). Each one only once.")]
    [SerializeField] private List<Currency> currencies = new List<Currency>();
    [Tooltip("Your coin currency. It pays for booster purchases and coin use, and is rewarded on a win. It must also be in the list above.")]
    [SerializeField] private Currency coinCurrency;
    [Tooltip("On = this manager calls Init() on every currency. Turn OFF if another manager already does (Init resets the currency's events).")]
    [SerializeField] private bool initializeCurrencies = true;
    [Tooltip("On = turns on PlayerPrefsHelper.CanSaveData and saves after every change, so reduced counts stay reduced.")]
    [SerializeField] private bool enableSaving = true;

    [Header("Rewards")]
    [Tooltip("Coins given every time a level is won (replaying a level and winning again pays again).")]
    [SerializeField] private int coinsPerWin = 50;

    [Header("Offers")]
    [SerializeField] private List<EconomyOffer> offers = new List<EconomyOffer>();

    public static Action<EconomyOffer> offerPurchased;
    public static Action<EconomyOffer, string> offerFailed;

    private bool initialized;

    public IReadOnlyList<Currency> Currencies => currencies;
    public IReadOnlyList<EconomyOffer> Offers => offers;
    public Currency CoinCurrency => coinCurrency;
    public int WinReward => coinsPerWin;

    private void OnEnable()
    {
        TileGameManager.stateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        TileGameManager.stateChanged -= OnStateChanged;
    }

    private void Start()
    {
        EnsureInitialized();
    }

    private void OnStateChanged(TileGameState state)
    {
        if (state == TileGameState.Won) Grant(coinCurrency, coinsPerWin);
    }

    /// <summary>Safe to call many times. BoosterManager and CoinShower call it before reading currencies.</summary>
    public void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;

        if (enableSaving) PlayerPrefsHelper.CanSaveData = true;
        if (coinCurrency == null) Debug.LogWarning("[ECONOMY] 'Coin Currency' is not assigned: coin rewards, purchases and coin use will not work.", this);

        if (!initializeCurrencies) return;
        foreach (Currency currency in currencies)
        {
            if (currency != null) currency.Init();
        }
    }

    private void Save()
    {
        if (enableSaving) PlayerPrefsHelper.SaveData();
    }

    public Currency GetCurrency(string assetName)
    {
        foreach (Currency currency in currencies)
        {
            if (currency != null && currency.name == assetName) return currency;
        }
        return null;
    }

    public EconomyOffer GetOffer(string offerId)
    {
        foreach (EconomyOffer offer in offers)
        {
            if (offer != null && offer.offerId == offerId) return offer;
        }
        return null;
    }

    /// <summary>True if the player has enough, or an infinite timer is running on that currency.</summary>
    public bool CanAfford(Currency currency, int amount)
    {
        return currency != null && (amount <= 0 || currency.IsInfiniteOnGoing || currency.Value >= amount);
    }

    public bool Spend(Currency currency, int amount)
    {
        if (!CanAfford(currency, amount)) return false;

        if (amount > 0)
        {
            currency.Add(-amount);
            Save();
        }
        return true;
    }

    public void Grant(Currency currency, int amount)
    {
        if (currency == null || amount <= 0) return;

        currency.Add(amount);
        Save();
    }

    public void GrantInfinite(Currency currency, int seconds)
    {
        if (currency == null || seconds <= 0) return;

        currency.AddInfinite(seconds);
        Save();
    }

    public void ResetCurrency(Currency currency)
    {
        if (currency == null) return;

        currency.Delete();   // back to Default Value, clears any infinite timer
        Save();
    }

    public void ResetAll()
    {
        foreach (Currency currency in currencies)
        {
            if (currency != null) currency.Delete();
        }
        Save();
    }

    public bool Purchase(string offerId)
    {
        return Purchase(GetOffer(offerId));
    }

    public bool Purchase(EconomyOffer offer)
    {
        if (offer == null) return false;
        if (offer.rewardCurrency == null) return Fail(offer, "Offer has no reward currency.");

        bool paid = offer.costCurrency == null || offer.costAmount <= 0 || Spend(offer.costCurrency, offer.costAmount);
        if (!paid) return Fail(offer, $"Not enough {offer.costCurrency.name}.");

        if (offer.rewardInfiniteSeconds > 0) GrantInfinite(offer.rewardCurrency, offer.rewardInfiniteSeconds);
        else Grant(offer.rewardCurrency, offer.rewardAmount);

        offerPurchased?.Invoke(offer);
        return true;
    }

    private bool Fail(EconomyOffer offer, string reason)
    {
        offerFailed?.Invoke(offer, reason);
        return false;
    }
}