using System;
using System.Collections.Generic;
using com.otm.currency;
using com.otm.manager;
using UnityEngine;

/// <summary>
/// A purchase the game can offer: pay a cost (or nothing, e.g. after a rewarded ad) and receive a reward.
/// Example: offer_booster_shuffle_x3 = 100 Currency_Coins -> 3 Currency_Booster_Shuffle.
/// </summary>
[Serializable]
public class EconomyOffer
{
    [Tooltip("Unique id, lower_snake_case. See the naming conventions, e.g. offer_booster_shuffle_x3.")]
    public string offerId = "";

    [Tooltip("What the player pays with. Leave empty for a free offer (e.g. granted after a rewarded ad).")]
    public Currency costCurrency;
    public int costAmount = 0;

    [Tooltip("What the player receives.")]
    public Currency rewardCurrency;
    public int rewardAmount = 1;

    [Tooltip("If above 0, the reward is 'infinite use' for this many seconds (Currency.AddInfinite) instead of an amount.")]
    public int rewardInfiniteSeconds = 0;
}

/// <summary>
/// One place that owns every Currency asset of the game: initialises them, grants / spends, runs offers,
/// and offers a debug panel (Inspector buttons in Play Mode + optional on-screen overlay for builds).
/// </summary>
public class TileEconomyManager : Manager<TileEconomyManager>
{
    [Header("Currencies (Assign in Inspector)")]
    [Tooltip("Every Currency / RefillCurrency asset the game uses: coins, gems, lives, boosters...")]
    [SerializeField] private List<Currency> currencies = new List<Currency>();
    [Tooltip("On = this manager calls Init() on every currency. Turn OFF if another manager in your project already does it " +
             "(Init resets the currency's events, so it must only happen once).")]
    [SerializeField] private bool initializeCurrencies = true;

    [Header("Offers")]
    [SerializeField] private List<EconomyOffer> offers = new List<EconomyOffer>();

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [Tooltip("Shows a small on-screen currency panel (Editor and Development Builds only).")]
    [SerializeField] private bool showDebugOverlay = false;

    [Header("Actions")]
    public static Action<Currency, int> currencyChanged;
    public static Action<EconomyOffer> offerPurchased;
    public static Action<EconomyOffer, string> offerFailed;

    private readonly Dictionary<Currency, Action<int>> changeListeners = new Dictionary<Currency, Action<int>>();
    private bool initialized;
    private bool listenersRegistered;

    public IReadOnlyList<Currency> Currencies => currencies;
    public IReadOnlyList<EconomyOffer> Offers => offers;
    public bool IsInitialized => initialized;

    private void OnEnable()
    {
        if (initialized) RegisterListeners();
    }

    private void OnDisable()
    {
        UnregisterListeners();
    }

    private void Start()
    {
        EnsureInitialized();
    }

    /// <summary>Safe to call many times. BoosterManager calls it before reading booster counts.</summary>
    public void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;

        HashSet<Currency> seen = new HashSet<Currency>();
        for (int i = 0; i < currencies.Count; i++)
        {
            Currency currency = currencies[i];
            if (currency == null)
            {
                Debug.LogWarning($"[ECONOMY] Currencies list has an empty element at index {i}.", this);
                continue;
            }

            if (!seen.Add(currency))
            {
                Debug.LogWarning($"[ECONOMY] '{currency.name}' is in the Currencies list more than once. Init is only called once.", this);
                continue;
            }

            if (initializeCurrencies) currency.Init();
        }

        RegisterListeners();

