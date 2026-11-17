using System.Collections.Generic;
using com.otm.currency;
using DG.Tweening;
using TMPro;
using UnityEngine;

// Shows the player's coins. When coins are earned, coin images pop out, fly into the counter,
// and the number counts up as each one lands.
public class CoinShower : MonoBehaviour
{
    private const string TweenId = "CoinShower";

    [SerializeField] private TileEconomyManager economyManager;

    [Header("Counter")]
    [SerializeField] private TMP_Text coinText;
    [Tooltip("The coin icon next to the number. Coins fly here and it pulses when one lands.")]
    [SerializeField] private RectTransform counterTarget;
    [Tooltip("{0} = the coin amount.")]
    [SerializeField] private string textFormat = "{0}";
    [SerializeField] private float pulseStrength = 0.15f;

    [Header("Shower")]
    [Tooltip("The coin image to copy (a RectTransform with an Image).")]
    [SerializeField] private RectTransform coinPrefab;
    [Tooltip("The canvas object the coins live under. Put it above your panels.")]
    [SerializeField] private RectTransform coinParent;
    [SerializeField] private RectTransform spawnPoint;
    [Tooltip("Size of the area (canvas units) coins appear in. Wide and short = a shower, small = a burst.")]
    [SerializeField] private Vector2 spawnSpread = new Vector2(600f, 200f);
    [Tooltip("Most coin images on screen at once.")]
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
    private CurrencyWatcher watcher;
    private int unshown;   // coins earned whose images haven't landed yet

    private void Start()
    {
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

        watcher = new CurrencyWatcher(coins, Refresh);
        coins.OnCurrencyEarn.RegisterEvent(PlayShower);
        Refresh();
    }

    private void OnDisable()
    {
        DOTween.Kill(TweenId);
        pool.ForEach(c => c.gameObject.SetActive(false));

        unshown = 0;
        if (coins != null) Refresh();
    }

    private void OnDestroy()
    {
        if (watcher == null) return;

        watcher.Dispose();
        coins.OnCurrencyEarn.DeRegisterEvent(PlayShower);
    }

    // The number on screen is the real amount minus the coins still flying in.
    private void Refresh()
    {
        coinText.text = string.Format(textFormat, Mathf.Max(0, coins.Value - unshown));
    }

    // Plays the shower for this many coins. It runs automatically whenever coins are earned.
    public void PlayShower(int amount)
    {
        if (amount <= 0) return;

        unshown += amount;
        Refresh();

        // Few images carry many coins, so a big reward doesn't need a hundred sprites.
        int images = Mathf.Max(1, Mathf.Min(amount, pool.Count));
        int share = amount / images;

        for (int i = 0; i < images; i++)
        {
            int worth = i == images - 1 ? amount - share * (images - 1) : share;
            RectTransform coin = pool.Find(c => !c.gameObject.activeSelf);

            if (coin == null) Land(worth);   // no free image: just count the coins up
            else Launch(coin, showerDelay + i * stagger, worth);
        }
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
            Land(worth);
        });
    }

    // A coin image reached the counter: show its coins and pulse the icon.
    private void Land(int worth)
    {
        unshown = Mathf.Max(0, unshown - worth);
        Refresh();

        counterTarget.DOKill(true);
        counterTarget.DOPunchScale(Vector3.one * pulseStrength, 0.15f).SetUpdate(true);
    }
}