/*
 * TileEconomyManager
 * ------------------
 * Owns the game's currencies. It holds the list of every currency asset (coins, lives, boosters), starts them
 * up once (Currency.Init wipes a currency's listeners, so untick "Initialize Currencies" if another script
 * already calls it), pays the coin reward every time a level is won, and runs shop offers by id. Everything that
 * listens to a currency (boosters, the coin shower) calls EnsureInitialized first, so the order scripts start in
 * doesn't matter. The coin currency must also be in the list. Money changes themselves go through CurrencyOps.
 */
using System.Collections.Generic;
using com.otm.currency;
using com.otm.manager;
using UnityEngine;

public class TileEconomyManager : Manager<TileEconomyManager>
{
    #region Private Variables

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

    #endregion

    #region Properties

    public IReadOnlyList<Currency> Currencies => currencies;
    public Currency CoinCurrency => coinCurrency;
    public int WinReward => coinsPerWin;

    #endregion

    #region Unity Callbacks

    // Starts listening for wins.
    private void OnEnable()
    {
        TileGameManager.stateChanged += OnStateChanged;
    }

    // Stops listening.
    private void OnDisable()
    {
        TileGameManager.stateChanged -= OnStateChanged;
    }

    // Starts up the currencies.
    private void Start()
    {
        EnsureInitialized();
    }

    #endregion

    #region Currencies

    // Safe to call many times. Everything that listens to a currency calls this first.
    public void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;

        if (initializeCurrencies) currencies.ForEach(c => { if (c != null) c.Init(); });
    }

    // Pays the coin reward when a level is won.
    private void OnStateChanged(TileGameState state)
    {
        if (state == TileGameState.Won) CurrencyOps.Add(coinCurrency, coinsPerWin);
    }

    #endregion

    #region Offers

    // Buys the offer with this id. Returns false if it doesn't exist or the player can't pay.
    public bool Purchase(string offerId)
    {
        EconomyOffer offer = offers.Find(o => o != null && o.offerId == offerId);
        return offer != null && offer.Buy();
    }

    #endregion
}