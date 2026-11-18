/*
 * MergeBurstManager
 * -----------------
 * Runs the extra merges that a MergeBurstTile asks for. When a burst is requested it locks the tray, waits for
 * the merge that triggered it (and any running booster) to finish, then repeats for every requested set: pick a
 * random face that still has a full set on the board, take three random tiles of it, line them up in a row at
 * their common center, take them off the board, and reuse the normal merge animation by raising
 * TileTrayManager.startMerge. Tiles inside a burst that are burst tiles themselves can trigger more (Allow Chain
 * Bursts). A level restart or the end of the level cancels everything. The loop itself is a coroutine, kept at
 * the bottom of the file.
 */
using System.Collections;
using System.Collections.Generic;
using com.otm.manager;
using DG.Tweening;
using UnityEngine;

public class MergeBurstManager : Manager<MergeBurstManager>
{
    #region Private Variables

    private const string TweenId = "MergeBurst";

    [Header("Scene references")]
    [SerializeField] private TileGameManager gameManager;
    [SerializeField] private TileBoardManager boardManager;
    [SerializeField] private TileTrayManager trayManager;
    [SerializeField] private TileMergeManager mergeManager;
    [Tooltip("Optional: the burst waits while a booster is running.")]
    [SerializeField] private BoosterManager boosterManager;

    [Header("Burst")]
    [Tooltip("On = a burst tile that gets merged inside a burst triggers its own burst too.")]
    [SerializeField] private bool allowChainBursts = true;
    [SerializeField] private float startDelay = 0.15f;
    [SerializeField] private float delayBetweenSets = 0.2f;
    [Tooltip("Gives up waiting for a merge animation after this long.")]
    [SerializeField] private float maxWaitForMerge = 3f;

    [Header("Lining the tiles up")]
    [SerializeField] private float gatherDuration = 0.35f;
    [SerializeField] private Ease gatherEase = Ease.InOutQuad;
    [Tooltip("World distance between the three tiles when they line up.")]
    [SerializeField] private float gatherSpacing = 1.2f;
    [Tooltip("Moves the meeting point, e.g. toward the camera so tiles don't clip others.")]
    [SerializeField] private Vector3 gatherWorldOffset = Vector3.zero;

    private readonly System.Random rng = new System.Random();
    private bool running;
    private int pending;
    private int arrived;

    #endregion

    #region Properties

    // True while a merge animation or a booster is still running.
    private bool Busy => mergeManager.IsMerging || (boosterManager != null && boosterManager.IsBusy);

    #endregion

    #region Unity Callbacks

    // Starts listening for burst requests and for level changes.
    private void OnEnable()
    {
        MergeBurstTile.burstRequested += OnBurstRequested;
        TileGameManager.levelStarted += OnLevelStarted;
        TileGameManager.stateChanged += OnStateChanged;
    }

    // Stops listening.
    private void OnDisable()
    {
        MergeBurstTile.burstRequested -= OnBurstRequested;
        TileGameManager.levelStarted -= OnLevelStarted;
        TileGameManager.stateChanged -= OnStateChanged;
    }

    #endregion

    #region Starting And Stopping

    // A burst tile asked for extra sets: queue them and start the routine if it isn't running.
    private void OnBurstRequested(int sets)
    {
        if (running && !allowChainBursts) return;

        pending += sets;
        if (running) return;

        running = true;
        trayManager.SetExternalLock(true);
        StartCoroutine(Run());
    }

    // A new level cancels any burst.
    private void OnLevelStarted(int levelNumber) { Stop(); }

    // Leaving the Playing state cancels any burst.
    private void OnStateChanged(TileGameState state) { if (state != TileGameState.Playing) Stop(); }

    // Cancels the routine and animations and unlocks the tray.
    private void Stop()
    {
        StopAllCoroutines();
        DOTween.Kill(TweenId);
        pending = 0;

        if (!running) return;
        running = false;
        trayManager.SetExternalLock(false);
    }

    #endregion

    #region Picking And Gathering

    // A random face that still has a full set on the board, and three random tiles of it.
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

    // Moves the three tiles into a row around their common center, so the merge animation looks right.
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

    #endregion

    #region Coroutines

    // The burst loop: wait for the trigger merge, then merge one random set after another.
    private IEnumerator Run()
    {
        yield return null;   // give TileMergeManager a frame to register the merge that triggered us
        while (Busy) yield return null;
        yield return new WaitForSeconds(startDelay);

        while (pending > 0 && gameManager.IsPlaying)
        {
            while (Busy) yield return null;
            pending--;

            List<Tile> set = PickSet();
            if (set == null) break;

            // Line the three up, and wait until they have all arrived (or time out).
            arrived = 0;
            Gather(set);
            for (float waited = 0f; arrived < set.Count && waited < gatherDuration + 2f; waited += Time.deltaTime) yield return null;
            if (set.Exists(t => t == null)) continue;

            // Take them off the board, then reuse the normal merge animation.
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

    #endregion
}