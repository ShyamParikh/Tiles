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
    [Tooltip("On = after the last level start again from the first. Off = keep replaying the last level.")]
    [SerializeField] private bool loopLevels = true;
    [Tooltip("Key used to save the level progress in LevelIndexHelper.")]
    [SerializeField] private string progressKey = "TileExplorer";

    [Header("Scene Managers & References (Assign in Inspector)")]
    [SerializeField] private TileVisualSpawner spawner;
    [SerializeField] private TileBoardManager boardManager;
    [SerializeField] private TileTrayManager trayManager;
    [SerializeField] private TileArrangementManager arrangementManager;
    [SerializeField] private TileMergeManager mergeManager;

    public static Action<TileGameState> stateChanged;
    public static Action<int> levelStarted;

    private TileGameState state = TileGameState.Loading;

    public TileGameState State => state;
    public bool IsPlaying => state == TileGameState.Playing;
    public int CurrentLevelNumber => LevelIndexHelper.Instance.GetLevelNumber(progressKey);

    /// <summary>The number of the level being played (or just finished), unaffected by progress saved on a win.</summary>
    public int PlayedLevelNumber { get; private set; }

    private void OnEnable()
    {
        TileTrayManager.trayFull += OnTrayFull;
        TileMergeManager.mergeFinished += CheckWin;
    }

    private void OnDisable()
    {
        TileTrayManager.trayFull -= OnTrayFull;
        TileMergeManager.mergeFinished -= CheckWin;
    }

    private void Start()
    {
        if (Time.timeScale <= 0f) Debug.LogError("[GAME] Time.timeScale is 0: DOTween animations will not run.", this);
        LoadCurrentLevel();
    }

    public void LoadCurrentLevel()
    {
        if (levels.Count == 0 || spawner == null || boardManager == null || trayManager == null || arrangementManager == null || mergeManager == null)
        {
            Debug.LogError("[GAME] The Levels list or a scene reference on TileGameManager is missing!", this);
            return;
        }

        SetState(TileGameState.Loading);
        arrangementManager.KillAll();
        mergeManager.KillAll();
        trayManager.ResetTray();

        PlayedLevelNumber = CurrentLevelNumber;
        spawner.Generate(levels[LevelListIndex()]);
        levelStarted?.Invoke(PlayedLevelNumber);
        arrangementManager.PlayFallIntro(new List<Tile>(spawner.SpawnedTiles), () => SetState(TileGameState.Playing));
    }

    public void ForcePlayingState() { SetState(TileGameState.Playing); }

    /// <summary>Replays the current level. After a win, the saved progress steps back so the level just won reloads.</summary>
    public void RestartLevel()
    {
        if (state == TileGameState.Won) ShiftSavedLevel(-1);
        LoadCurrentLevel();
    }

    /// <summary>Goes to the next level. After a win the progress is already saved, so it only loads.</summary>
    public void NextLevel()
    {
        if (state != TileGameState.Won) ShiftSavedLevel(1);
        LoadCurrentLevel();
    }

    private void ShiftSavedLevel(int amount)
    {
        int saved = LevelIndexHelper.Instance.GetLevelIndex(progressKey);
        LevelIndexHelper.Instance.SetLevelIndex(progressKey, Mathf.Max(0, saved + amount));
    }

    private int LevelListIndex()
    {
        int saved = LevelIndexHelper.Instance.GetLevelIndex(progressKey);
        return loopLevels ? saved % levels.Count : Mathf.Min(saved, levels.Count - 1);
    }

    private void CheckWin()
    {
        if (state != TileGameState.Playing || boardManager.RemainingTiles > 0 || trayManager.TileCount > 0 || mergeManager.IsMerging) return;

        ShiftSavedLevel(1);
        SetState(TileGameState.Won);
    }

    private void OnTrayFull()
    {
        if (state == TileGameState.Playing) SetState(TileGameState.Lost);
    }

    private void SetState(TileGameState newState)
    {
        state = newState;
        stateChanged?.Invoke(newState);
    }
}