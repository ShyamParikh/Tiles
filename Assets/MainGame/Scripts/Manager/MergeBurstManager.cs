using System.Collections;
using System.Collections.Generic;
using com.otm.manager;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Runs the extra merges requested by MergeBurstTile: waits for the triggering merge to finish, then gathers
/// random complete sets from the board and merges them one after another.
/// </summary>
public class MergeBurstManager : Manager<MergeBurstManager>
{
    private const string TweenId = "MergeBurst";

    [Header("Scene References (Assign in Inspector)")]
    [SerializeField] private TileGameManager gameManager;
    [SerializeField] private TileBoardManager boardManager;
    [SerializeField] private TileTrayManager trayManager;
    [SerializeField] private TileMergeManager mergeManager;
    [Tooltip("Optional: the burst waits while a booster is running.")]
    [SerializeField] private BoosterManager boosterManager;

    [Header("Merge Burst")]
    [Tooltip("On = a burst set that contains another burst tile triggers its burst too.")]
    [SerializeField] private bool allowChainBursts = true;
    [SerializeField] private float startDelay = 0.15f;
    [SerializeField] private float delayBetweenSets = 0.2f;
    [Tooltip("Safety: give up waiting for a merge animation after this many seconds.")]
    [SerializeField] private float maxWaitForMerge = 3f;

    [Header("Gather Animation")]
    [SerializeField] private float gatherDuration = 0.35f;
    [SerializeField] private Ease gatherEase = Ease.InOutQuad;
    [Tooltip("World-space distance between the 3 tiles when they line up.")]
    [SerializeField] private float gatherSpacing = 1.2f;
    [Tooltip("Shifts the meeting point (world space), e.g. toward the camera.")]
    [SerializeField] private Vector3 gatherWorldOffset = Vector3.zero;

    private readonly System.Random rng = new System.Random();
    private bool running;
    private int pending;
    private int arrived;

    private bool Busy => mergeManager.IsMerging || (boosterManager != null && boosterManager.IsBusy);

    private void OnEnable()
    {
        MergeBurstTile.burstRequested += OnBurstRequested;
        TileGameManager.levelStarted += OnLevelStarted;
        TileGameManager.stateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        MergeBurstTile.burstRequested -= OnBurstRequested;
        TileGameManager.levelStarted -= OnLevelStarted;
        TileGameManager.stateChanged -= OnStateChanged;
    }

    private void OnBurstRequested(int sets)
    {
        if (running && !allowChainBursts) return;

        pending += sets;
        if (running) return;

        running = true;
        trayManager.SetExternalLock(true);
        StartCoroutine(Run());
    }

    private void OnLevelStarted(int levelNumber) { Stop(); }
    private void OnStateChanged(TileGameState state) { if (state != TileGameState.Playing) Stop(); }

    private void Stop()
    {
        StopAllCoroutines();
        DOTween.Kill(TweenId);
        pending = 0;

        if (!running) return;
        running = false;
        trayManager.SetExternalLock(false);
    }

    private IEnumerator Run()
    {
        yield return null;   // let TileMergeManager register the merge that triggered us
        while (Busy) yield return null;
        yield return new WaitForSeconds(startDelay);

        while (pending > 0 && gameManager.IsPlaying)
        {
            while (Busy) yield return null;
            pending--;

            List<Tile> set = PickSet();
            if (set == null) break;

            arrived = 0;
            Gather(set);
            for (float waited = 0f; arrived < set.Count && waited < gatherDuration + 2f; waited += Time.deltaTime) yield return null;
            if (!set.TrueForAll(t => t != null)) continue;

            foreach (Tile tile in set)
            {
                TileTrayManager.tilePickedUp?.Invoke(tile);
                tile.PrepareForTray();
            }
            TileTrayManager.startMerge?.Invoke(set);

            for (float waited = 0f; set.Exists(t => t != null) && waited < maxWaitForMerge; waited += Time.deltaTime) yield return null;
            yield return new WaitForSeconds(delayBetweenSets);
        }

        Stop();
    }

    /// <summary>A random face that still has a full set on the board, and 3 random tiles of it.</summary>
    private List<Tile> PickSet()
    {
        List<Tile> board = new List<Tile>(boardManager.Tiles);
        List<Vector2Int> runs = TileRules.FaceRuns(board);
        runs.RemoveAll(run => run.y < TileRules.SetSize);
        if (runs.Count == 0) return null;

        Vector2Int chosen = runs[rng.Next(runs.Count)];
        List<Tile> group = board.GetRange(chosen.x, chosen.y);
        TileRules.Shuffle(group, rng);
        return group.GetRange(0, TileRules.SetSize);
    }

    /// <summary>Lines the 3 tiles up around their common center so the normal merge animation looks right.</summary>
    private void Gather(List<Tile> set)
    {
        set.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        Vector3 center = Vector3.zero;
        foreach (Tile tile in set) center += tile.transform.position;
        center = center / set.Count + gatherWorldOffset;

        for (int i = 0; i < set.Count; i++)
        {
            Tile tile = set[i];
            tile.transform.DOKill();
            tile.DisableInteraction();

            tile.transform.DOMove(center + Vector3.right * ((i - 1) * gatherSpacing), gatherDuration)
                .SetEase(gatherEase)
                .Bind(TweenId, tile.gameObject)
                .OnComplete(() => arrived++);
        }
    }
}