/*
 * TileGameManager
 * ---------------
 * The referee of the game. It holds the level list and the game state (Loading, Playing, Won, Lost) and
 * announces changes through stateChanged and levelStarted. Loading a level clears old tweens and the tray,
 * asks the spawner to build the level, then plays the fall-in animation and switches to Playing. After every
 * merge it checks for a win: nothing left on the board or in the tray and no merge still playing. Progress is
 * saved on the win. A full tray with no merge is a loss. RestartLevel replays the level (after a win it
 * steps the saved progress back first), NextLevel goes forward, and ForcePlayingState unlocks input if a
 * level ever gets stuck in Loading. Those three are Odin buttons, enabled in Play Mode only.
 */
using com.otm.manager;
using com.otm.utility;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum TileGameState { Loading, Playing, Won, Lost }

public class TileGameManager : Manager<TileGameManager>
{
    #region Public Variables

    public static Action<TileGameState> stateChanged;
    public static Action<int> levelStarted;

    #endregion

    #region Private Variables

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

    private TileGameState state = TileGameState.Loading;

    #endregion

    #region Properties

    public bool IsPlaying => state == TileGameState.Playing;

    // The level being played (or just finished). Handy for UI text.
    public int PlayedLevelNumber { get; private set; }

    private int SavedIndex
    {
        get { return LevelIndexHelper.Instance.GetLevelIndex(progressKey); }
        set { LevelIndexHelper.Instance.SetLevelIndex(progressKey, Mathf.Max(0, value)); }
    }

    #endregion

    #region Unity Callbacks

    // Starts listening for a full tray and for finished merges.
    private void OnEnable()
    {
        TileTrayManager.trayFull += OnTrayFull;
        TileMergeManager.mergeFinished += CheckWin;
    }

    // Stops listening.
    private void OnDisable()
    {
        TileTrayManager.trayFull -= OnTrayFull;
        TileMergeManager.mergeFinished -= CheckWin;
    }

    // Loads the first level.
    private void Start()
    {
        LoadCurrentLevel();
    }

    #endregion

    #region Level Loading

    // Clears everything, builds the saved level, and plays the fall-in before switching to Playing.
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

    // Replays the current level. After a win the saved progress steps back so the level just won reloads.
    [Button, EnableIf("@UnityEngine.Application.isPlaying")]
    public void RestartLevel()
    {
        if (state == TileGameState.Won) SavedIndex--;
        LoadCurrentLevel();
    }

    // Goes to the next level. After a win the saved index already points at it.
    [Button, EnableIf("@UnityEngine.Application.isPlaying")]
    public void NextLevel()
    {
        if (state != TileGameState.Won) SavedIndex++;
        LoadCurrentLevel();
    }

    // Testing helper: unlocks input if a level ever gets stuck in Loading.
    [Button, EnableIf("@UnityEngine.Application.isPlaying")]
    public void ForcePlayingState()
    {
        SetState(TileGameState.Playing);
    }

    // Sets the saved progress back to level 1 and saves it. The current level is not reloaded.
    public void ResetProgress()
    {
        PlayerPrefsHelper.CanSaveData = true;
        SavedIndex = 0;
        PlayerPrefsHelper.SaveData();
    }

    // Sets the progress back to level 1 and loads it right away (Play Mode).
    public void ResetToFirstLevel()
    {
        ResetProgress();
        LoadCurrentLevel();
    }

    #endregion

    #region Win, Lose And State

    // You win when nothing is left on the board or in the tray, and no merge is still playing.
    private void CheckWin()
    {
        if (state != TileGameState.Playing || boardManager.RemainingTiles > 0 || trayManager.TileCount > 0 || mergeManager.IsMerging) return;

        SavedIndex++;
        SetState(TileGameState.Won);
    }

    // A full tray with no merge is a loss.
    private void OnTrayFull()
    {
        if (state == TileGameState.Playing) SetState(TileGameState.Lost);
    }

    // Changes the state and tells everyone.
    private void SetState(TileGameState newState)
    {
        state = newState;
        stateChanged?.Invoke(newState);
    }

    #endregion
}