using System.Collections;
using System.Collections.Generic;
using com.otm.manager;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Handles special tile effects that need the whole board. Right now: MergeBurstTile.
/// When a set that contains a MergeBurstTile merges, this waits for that merge to finish, then picks random
/// complete sets of 3 from the board, gathers each set together and plays the normal merge animation on it.
/// One merge = one burst, even if several burst tiles are in it (the biggest count is used).
/// </summary>
public class SpecialTileManager : Manager<SpecialTileManager>
{
    private const string TweenId = "SpecialTile";

    [Header("Scene References (Assign in Inspector)")]
    [SerializeField] private TileGameManager gameManager;
    [SerializeField] private TileVisualSpawner spawner;
    [SerializeField] private TileTrayManager trayManager;
    [SerializeField] private TileMergeManager mergeManager;
    [Tooltip("Optional: the burst waits while a booster is running.")]
    [SerializeField] private BoosterManager boosterManager;

    [Header("Merge Burst")]
    [Tooltip("On = a burst set that contains another MergeBurstTile triggers its burst too.")]
    [SerializeField] private bool allowChainBursts = true;
    [Tooltip("Pause after the triggering merge finishes, before the first burst set starts.")]
    [SerializeField] private float startDelay = 0.15f;
    [Tooltip("Pause between one burst set and the next.")]
    [SerializeField] private float delayBetweenSets = 0.2f;

    [Header("Burst Gather Animation")]
    [SerializeField] private float gatherDuration = 0.35f;
    [SerializeField] private Ease gatherEase = Ease.InOutQuad;
    [Tooltip("World-space distance between the 3 tiles when they line up for the merge.")]
    [SerializeField] private float gatherSpacing = 1.2f;
    [Tooltip("Shifts the meeting point (world space). Use it to push the tiles toward the camera so they don't clip other tiles.")]
    [SerializeField] private Vector3 gatherWorldOffset = Vector3.zero;
    [Tooltip("Safety: give up waiting for a merge animation after this many seconds.")]
    [SerializeField] private float maxWaitForMerge = 3f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private readonly List<Tile> boardTiles = new List<Tile>();

    private bool running;
    private int pendingSets;

    public bool IsBusy => running;

    private void OnEnable()
    {
        TileTrayManager.startMerge += OnStartMerge;
        TileGameManager.levelStarted += OnLevelStarted;
        TileGameManager.stateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        TileTrayManager.startMerge -= OnStartMerge;
        TileGameManager.levelStarted -= OnLevelStarted;
        TileGameManager.stateChanged -= OnStateChanged;
    }

    private void Start()
    {
        if (gameManager == null) Debug.LogError("[SPECIAL] CRITICAL: 'Game Manager' is not assigned!", this);
        if (spawner == null) Debug.LogError("[SPECIAL] CRITICAL: 'Spawner' is not assigned!", this);
        if (trayManager == null) Debug.LogError("[SPECIAL] CRITICAL: 'Tray Manager' is not assigned!", this);
        if (mergeManager == null) Debug.LogError("[SPECIAL] CRITICAL: 'Merge Manager' is not assigned!", this);
    }

    // ---------------- Triggers ----------------

