using System.Collections.Generic;
using com.otm.currency;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shows the player's coins. When coins are earned, spinning coin sprites burst out, hover for a beat,
// then curve into the counter. The number goes up with each coin that lands and ends on the real amount.
public class CoinShower : MonoBehaviour
{
    private const string TweenId = "CoinShower";
    private const float RecheckSeconds = 0.25f;

    // One pooled coin and the part of the earned amount it carries to the counter.
    private class CoinSlot
    {
        public Image image;
        public RectTransform rect;
        public CoinShower owner;
        public bool busy;
        public int share;

        public Vector2 burstStart;
        public Vector2 burstEnd;
        public Vector3 flyStart;
        public Vector3 flyControl;
        public bool flyStarted;
        public float spinOffset;
        public float spinSpeed;
        public float curveSide;
        public float arcHeight;

        public void Begin()
        {
            owner.BeginCoin(this);
        }

        public void Tick(float time)
        {
            owner.UpdateCoin(this, time);
        }

        public void Landed()
        {
            owner.LandCoin(this);
        }
    }

    [SerializeField] private TileEconomyManager economyManager;

    [Header("Counter")]
    [SerializeField] private TMP_Text coinText;
    [Tooltip("The coin icon next to the number. Coins fly here and it pulses when one lands.")]
    [SerializeField] private RectTransform counterTarget;
    [Tooltip("{0} = the coin amount.")]
    [SerializeField] private string textFormat = "{0}";
    [SerializeField] private float pulseStrength = 0.15f;
    [SerializeField] private float pulseDuration = 0.15f;

    [Header("Coin look")]
    [Tooltip("The coin face.")]
    [SerializeField] private Sprite coinSprite;
    [Tooltip("Optional: shown on the back while the coin spins. Empty = the face is mirrored.")]
    [SerializeField] private Sprite coinBackSprite;
    [SerializeField] private Color coinTint = Color.white;
    [Tooltip("Size of one coin in canvas units. 0 = the sprite's own size.")]
    [SerializeField] private Vector2 coinSize = new Vector2(80f, 80f);
    [Tooltip("The canvas object the coins live under. Put it above your panels.")]
    [SerializeField] private RectTransform coinParent;
    [Tooltip("Most coin images on screen at once.")]
    [SerializeField] private int maxCoins = 15;

    [Header("Shower")]
    [SerializeField] private RectTransform spawnPoint;
    [Tooltip("Size of the area (canvas units) coins burst into. Wide and short = a shower, small = a burst.")]
    [SerializeField] private Vector2 spawnSpread = new Vector2(600f, 200f);
    [Tooltip("Wait before the shower starts, e.g. to line up with the win panel.")]
    [SerializeField] private float showerDelay = 0.5f;
    [Tooltip("Delay between one coin and the next.")]
    [SerializeField] private float stagger = 0.05f;

    [Header("Burst")]
    [SerializeField] private float popDuration = 0.35f;
    [Tooltip("How high (canvas units) a coin jumps above its straight path.")]
    [SerializeField] private Vector2 arcHeightRange = new Vector2(80f, 200f);
    [Tooltip("Scale of a coin at the end of the burst.")]
    [SerializeField] private float popScale = 1f;
    [SerializeField] private Ease popPositionEase = Ease.OutQuad;
    [SerializeField] private Ease popScaleEase = Ease.OutBack;
    [Tooltip("How long a coin stays put before it flies to the counter.")]
    [SerializeField] private float holdDuration = 0.1f;

    [Header("Fly to counter")]
    [SerializeField] private float flyDuration = 0.6f;
    [SerializeField] private Ease flyEase = Ease.InQuad;
    [Tooltip("How much the flight bends sideways. 0 = a straight line.")]
    [SerializeField] private float curveAmount = 0.3f;
    [Tooltip("Scale of a coin when it reaches the counter.")]
    [SerializeField] private float landScale = 0.6f;

    [Header("Spin")]
    [Tooltip("Turns per second. Each coin gets a random value in this range.")]
    [SerializeField] private Vector2 spinSpeedRange = new Vector2(1.5f, 3f);

    [Header("Preview (Play mode)")]
    [Tooltip("Use the three-dot menu of this component > Preview Shower. It does not change the real coins.")]
    [SerializeField] private int previewCoinCount = 10;

