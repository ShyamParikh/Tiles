using System;
using System.Collections.Generic;
using com.otm.manager;
using UnityEngine;

public enum TileGameState
{
    Loading,
    Playing,
    Won,
    Lost
}

public class TileGameManager : Manager<TileGameManager>
{
    [Header("Levels")]
    [SerializeField] private List<TileLevelData> levels = new List<TileLevelData>();
    [Tooltip("On = after the last level, start again from the first. Off = keep replaying the last level.")]
    [SerializeField] private bool loopLevels = true;
    [Tooltip("Key used to save the level progress in LevelIndexHelper.")]
    [SerializeField] private string progressKey = "TileExplorer";

    [Header("Scene Managers & References (Assign in Inspector)")]
    [SerializeField] private TileVisualSpawner spawner;
    [SerializeField] private TileBoardManager boardManager;
    [SerializeField] private TileTrayManager trayManager;
    [SerializeField] private TileArrangementManager arrangementManager;
    [SerializeField] private TileMergeManager mergeManager;
    [SerializeField] private TileInputManager inputManager;

    [Header("UI Panels (Optional)")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Actions")]
    public static Action<TileGameState> stateChanged;
    public static Action<int> levelStarted;

    private TileGameState state = TileGameState.Loading;

    public TileGameState State => state;
    public bool IsPlaying => state == TileGameState.Playing;
    public int CurrentLevelNumber => LevelIndexHelper.Instance.GetLevelNumber(progressKey);

    private void OnEnable()
    {
        TileTrayManager.trayFull += OnTrayFull;
        TileMergeManager.mergeFinished += OnMergeFinished;
    }

    private void OnDisable()
    {
        TileTrayManager.trayFull -= OnTrayFull;
        TileMergeManager.mergeFinished -= OnMergeFinished;
    }

    private void Start()
    {
        RunStartupDiagnostics();
        LoadCurrentLevel();
    }

    private void RunStartupDiagnostics()
    {
        if (Time.timeScale <= 0f)
            Debug.LogError($"[GAME] CRITICAL: Time.timeScale is {Time.timeScale}! DOTween animations will not run.");

        if (levels == null || levels.Count == 0)
            Debug.LogError("[GAME] CRITICAL: 'Levels' list on TileGameManager is EMPTY!");

        if (spawner == null)
            Debug.LogError("[GAME] CRITICAL: 'Spawner' is not assigned in TileGameManager Inspector!");

        if (boardManager == null)
            Debug.LogError("[GAME] CRITICAL: 'Board Manager' is not assigned in TileGameManager Inspector!");

        if (trayManager == null)
            Debug.LogError("[GAME] CRITICAL: 'Tray Manager' is not assigned in TileGameManager Inspector!");

        if (arrangementManager == null)
            Debug.LogError("[GAME] CRITICAL: 'Arrangement Manager' is not assigned in TileGameManager Inspector!");

        if (mergeManager == null)
            Debug.LogError("[GAME] CRITICAL: 'Merge Manager' is not assigned in TileGameManager Inspector!");

        if (inputManager == null)
            Debug.LogError("[GAME] CRITICAL: 'Input Manager' is not assigned in TileGameManager Inspector!");
    }

    public void LoadCurrentLevel()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("[GAME] LoadCurrentLevel aborted: No levels assigned.");
            return;
        }
        if (spawner == null)
        {
            Debug.LogError("[GAME] LoadCurrentLevel aborted: No TileVisualSpawner assigned.");
            return;
        }

        SetState(TileGameState.Loading);

        if (arrangementManager != null) arrangementManager.KillAll();
        if (mergeManager != null) mergeManager.KillAll();
        if (trayManager != null) trayManager.ResetTray();

        TileLevelData levelToLoad = levels[GetLevelListIndex()];
        if (enableDebugLogs)
            Debug.Log($"[GAME] Loading level '{(levelToLoad != null ? levelToLoad.name : "NULL")}'...");

        spawner.Generate(levelToLoad);
        levelStarted?.Invoke(CurrentLevelNumber);

        if (arrangementManager != null)
            arrangementManager.PlayFallIntro(new List<Tile>(spawner.SpawnedTiles), OnIntroFinished);
        else
            OnIntroFinished();
    }

    private void OnIntroFinished()
    {
        if (enableDebugLogs) Debug.Log("[GAME] Intro finished -> Switching state to PLAYING (Clicks enabled).");
        SetState(TileGameState.Playing);
    }

    public void ForcePlayingState()
    {
        Debug.LogWarning("[GAME] ForcePlayingState called manually from Inspector.");
        SetState(TileGameState.Playing);
    }

    public void NextLevel()
    {
        LoadCurrentLevel();
    }

    public void RestartLevel()
    {
        if (enableDebugLogs) Debug.Log("[GAME] RestartLevel triggered.");
        LoadCurrentLevel();
    }

    private int GetLevelListIndex()
    {
        int savedIndex = LevelIndexHelper.Instance.GetLevelIndex(progressKey);

        if (loopLevels) return savedIndex % levels.Count;
        return Mathf.Min(savedIndex, levels.Count - 1);
    }

    private void OnMergeFinished()
    {
        CheckWin();
    }

    private void CheckWin()
    {
        if (state != TileGameState.Playing) return;

        if (boardManager == null || trayManager == null) return;
        if (boardManager.RemainingTiles > 0) return;
        if (trayManager.TileCount > 0) return;
        if (mergeManager != null && mergeManager.IsMerging) return;

        int savedIndex = LevelIndexHelper.Instance.GetLevelIndex(progressKey);
        LevelIndexHelper.Instance.SetLevelIndex(progressKey, savedIndex + 1);

        SetState(TileGameState.Won);
    }

    private void OnTrayFull()
    {
        if (state != TileGameState.Playing) return;

        SetState(TileGameState.Lost);
    }

    private void SetState(TileGameState newState)
    {
        state = newState;

        if (enableDebugLogs) Debug.Log($"[GAME] State -> {newState}");

        if (winPanel != null) winPanel.SetActive(newState == TileGameState.Won);
        if (losePanel != null) losePanel.SetActive(newState == TileGameState.Lost);

        stateChanged?.Invoke(newState);
    }
}