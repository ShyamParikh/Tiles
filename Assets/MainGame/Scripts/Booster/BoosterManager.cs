using System;
using System.Collections.Generic;
using com.otm.manager;
using UnityEngine;

/// <summary>
/// Single entry point for all boosters. Makes sure only one booster runs at a time,
/// locks tile input while it runs, and cancels it if the level restarts or ends.
/// </summary>
public class BoosterManager : Manager<BoosterManager>
{
    [Header("Scene References (Assign in Inspector)")]
    [SerializeField] private TileGameManager gameManager;
    [SerializeField] private TileVisualSpawner spawner;
    [SerializeField] private TileBoardManager boardManager;
    [SerializeField] private TileTrayManager trayManager;
    [SerializeField] private TileArrangementManager arrangementManager;
    [Tooltip("Initialises the booster currencies before the boosters read them.")]
    [SerializeField] private TileEconomyManager economyManager;

    [Header("Boosters (Assign in Inspector)")]
    [SerializeField] private List<BoosterBase> boosters = new List<BoosterBase>();

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Actions")]
    public static Action<BoosterBase> boosterUsed;
    [Tooltip("Fired when a booster is pressed with 0 stock (open your shop / rewarded ad from this).")]
    public static Action<BoosterBase> boosterEmpty;
    public static Action<BoosterBase> boosterUnavailable;
    public static Action<bool> busyChanged;

    private bool isBusy;
    private int executionToken;

    public bool IsBusy => isBusy;

    public TileGameManager Game => gameManager;
    public TileVisualSpawner Spawner => spawner;
    public TileBoardManager Board => boardManager;
    public TileTrayManager Tray => trayManager;
    public TileArrangementManager Arrangement => arrangementManager;

    private void OnEnable()
    {
        TileGameManager.levelStarted += OnLevelStarted;
        TileGameManager.stateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        TileGameManager.levelStarted -= OnLevelStarted;
        TileGameManager.stateChanged -= OnStateChanged;
    }

    private void Start()
    {
        if (gameManager == null) Debug.LogError("[BOOSTER] CRITICAL: 'Game Manager' is not assigned!", this);
        if (spawner == null) Debug.LogError("[BOOSTER] CRITICAL: 'Spawner' is not assigned!", this);
        if (boardManager == null) Debug.LogError("[BOOSTER] CRITICAL: 'Board Manager' is not assigned!", this);
        if (trayManager == null) Debug.LogError("[BOOSTER] CRITICAL: 'Tray Manager' is not assigned!", this);
        if (arrangementManager == null) Debug.LogError("[BOOSTER] CRITICAL: 'Arrangement Manager' is not assigned!", this);

        if (economyManager != null)
            economyManager.EnsureInitialized();
        else
            Debug.LogWarning("[BOOSTER] 'Economy Manager' is not assigned. Make sure the booster Currencies are initialised (Currency.Init) before this point, otherwise the count labels will not update.", this);

        for (int i = 0; i < boosters.Count; i++)
        {
            if (boosters[i] != null) boosters[i].Initialize(this);
        }
    }

    // ---------------- Public API ----------------

    public BoosterBase GetBooster(BoosterType type)
    {
        for (int i = 0; i < boosters.Count; i++)
        {
            if (boosters[i] != null && boosters[i].Type == type) return boosters[i];
        }
        return null;
    }

    public bool TryUse(BoosterType type)
    {
        return TryUse(GetBooster(type));
    }

    // Convenient for UI buttons wired through the Inspector.
    public void UseShuffle() { TryUse(BoosterType.Shuffle); }
    public void UseUndo() { TryUse(BoosterType.Undo); }
    public void UseAutoComplete() { TryUse(BoosterType.AutoComplete); }

    public bool TryUse(BoosterBase booster)
    {
        if (booster == null || isBusy) return false;
        if (gameManager == null || !gameManager.IsPlaying) return false;
        if (trayManager == null || trayManager.IsLocked) return false;

        if (!booster.HasStock)
        {
            if (enableDebugLogs) Debug.Log($"[BOOSTER] {booster.Type} has no stock.");
            boosterEmpty?.Invoke(booster);
            return false;
        }

        if (!booster.CanExecute())
        {
            if (enableDebugLogs) Debug.Log($"[BOOSTER] {booster.Type} cannot be used right now.");
            boosterUnavailable?.Invoke(booster);
            return false;
        }

        SetBusy(true);
        int token = ++executionToken;

        if (enableDebugLogs) Debug.Log($"[BOOSTER] Using {booster.Type}.");

        booster.Execute(success =>
        {
            if (token != executionToken) return; // cancelled (level restarted / ended)

            if (success)
            {
                booster.Consume();
                boosterUsed?.Invoke(booster);
            }

            SetBusy(false);
        });

        return true;
    }

    /// <summary>Fills result with every tile that is currently lying on the board (not in the tray).</summary>
    public void CollectBoardTiles(List<Tile> result)
    {
        result.Clear();
        if (spawner == null) return;

        IReadOnlyList<Tile> spawned = spawner.SpawnedTiles;
        for (int i = 0; i < spawned.Count; i++)
        {
            Tile tile = spawned[i];
            if (tile != null && tile.Slot == null) result.Add(tile);
        }
    }

    // ---------------- Internals ----------------

    private void SetBusy(bool busy)
    {
        isBusy = busy;
        if (trayManager != null) trayManager.SetExternalLock(busy);
        busyChanged?.Invoke(busy);
    }

    private void CancelRunningBooster()
    {
        executionToken++;
        if (isBusy) SetBusy(false);
    }

    private void OnLevelStarted(int levelNumber)
    {
        CancelRunningBooster();
        for (int i = 0; i < boosters.Count; i++)
        {
            if (boosters[i] != null) boosters[i].ResetForNewLevel();
        }
    }

    private void OnStateChanged(TileGameState state)
    {
        if (state != TileGameState.Playing) CancelRunningBooster();
    }
}