    private readonly List<CoinSlot> pool = new List<CoinSlot>();
    private Currency coins;
    private CurrencyWatcher watcher;
    private int shownAmount = -1;
    private int pendingAmount;   // earned coins whose images have not landed yet
    private float recheckTimer;

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
            pool.Add(CreateCoin());
        }

        watcher = new CurrencyWatcher(coins, Refresh);
        coins.OnCurrencyEarn.RegisterEvent(PlayShower);
        Refresh();
    }

    // Safety net: if a change ever slips past the events, the number still catches up a moment later.
    private void Update()
    {
        if (coins == null)
        {
            return;
        }

        recheckTimer += Time.unscaledDeltaTime;
        if (recheckTimer < RecheckSeconds)
        {
            return;
        }

        recheckTimer = 0f;
        if (Mathf.Max(0, coins.Value - pendingAmount) != shownAmount)
        {
            Refresh();
        }
    }

    private void OnDisable()
    {
        DOTween.Kill(TweenId);
        for (int i = 0; i < pool.Count; i++)
        {
            pool[i].rect.gameObject.SetActive(false);
            pool[i].busy = false;
            pool[i].share = 0;
        }

        // No coin will land any more, so show the real amount.
        pendingAmount = 0;
        if (coins != null && coinText != null)
        {
            Refresh();
        }
    }

    private void OnDestroy()
    {
        if (watcher == null)
        {
            return;
        }

        watcher.Dispose();
        coins.OnCurrencyEarn.DeRegisterEvent(PlayShower);
    }

    // ---------------- Coin creation ----------------

    // Builds one coin image from the sprite. Anchors and pivot are centered so position and size act the same everywhere.
    private CoinSlot CreateCoin()
    {
        GameObject coinObject = new GameObject("Coin", typeof(RectTransform));
        coinObject.transform.SetParent(coinParent, false);

        Image image = coinObject.AddComponent<Image>();
        image.sprite = coinSprite;
        image.color = coinTint;
        image.raycastTarget = false;
        image.preserveAspect = true;

        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        if (coinSize.x > 0f && coinSize.y > 0f)
        {
            rect.sizeDelta = coinSize;
        }
        else
        {
            image.SetNativeSize();
        }

        coinObject.SetActive(false);

        CoinSlot slot = new CoinSlot();
        slot.image = image;
        slot.rect = rect;
        slot.owner = this;
        return slot;
    }

    // ---------------- Counter ----------------

    // Real amount minus the coins that are still flying.
    private void Refresh()
    {
        shownAmount = Mathf.Max(0, coins.Value - pendingAmount);
        coinText.text = string.Format(textFormat, shownAmount);
    }

    // Plays the coin shower. It runs automatically whenever coins are earned.
    public void PlayShower(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        LaunchShower(amount, true);
    }

    // Test the animation without earning anything.
    [ContextMenu("Preview Shower")]
    public void PreviewShower()
    {
        if (pool.Count == 0)
        {
            return;
        }

        LaunchShower(previewCoinCount, false);
    }

    private void LaunchShower(int amount, bool countUp)
    {
        List<CoinSlot> freeSlots = new List<CoinSlot>();
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].busy)
            {
                freeSlots.Add(pool[i]);
            }
        }

        // A few images are enough to show a big reward.
        int images = Mathf.Min(amount, freeSlots.Count);
        if (images <= 0)
        {
            return;
        }

        // Hold the earned amount back from the number until the coins land.
        if (countUp)
        {
            pendingAmount += amount;
            Refresh();
        }

        for (int i = 0; i < images; i++)
        {
            int share = 0;
            if (countUp)
            {
                share = amount / images;
                if (i < amount % images)
                {
                    share++;
                }
            }
            Launch(freeSlots[i], share, showerDelay + i * stagger);
        }
    }

    // ---------------- Coin animation ----------------

    // One tween drives the whole flight (burst, hold, fly), so the timing is easy to reason about.
    private void Launch(CoinSlot slot, int share, float delay)
    {
        slot.busy = true;
        slot.share = share;
        slot.flyStarted = false;
        slot.spinOffset = Random.value;
        slot.spinSpeed = Random.Range(spinSpeedRange.x, spinSpeedRange.y);
        slot.curveSide = Random.value < 0.5f ? -1f : 1f;
        slot.arcHeight = Random.Range(arcHeightRange.x, arcHeightRange.y);

        float totalTime = Mathf.Max(0.01f, popDuration) + Mathf.Max(0f, holdDuration) + Mathf.Max(0.01f, flyDuration);

        DOVirtual.Float(0f, totalTime, totalTime, slot.Tick)
            .SetEase(Ease.Linear)
            .SetDelay(delay)
            .SetUpdate(true)
            .SetId(TweenId)
            .SetLink(slot.rect.gameObject)
            .OnStart(slot.Begin)
            .OnComplete(slot.Landed);
    }

    // Runs when the delay is over: the coin appears at the spawn point and picks its burst target.
    private void BeginCoin(CoinSlot slot)
    {
        slot.rect.gameObject.SetActive(true);
        slot.rect.position = spawnPoint.position;
        slot.rect.localScale = Vector3.zero;

        Vector2 scatter = new Vector2(Random.Range(-0.5f, 0.5f) * spawnSpread.x, Random.Range(-0.5f, 0.5f) * spawnSpread.y);
        slot.burstStart = slot.rect.anchoredPosition;
        slot.burstEnd = slot.burstStart + scatter;
    }

    // time = seconds since the coin appeared.
    private void UpdateCoin(CoinSlot slot, float time)
    {
        float popTime = Mathf.Max(0.01f, popDuration);
        float holdTime = Mathf.Max(0f, holdDuration);
        float flyTime = Mathf.Max(0.01f, flyDuration);

        float uniformScale = popScale;

        if (time < popTime)
        {
            // Burst: jump out along an arc while growing.
            float progress = time / popTime;
            float movement = DOVirtual.EasedValue(0f, 1f, progress, popPositionEase);
            Vector2 position = Vector2.Lerp(slot.burstStart, slot.burstEnd, movement);
            position.y += Mathf.Sin(progress * Mathf.PI) * slot.arcHeight;
            slot.rect.anchoredPosition = position;
            uniformScale = DOVirtual.EasedValue(0f, popScale, progress, popScaleEase);
        }
        else if (time < popTime + holdTime)
        {
            slot.rect.anchoredPosition = slot.burstEnd;
        }
        else
        {
            // Fly: a curved path into the counter, shrinking a little on the way.
            if (!slot.flyStarted)
            {
                StartFly(slot);
            }

            float progress = Mathf.Clamp01((time - popTime - holdTime) / flyTime);
            float movement = DOVirtual.EasedValue(0f, 1f, progress, flyEase);
            Vector3 target = counterTarget.position;
            Vector3 inverse = Vector3.one - new Vector3(movement, movement, movement);

            Vector3 point = slot.flyStart * (1f - movement) * (1f - movement)
                + slot.flyControl * (2f * (1f - movement) * movement)
                + target * (movement * movement);
            slot.rect.position = point;
            uniformScale = Mathf.Lerp(popScale, landScale, movement);
        }

        // Spin: the width follows a cosine. With a back sprite it swaps faces instead of mirroring.
        float spinAngle = (slot.spinOffset + time * slot.spinSpeed) * Mathf.PI * 2f;
        float spinFactor = Mathf.Cos(spinAngle);

        if (coinBackSprite != null)
        {
            if (spinFactor >= 0f)
            {
                slot.image.sprite = coinSprite;
            }
            else
            {
                slot.image.sprite = coinBackSprite;
            }
            spinFactor = Mathf.Abs(spinFactor);
        }

        slot.rect.localScale = new Vector3(uniformScale * spinFactor, uniformScale, 1f);
    }

    // Remembers where the flight starts and bends the path sideways with a control point.
    private void StartFly(CoinSlot slot)
    {
        slot.flyStarted = true;
        slot.flyStart = slot.rect.position;

        Vector3 target = counterTarget.position;
        Vector3 middle = (slot.flyStart + target) * 0.5f;
        Vector3 direction = target - slot.flyStart;
        Vector3 sideways = new Vector3(-direction.y, direction.x, 0f).normalized;
        slot.flyControl = middle + sideways * (direction.magnitude * curveAmount * slot.curveSide);
    }

    // A coin reached the counter: the number goes up by its share and the icon pulses.
    private void LandCoin(CoinSlot slot)
    {
        slot.rect.gameObject.SetActive(false);
        slot.busy = false;

        pendingAmount = Mathf.Max(0, pendingAmount - slot.share);
        slot.share = 0;
        Refresh();

        counterTarget.DOKill(true);
        counterTarget.DOPunchScale(Vector3.one * pulseStrength, pulseDuration).SetUpdate(true);
    }
}