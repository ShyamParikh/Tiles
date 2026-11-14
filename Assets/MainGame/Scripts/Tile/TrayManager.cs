using System;
using System.Collections.Generic;
using com.otm.manager;
using DG.Tweening;
using UnityEngine;

public class TileTrayManager : Manager<TileTrayManager>
{
    [Header("Scene References (Assign in Inspector)")]
    [Tooltip("REQUIRED: Drag your TileGameManager object here.")]
    [SerializeField] private TileGameManager gameManager;
    [Tooltip("REQUIRED: Drag your TileArrangementManager object here.")]
    [SerializeField] private TileArrangementManager arrangementManager;
    [Tooltip("REQUIRED: Drag all TraySlot objects in order, left to right.")]
    [SerializeField] private List<TraySlot> slots = new List<TraySlot>();

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Actions")]
    public static Action<Tile> tilePickedUp;
    public static Action<List<Tile>> startMerge;
    public static Action trayFull;

    private readonly List<Tile> trayTiles = new List<Tile>();
    private readonly Dictionary<int, TileMergeData> mergeDataByFace = new Dictionary<int, TileMergeData>();

    private bool isLocked;

    public int TileCount => trayTiles.Count;
    public int SlotCount => slots.Count;

    private void OnEnable()
    {
        TileInputManager.tileClicked += OnTileClicked;
    }

    private void OnDisable()
    {
        TileInputManager.tileClicked -= OnTileClicked;
    }

    private void Start()
    {
        if (gameManager == null)
            Debug.LogError("[TRAY] CRITICAL: 'Game Manager' is not assigned in TileTrayManager Inspector!", this);

        if (arrangementManager == null)
            Debug.LogError("[TRAY] CRITICAL: 'Arrangement Manager' is not assigned in TileTrayManager Inspector!", this);

        if (slots == null || slots.Count == 0)
            Debug.LogError("[TRAY] CRITICAL: 'Slots' list on TileTrayManager is EMPTY (0 slots)! Assign your TraySlot objects in the Inspector.", this);
        else if (enableDebugLogs)
            Debug.Log($"[TRAY] Ready with {slots.Count} tray slots.");
    }

    private void OnTileClicked(Tile tile)
    {
        if (enableDebugLogs)
            Debug.Log($"[TRAY] Received OnTileClicked for '{(tile != null ? tile.name : "NULL")}' | isLocked: {isLocked} | trayTiles: {trayTiles.Count}/{slots.Count}");

        if (tile == null) return;
        if (isLocked)
        {
            Debug.LogWarning("[TRAY] Ignored click: Tray is currently LOCKED waiting for previous tile to finish flying.");
            return;
        }
        if (tile.IsBlocked)
        {
            Debug.LogWarning($"[TRAY] Ignored click: '{tile.name}' is marked IsBlocked.");
            return;
        }
        if (trayTiles.Contains(tile))
        {
            Debug.LogWarning($"[TRAY] Ignored click: '{tile.name}' is already in the tray.");
            return;
        }
        if (slots.Count == 0)
        {
            Debug.LogError("[TRAY] Ignored click: 0 slots assigned in TileTrayManager Inspector!");
            return;
        }
        if (trayTiles.Count >= slots.Count)
        {
            Debug.LogWarning($"[TRAY] Ignored click: Tray is full ({trayTiles.Count}/{slots.Count}).");
            return;
        }
        if (gameManager != null && !gameManager.IsPlaying)
        {
            Debug.LogWarning($"[TRAY] Ignored click: TileGameManager state is {gameManager.State} (not Playing).");
            return;
        }

        isLocked = true;

        tilePickedUp?.Invoke(tile);
        tile.DisableInteraction();
        tile.DisableShadows();

        int insertIndex = GetInsertIndex(tile.FaceId);
        trayTiles.Insert(insertIndex, tile);
        AddToMergeData(tile);

        if (enableDebugLogs)
            Debug.Log($"[TRAY] Moving '{tile.name}' (FaceId {tile.FaceId}) to Slot[{insertIndex}] ('{slots[insertIndex].name}').");

        for (int i = insertIndex + 1; i < trayTiles.Count; i++)
        {
            MoveTileToSlot(trayTiles[i], slots[i], null);
        }

        MoveTileToSlot(tile, slots[insertIndex], () => OnTileArrived(tile));
    }

    private int GetInsertIndex(int faceId)
    {
        if (mergeDataByFace.TryGetValue(faceId, out TileMergeData data))
        {
            int lastIndex = -1;
            for (int i = 0; i < data.tiles.Count; i++)
                lastIndex = Mathf.Max(lastIndex, trayTiles.IndexOf(data.tiles[i]));

            if (lastIndex >= 0) return lastIndex + 1;
        }

        return trayTiles.Count;
    }

    private void AddToMergeData(Tile tile)
    {
        if (mergeDataByFace.TryGetValue(tile.FaceId, out TileMergeData data))
            data.Add(tile);
        else
            mergeDataByFace.Add(tile.FaceId, new TileMergeData(tile));
    }

    private void OnTileArrived(Tile tile)
    {
        if (enableDebugLogs)
            Debug.Log($"[TRAY] '{tile.name}' arrived in slot. Unlocking tray.");

        if (tile.Slot != null) tile.Slot.PlayBumpAnimation();

        if (mergeDataByFace.TryGetValue(tile.FaceId, out TileMergeData data) && data.CanMerge())
        {
            if (enableDebugLogs)
                Debug.Log($"[TRAY] Match of {TileMergeData.TilesPerMerge} found for FaceId {tile.FaceId}! Invoking startMerge.");
            MergeTiles(data);
            isLocked = false;
        }
        else if (trayTiles.Count >= slots.Count)
        {
            if (enableDebugLogs)
                Debug.Log("[TRAY] Tray full with no merge -> invoking trayFull.");
            trayFull?.Invoke();
        }
        else
        {
            isLocked = false;
        }
    }

    private void MergeTiles(TileMergeData data)
    {
        List<Tile> tilesToMerge = new List<Tile>(data.tiles);

        mergeDataByFace.Remove(data.faceId);
        for (int i = 0; i < tilesToMerge.Count; i++)
            trayTiles.Remove(tilesToMerge[i]);

        for (int i = 0; i < trayTiles.Count; i++)
        {
            if (trayTiles[i].Slot != slots[i])
                MoveTileToSlot(trayTiles[i], slots[i], null);
        }

        int mergeListeners = startMerge != null ? startMerge.GetInvocationList().Length : 0;
        if (mergeListeners == 0)
        {
            Debug.LogError("[TRAY] startMerge has 0 listeners! Ensure TileMergeManager (MergeManager.cs) is in the scene and active.");
        }

        startMerge?.Invoke(tilesToMerge);
    }

    private void MoveTileToSlot(Tile tile, TraySlot slot, Action completeCallback)
    {
        if (arrangementManager != null)
        {
            arrangementManager.MoveTileToSlot(tile, slot, completeCallback);
            return;
        }

        Debug.LogError("[TRAY] 'Arrangement Manager' is not assigned on TileTrayManager! Snapping tile to slot.");
        tile.SetSlot(slot);
        tile.transform.SetParent(slot.TileParent);
        tile.transform.localPosition = Vector3.zero;
        completeCallback?.Invoke();
    }

    public void ResetTray()
    {
        for (int i = 0; i < trayTiles.Count; i++)
        {
            if (trayTiles[i] == null) continue;

            trayTiles[i].transform.DOKill();
            Destroy(trayTiles[i].gameObject);
        }

        trayTiles.Clear();
        mergeDataByFace.Clear();
        isLocked = false;
    }
}