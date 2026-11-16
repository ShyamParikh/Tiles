using System;
using System.Collections.Generic;
using com.otm.manager;
using DG.Tweening;
using UnityEngine;

/// <summary>Holds the tiles in the tray, keeps same faces together and merges complete sets.</summary>
public class TileTrayManager : Manager<TileTrayManager>
{
    [Header("Scene References (Assign in Inspector)")]
    [SerializeField] private TileGameManager gameManager;
    [SerializeField] private TileArrangementManager arrangementManager;
    [Tooltip("Drag all TraySlot objects in order, left to right.")]
    [SerializeField] private List<TraySlot> slots = new List<TraySlot>();

    public static Action<Tile> tilePickedUp;
    public static Action<List<Tile>> startMerge;
    public static Action trayFull;

    private readonly List<Tile> tiles = new List<Tile>();
    private bool flying;
    private int externalLocks;

    public int TileCount => tiles.Count;
    public int SlotCount => slots.Count;
    public int FreeSlotCount => slots.Count - tiles.Count;
    public bool IsExternallyLocked => externalLocks > 0;
    public bool IsLocked => flying || externalLocks > 0;

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
        if (gameManager == null || arrangementManager == null || slots.Count == 0)
            Debug.LogError("[TRAY] Game Manager, Arrangement Manager and at least one Slot must be assigned!", this);
    }

    /// <summary>Boosters and effects lock the tray while they run. Every true needs its own false.</summary>
    public void SetExternalLock(bool locked)
    {
        externalLocks = locked ? externalLocks + 1 : Mathf.Max(0, externalLocks - 1);
    }

    public int CountOf(int faceId)
    {
        int count = 0;
        foreach (Tile tile in tiles)
        {
            if (tile.FaceId == faceId) count++;
        }
        return count;
    }

    private void OnTileClicked(Tile tile)
    {
        if (tile == null || IsLocked || tile.IsBlocked || tile.Slot != null || tiles.Count >= slots.Count || !gameManager.IsPlaying) return;

        flying = true;
        Place(tile, () => OnArrived(tile));
    }

    private void OnArrived(Tile tile)
    {
        if (MergeIfComplete(tile.FaceId) || tiles.Count < slots.Count) flying = false;
        else trayFull?.Invoke();
    }

    /// <summary>Puts a tile next to its face mates and flies it to its slot.</summary>
    private void Place(Tile tile, Action onArrived)
    {
        tilePickedUp?.Invoke(tile);
        tile.PrepareForTray();

        int index = InsertIndex(tile.FaceId);
        tiles.Insert(index, tile);
        Compact(index + 1);

        arrangementManager.MoveTileToSlot(tile, slots[index], () =>
        {
            if (tile.Slot != null) tile.Slot.PlayBumpAnimation();
            onArrived?.Invoke();
        });
    }

    private int InsertIndex(int faceId)
    {
        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            if (tiles[i].FaceId == faceId) return i + 1;
        }
        return tiles.Count;
    }

    /// <summary>Moves every tile from this index on into its own slot.</summary>
    private void Compact(int start)
    {
        for (int i = start; i < tiles.Count; i++)
        {
            if (tiles[i].Slot != slots[i]) arrangementManager.MoveTileToSlot(tiles[i], slots[i], null);
        }
    }

    /// <summary>Merges the face if the tray holds a full set of it.</summary>
    public bool MergeIfComplete(int faceId)
    {
        if (CountOf(faceId) < TileRules.SetSize) return false;

        List<Tile> group = tiles.FindAll(t => t.FaceId == faceId);
        tiles.RemoveAll(t => t.FaceId == faceId);
        Compact(0);
        startMerge?.Invoke(group);
        return true;
    }

    /// <summary>Booster path: adds a tile from anywhere on the board. Call MergeIfComplete once all arrived.</summary>
    public void InsertFromBooster(Tile tile, Action onArrived)
    {
        if (tile == null || tiles.Count >= slots.Count) onArrived?.Invoke();
        else Place(tile, onArrived);
    }

    /// <summary>Undo path: removes the tile and slides the ones after it to the left.</summary>
    public bool RemoveTile(Tile tile)
    {
        int index = tiles.IndexOf(tile);
        if (index < 0) return false;

        tiles.RemoveAt(index);
        Compact(index);
        return true;
    }

    public void ResetTray()
    {
        foreach (Tile tile in tiles)
        {
            if (tile == null) continue;
            tile.transform.DOKill();
            Destroy(tile.gameObject);
        }

        tiles.Clear();
        flying = false;
        externalLocks = 0;
    }
}