        if (enableDebugLogs) Debug.Log($"[ECONOMY] Initialised {seen.Count} currencies (Init called: {initializeCurrencies}).");
    }

    private void RegisterListeners()
    {
        if (listenersRegistered) return;
        listenersRegistered = true;

        for (int i = 0; i < currencies.Count; i++)
        {
            Currency currency = currencies[i];
            if (currency == null || changeListeners.ContainsKey(currency)) continue;

            Currency captured = currency;
            Action<int> listener = value => currencyChanged?.Invoke(captured, value);
            changeListeners[currency] = listener;
            currency.OnCurrencyChange.RegisterEvent(listener);
        }
    }

    private void UnregisterListeners()
    {
        if (!listenersRegistered) return;
        listenersRegistered = false;

        foreach (KeyValuePair<Currency, Action<int>> pair in changeListeners)
        {
            if (pair.Key != null) pair.Key.OnCurrencyChange.DeRegisterEvent(pair.Value);
        }
        changeListeners.Clear();
    }

    // ---------------- Lookup ----------------

    /// <summary>Finds a currency by its asset name, e.g. "Currency_Coins".</summary>
    public Currency GetCurrency(string assetName)
    {
        for (int i = 0; i < currencies.Count; i++)
        {
            if (currencies[i] != null && currencies[i].name == assetName) return currencies[i];
        }
        return null;
    }

    public int GetValue(Currency currency)
    {
        return currency != null ? currency.Value : 0;
    }

    public EconomyOffer GetOffer(string offerId)
    {
        for (int i = 0; i < offers.Count; i++)
        {
            if (offers[i] != null && offers[i].offerId == offerId) return offers[i];
        }
        return null;
    }

    // ---------------- Earn / Spend ----------------

    /// <summary>True if the player has enough, or has an infinite timer running on that currency.</summary>
    public bool CanAfford(Currency currency, int amount)
    {
        if (currency == null) return false;
        if (amount <= 0) return true;
        return currency.IsInfiniteOnGoing || currency.Value >= amount;
    }

    public bool TrySpend(Currency currency, int amount)
    {
        if (amount <= 0) return true;
        if (!CanAfford(currency, amount)) return false;

        currency.Add(-amount);   // ignored by Currency while infinite is active
        return true;
    }

    public void Grant(Currency currency, int amount)
    {
        if (currency == null || amount <= 0) return;
        currency.Add(amount);
    }

    public void GrantInfinite(Currency currency, int seconds)
    {
        if (currency == null || seconds <= 0) return;
        currency.AddInfinite(seconds);
    }

    public void ResetCurrency(Currency currency)
    {
        if (currency == null) return;
        currency.Delete();   // back to Default Value, clears any infinite timer
    }

    public void ResetAll()
    {
        for (int i = 0; i < currencies.Count; i++) ResetCurrency(currencies[i]);
        if (enableDebugLogs) Debug.Log("[ECONOMY] All currencies reset to their default values.");
    }

    // ---------------- Offers ----------------

    public bool TryPurchase(string offerId)
    {
        EconomyOffer offer = GetOffer(offerId);
        if (offer == null)
        {
            Debug.LogWarning($"[ECONOMY] Offer '{offerId}' not found.", this);
            return false;
        }
        return TryPurchase(offer);
    }

    public bool TryPurchase(EconomyOffer offer)
    {
        if (offer == null) return false;

        if (offer.rewardCurrency == null)
        {
            Fail(offer, "Offer has no reward currency.");
            return false;
        }

        bool hasCost = offer.costCurrency != null && offer.costAmount > 0;
        if (hasCost && !TrySpend(offer.costCurrency, offer.costAmount))
        {
            Fail(offer, $"Not enough {offer.costCurrency.name}.");
            return false;
        }

        if (offer.rewardInfiniteSeconds > 0)
            GrantInfinite(offer.rewardCurrency, offer.rewardInfiniteSeconds);
        else
            Grant(offer.rewardCurrency, offer.rewardAmount);

        if (enableDebugLogs) Debug.Log($"[ECONOMY] Purchased '{offer.offerId}'.");
        offerPurchased?.Invoke(offer);
        return true;
    }

    private void Fail(EconomyOffer offer, string reason)
    {
        if (enableDebugLogs) Debug.Log($"[ECONOMY] Offer '{offer.offerId}' failed: {reason}");
        offerFailed?.Invoke(offer, reason);
    }

    // ---------------- On-screen debug overlay ----------------

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private Vector2 overlayScroll;

    private void OnGUI()
    {
        if (!showDebugOverlay || !initialized) return;

        float scale = Mathf.Max(1f, Screen.dpi / 160f);
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

        float width = 340f;
        float height = Screen.height / scale - 20f;

        GUILayout.BeginArea(new Rect(10f, 10f, width, height), GUI.skin.box);
        GUILayout.Label("ECONOMY DEBUG");
        overlayScroll = GUILayout.BeginScrollView(overlayScroll);

        for (int i = 0; i < currencies.Count; i++)
        {
            Currency c = currencies[i];
            if (c == null) continue;

            string value = c.IsInfiniteOnGoing ? "INF " + c.InfiniteTime.ToString(@"hh\:mm\:ss") : c.Value.ToString();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{c.name}: {value}", GUILayout.Width(190f));
            if (GUILayout.Button("-1")) TrySpend(c, 1);
            if (GUILayout.Button("+1")) Grant(c, 1);
            if (GUILayout.Button("+10")) Grant(c, 10);
            if (GUILayout.Button("R")) ResetCurrency(c);
            GUILayout.EndHorizontal();
        }

        if (offers.Count > 0)
        {
            GUILayout.Space(6f);
            GUILayout.Label("Offers");
            for (int i = 0; i < offers.Count; i++)
            {
                if (offers[i] == null) continue;
                if (GUILayout.Button(offers[i].offerId)) TryPurchase(offers[i]);
            }
        }

        GUILayout.Space(6f);
        if (GUILayout.Button("Reset ALL currencies")) ResetAll();

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
#endif
}