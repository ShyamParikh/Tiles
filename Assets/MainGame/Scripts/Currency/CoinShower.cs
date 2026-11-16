using System.Collections.Generic;
using com.otm.currency;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Shows the player's coins. Whenever coins are earned (a win, a debugger button, anything) a coin shower plays:
/// coin images appear, fly into the counter, and the number counts up as they arrive.
/// </summary>
public class CoinShower : MonoBehaviour
{
    private const string TweenId = "CoinShower";

    [Header("Scene References (Assign in Inspector)")]
    [Tooltip("Initialises the coin currency before this listens to it.")]
    [SerializeField] private TileEconomyManager economyManager;

    [Header("Counter")]
    [SerializeField] private TMP_Text coinText;
    [Tooltip("The coin icon next to the number. Coins fly here and it pulses on every arrival.")]
    [SerializeField] private RectTransform counterTarget;
    [Tooltip("{0} = the coin amount.")]
    [SerializeField] private string textFormat = "{0}";
    [Tooltip("How much the counter icon pulses when a coin arrives.")]
    [SerializeField] private float pulseStrength = 0.15f;

    [Header("Shower")]
    [Tooltip("The coin image prefab (a RectTransform with an Image).")]
    [SerializeField] private RectTransform coinPrefab;
    [Tooltip("Canvas object the coins are created under. Put it above your panels.")]
    [SerializeField] private RectTransform coinParent;
    [Tooltip("Where the coins appear.")]
    [SerializeField] private RectTransform spawnPoint;
    [Tooltip("Size of the area (canvas units) around the spawn point where coins appear. Wide and short = a shower, small = a burst.")]
    [SerializeField] private Vector2 spawnSpread = new Vector2(600f, 200f);
    [Tooltip("Most coin images on screen at once. This is also the size of the pool.")]
    [SerializeField] private int maxCoins = 15;
    [Tooltip("Wait before the shower starts, e.g. to line up with the win panel.")]
    [SerializeField] private float showerDelay = 0.5f;
    [SerializeField] private float popDuration = 0.25f;
    [SerializeField] private float flyDuration = 0.6f;
    [Tooltip("Delay between one coin and the next.")]
    [SerializeField] private float stagger = 0.05f;
    [SerializeField] private Ease flyEase = Ease.InQuad;

    private readonly List<RectTransform> pool = new List<RectTransform>();
    private Currency coins;
    private int unshown;   // coins earned whose images have not arrived at the counter yet
    private bool registered;

    private void Start()
    {
        if (economyManager == null || coinText == null || counterTarget == null || spawnPoint == null || coinPrefab == null || coinParent == null)
        {
            Debug.LogError("[COINS] Economy Manager, Coin Text, Counter Target, Spawn Point, Coin Prefab and Coin Parent must be assigned!", this);
            return;
        }

        economyManager.EnsureInitialized();
        coins = economyManager.CoinCurrency;
        if (coins == null)
        {
            Debug.LogError("[COINS] The Economy Manager has no Coin Currency assigned!", this);
            return;
        }

        for (int i = 0; i < maxCoins; i++)
        {
            RectTransform coin = Instantiate(coinPrefab, coinParent);
            coin.gameObject.SetActive(false);
            pool.Add(coin);
        }

        coins.OnCurrencyChange.RegisterEvent(OnCoinsChanged);
        coins.OnCurrencyEarn.RegisterEvent(PlayShower);
        registered = true;
        Refresh();
    }

    private void OnDisable()
    {
        DOTween.Kill(TweenId);
        foreach (RectTransform coin in pool) coin.gameObject.SetActive(false);

        unshown = 0;
        if (coins != null) Refresh();
    }

    private void OnDestroy()
    {
        if (!registered || coins == null) return;

        coins.OnCurrencyChange.DeRegisterEvent(OnCoinsChanged);
        coins.OnCurrencyEarn.DeRegisterEvent(PlayShower);
    }

    private void OnCoinsChanged(int value)
    {
        Refresh();
    }

    /// <summary>The number on screen is the real amount minus the coins still flying in.</summary>
    private void Refresh()
    {
        coinText.text = string.Format(textFormat, Mathf.Max(0, coins.Value - unshown));
    }

    /// <summary>Plays the shower for this many coins. Called automatically whenever coins are earned.</summary>
    public void PlayShower(int amount)
    {
        if (amount <= 0 || coins == null) return;

        unshown += amount;
        Refresh();

        int visual = Mathf.Max(1, Mathf.Min(amount, pool.Count));
        int share = amount / visual;

        for (int i = 0; i < visual; i++)
        {
            int worth = i == visual - 1 ? amount - share * (visual - 1) : share;
            RectTransform coin = FreeCoin();

            if (coin == null) Arrive(worth);   // no free coin image: just count the coins up
            else Launch(coin, showerDelay + i * stagger, worth);
        }
    }

    private RectTransform FreeCoin()
    {
        foreach (RectTransform coin in pool)
        {
            if (!coin.gameObject.activeSelf) return coin;
        }
        return null;
    }

    private void Launch(RectTransform coin, float delay, int worth)
    {
        coin.gameObject.SetActive(true);
        coin.position = spawnPoint.position;
        coin.localScale = Vector3.zero;

        Vector2 scatter = new Vector2(Random.Range(-0.5f, 0.5f) * spawnSpread.x, Random.Range(-0.5f, 0.5f) * spawnSpread.y);

        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetId(TweenId).SetLink(coin.gameObject);
        sequence.AppendInterval(delay);
        sequence.Append(coin.DOScale(1f, popDuration).SetEase(Ease.OutBack));
        sequence.Join(coin.DOAnchorPos(coin.anchoredPosition + scatter, popDuration).SetEase(Ease.OutQuad));
        sequence.Append(coin.DOMove(counterTarget.position, flyDuration).SetEase(flyEase));
        sequence.Join(coin.DOScale(0.6f, flyDuration));
        sequence.OnComplete(() =>
        {
            coin.gameObject.SetActive(false);
            Arrive(worth);
        });
    }

    private void Arrive(int worth)
    {
        unshown = Mathf.Max(0, unshown - worth);
        Refresh();

        counterTarget.DOKill(true);
        counterTarget.DOPunchScale(Vector3.one * pulseStrength, 0.15f).SetUpdate(true);
    }
}