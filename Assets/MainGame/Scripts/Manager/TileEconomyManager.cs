using System.Collections.Generic;
using com.otm.currency;
using com.otm.manager;
using UnityEngine;

// Owns the game's currencies: starts them up, pays the win reward and runs shop offers.
public class TileEconomyManager : Manager<TileEconomyManager>
{
    [Tooltip("Every currency the game uses (coins, lives, boosters). Each one only once.")]
    [SerializeField] private List<Currency> currencies = new List<Currency>();
    [Tooltip("Your coin currency. It must also be in the list above.")]
    [SerializeField] private Currency coinCurrency;
    [Tooltip("Turn this off if another script already calls Init on the currencies. Init wipes a currency's listeners, so it must only happen once.")]
    [SerializeField] private bool initializeCurrencies = true;
    [Tooltip("Coins paid every time a level is won.")]
    [SerializeField] private int coinsPerWin = 50;
    [SerializeField] private List<EconomyOffer> offers = new List<EconomyOffer>();

    private bool initialized;

    public IReadOnlyList<Currency> Currencies => currencies;
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
        if (state == TileGameState.Won) CurrencyOps.Add(coinCurrency, coinsPerWin);
    }

    // Safe to call many times. Everything that listens to a currency calls this first.
    public void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;

        if (initializeCurrencies) currencies.ForEach(c => { if (c != null) c.Init(); });
    }

    public bool Purchase(string offerId)
    {
        EconomyOffer offer = offers.Find(o => o != null && o.offerId == offerId);
        return offer != null && offer.Buy();
    }
}