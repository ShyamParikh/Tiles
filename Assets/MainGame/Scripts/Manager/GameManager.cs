using System;
using System.Collections.Generic;
using com.otm.manager;
using Sirenix.OdinInspector;
using UnityEngine;

public enum TileGameState { Loading, Playing, Won, Lost }

// The referee: loads levels, tracks the game state and decides when you win or lose.
public class TileGameManager : Manager<TileGameManager>
{
    [Header("Levels")]
    [SerializeField] private List<TileLevelData> levels = new List<TileLevelData>();
    [Tooltip("On = start again from level 1 after the last one. Off = keep replaying the last level.")]
    [SerializeField] private bool loopLevels = true;
    [Tooltip("Where the level progress is saved (LevelIndexHelper).")]
    [SerializeField] private string progressKey = "TileExplorer";

    [Header("Scene references")]
    [SerializeField] private TileVisualSpawner spawner;
    [SerializeField] private TileBoardManager boardManager;
    [SerializeField] private TileTrayManager trayManager;
    [SerializeField] private TileArrangementManager arrangementManager;
    [SerializeField] private TileMergeManager mergeManager;

    public static Action<TileGameState> stateChanged;
    public static Action<int> levelStarted;

    private TileGameState state = TileGameState.Loading;

    public bool IsPlaying => state == TileGameState.Playing;
    // The level being played (or just finished). Handy for UI text.
    public int PlayedLevelNumber { get; private set; }

    private int SavedIndex
    {
        get { return LevelIndexHelper.Instance.GetLevelIndex(progressKey); }
        set { LevelIndexHelper.Instance.SetLevelIndex(progressKey, Mathf.Max(0, value)); }
    }

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
        LoadCurrentLevel();
    }

    public void LoadCurrentLevel()
    {
        SetState(TileGameState.Loading);
        arrangementManager.KillAll();
        mergeManager.KillAll();
        trayManager.ResetTray();

        PlayedLevelNumber = LevelIndexHelper.Instance.GetLevelNumber(progressKey);
        int index = loopLevels ? SavedIndex % levels.Count : Mathf.Min(SavedIndex, levels.Count - 1);
        spawner.Generate(levels[index]);

        levelStarted?.Invoke(PlayedLevelNumber);
        arrangementManager.PlayFallIntro(new List<Tile>(spawner.SpawnedTiles), () => SetState(TileGameState.Playing));
    }

    [Button, EnableIf("@UnityEngine.Application.isPlaying")]
    public void RestartLevel()
    {
        // A win already saved the progress, so step back to replay the level we just beat.
        if (state == TileGameState.Won) SavedIndex--;
        LoadCurrentLevel();
    }

    [Button, EnableIf("@UnityEngine.Application.isPlaying")]
    public void NextLevel()
    {
        // After a win the saved index already points at the next level.
        if (state != TileGameState.Won) SavedIndex++;
        LoadCurrentLevel();
    }

    // Testing helper: unlocks input if a level ever gets stuck in Loading.
    [Button, EnableIf("@UnityEngine.Application.isPlaying")]
    public void ForcePlayingState()
    {
        SetState(TileGameState.Playing);
    }

    // You win when nothing is left on the board or in the tray, and no merge is still playing.
    private void CheckWin()
    {
        if (state != TileGameState.Playing || boardManager.RemainingTiles > 0 || trayManager.TileCount > 0 || mergeManager.IsMerging) return;

        SavedIndex++;
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