    private void OnStartMerge(List<Tile> tiles)
    {
        if (tiles == null) return;

        int sets = 0;
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] is MergeBurstTile burstTile)
                sets = Mathf.Max(sets, burstTile.BurstSetCount);
        }

        if (sets <= 0) return;
        if (running && !allowChainBursts) return;

        pendingSets += sets;
        if (enableDebugLogs) Debug.Log($"[SPECIAL] Merge burst requested: +{sets} set(s) (pending {pendingSets}).");

        if (!running)
        {
            running = true;
            trayManager.SetExternalLock(true);
            StartCoroutine(RunBursts());
        }
    }

    private void OnLevelStarted(int levelNumber)
    {
        CancelBursts();
    }

    private void OnStateChanged(TileGameState state)
    {
        if (state != TileGameState.Playing) CancelBursts();
    }

    private void CancelBursts()
    {
        pendingSets = 0;
        if (!running) return;

        StopAllCoroutines();
        DOTween.Kill(TweenId);
        running = false;

        if (trayManager != null) trayManager.SetExternalLock(false);
    }

    private void FinishBursts()
    {
        pendingSets = 0;
        running = false;

        if (trayManager != null) trayManager.SetExternalLock(false);
        if (enableDebugLogs) Debug.Log("[SPECIAL] Merge burst finished.");
    }

    // ---------------- Burst routine ----------------

    private bool IsWaitingForOthers()
    {
        if (mergeManager != null && mergeManager.IsMerging) return true;
        if (boosterManager != null && boosterManager.IsBusy) return true;
        return false;
    }

    private IEnumerator RunBursts()
    {
        // Let TileMergeManager register the merge that triggered us, then wait for it to finish.
        yield return null;
        while (IsWaitingForOthers()) yield return null;
        yield return new WaitForSeconds(startDelay);

        while (pendingSets > 0)
        {
            if (gameManager == null || !gameManager.IsPlaying) break;

            while (IsWaitingForOthers()) yield return null;

            pendingSets--;

            List<Tile> set = PickRandomSet();
            if (set == null)
            {
                if (enableDebugLogs) Debug.Log("[SPECIAL] No complete set of 3 left on the board for the burst.");
                break;
            }

            // 1. Gather the 3 tiles in a row at their common center
            set.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

            Vector3 center = Vector3.zero;
            for (int i = 0; i < set.Count; i++) center += set[i].transform.position;
            center = center / set.Count + gatherWorldOffset;

            int arrived = 0;
            for (int i = 0; i < set.Count; i++)
            {
                Tile tile = set[i];
                tile.transform.DOKill();
                tile.DisableInteraction();

                Vector3 target = center + Vector3.right * ((i - 1) * gatherSpacing);

                tile.transform
                    .DOMove(target, gatherDuration)
                    .SetEase(gatherEase)
                    .SetId(TweenId)
                    .SetLink(tile.gameObject)
                    .OnComplete(() => arrived++);
            }

            float waited = 0f;
            float gatherTimeout = gatherDuration + 2f;
            while (arrived < set.Count && waited < gatherTimeout)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            if (!AllAlive(set)) continue;

            // 2. Take them off the board (frees tiles they were covering), then play the normal merge animation
            for (int i = 0; i < set.Count; i++)
            {
                TileTrayManager.tilePickedUp?.Invoke(set[i]);
                set[i].SetBlocked(false);
                set[i].DisableShadows();
            }

            TileTrayManager.startMerge?.Invoke(set);

            // 3. Wait until the merge animation has removed the tiles
            waited = 0f;
            while (AnyAlive(set) && waited < maxWaitForMerge)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(delayBetweenSets);
        }

        FinishBursts();
    }

    /// <summary>Picks a random face that still has 3 or more tiles on the board, then 3 random tiles of it.</summary>
    private List<Tile> PickRandomSet()
    {
        boardTiles.Clear();

        IReadOnlyList<Tile> spawned = spawner.SpawnedTiles;
        for (int i = 0; i < spawned.Count; i++)
        {
            Tile tile = spawned[i];
            if (tile != null && tile.Slot == null) boardTiles.Add(tile);
        }

        Dictionary<int, List<Tile>> byFace = new Dictionary<int, List<Tile>>();
        for (int i = 0; i < boardTiles.Count; i++)
        {
            Tile tile = boardTiles[i];
            if (!byFace.TryGetValue(tile.FaceId, out List<Tile> list))
            {
                list = new List<Tile>();
                byFace.Add(tile.FaceId, list);
            }
            list.Add(tile);
        }

        int perSet = TileMergeData.TilesPerMerge;
        List<List<Tile>> candidates = new List<List<Tile>>();
        foreach (KeyValuePair<int, List<Tile>> pair in byFace)
        {
            if (pair.Value.Count >= perSet) candidates.Add(pair.Value);
        }

        if (candidates.Count == 0) return null;

        List<Tile> group = candidates[Random.Range(0, candidates.Count)];

        // Partial Fisher-Yates: move 3 random tiles to the front
        for (int i = 0; i < perSet; i++)
        {
            int j = Random.Range(i, group.Count);
            Tile temp = group[i];
            group[i] = group[j];
            group[j] = temp;
        }

        return group.GetRange(0, perSet);
    }

    private static bool AllAlive(List<Tile> tiles)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] == null) return false;
        }
        return true;
    }

    private static bool AnyAlive(List<Tile> tiles)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] != null) return true;
        }
        return false;
    }
}