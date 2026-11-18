/*
 * TileTrayManager
 * ---------------
 * Holds the tiles that were picked up. Tiles sit in slot order and equal faces are kept together. When the
 * player clicks a free tile it is registered (tilePickedUp), put next to its face mates and flown into its
 * slot; when three of a kind are in the tray they merge (startMerge); a full tray with no merge fires trayFull.
 * The tray is locked while a tile flies and while a booster or effect runs (those locks are counted, so they
 * never unlock each other). The Tile Pose (size, rotation and animation inside the tray) is set here and
 * handed to TileArrangementManager. Boosters use InsertFromBooster, MergeIfComplete, RemoveTile and CountOf.
 */
using System;
using System.Collections.Generic;
using com.otm.manager;
using DG.Tweening;
using UnityEngine;

public class TileTrayManager : Manager<TileTrayManager>
{
    #region Public Variables

    public static Action<Tile> tilePickedUp;
    public static Action<List<Tile>> startMerge;
    public static Action trayFull;

    #endregion

    #region Private Variables

    [SerializeField] private TileGameManager gameManager;
    [SerializeField] private TileArrangementManager arrangementManager;
    [Tooltip("The tray slots, left to right. The number of slots is the size of the tray.")]
    [SerializeField] private List<TraySlot> slots = new List<TraySlot>();

    [Header("Tile in the tray")]
    [Tooltip("The size and rotation a tile gets when it reaches a slot, and the animation into that pose.")]
    [SerializeField] private TrayPose tilePose = new TrayPose();

    private readonly List<Tile> tiles = new List<Tile>();
    private bool flying;
    private int externalLocks;

    #endregion

    #region Properties

    public int TileCount => tiles.Count;
    public int FreeSlotCount => slots.Count - tiles.Count;
    public bool IsExternallyLocked => externalLocks > 0;
    public bool IsLocked => flying || externalLocks > 0;

    #endregion

    #region Unity Callbacks

    // Starts listening for clicks.
    private void OnEnable()
    {
        TileInputManager.tileClicked += OnTileClicked;
    }

    // Stops listening.
    private void OnDisable()
    {
        TileInputManager.tileClicked -= OnTileClicked;
    }

    #endregion

    #region Locking

    // Boosters and effects lock the tray while they run. Every true needs its own false.
    public void SetExternalLock(bool locked)
    {
        externalLocks = locked ? externalLocks + 1 : Mathf.Max(0, externalLocks - 1);
    }

    #endregion

    #region Picking Up Tiles

    // A free tile was clicked: take it if the tray can.
    private void OnTileClicked(Tile tile)
    {
        bool canTake = tile != null && !IsLocked && !tile.IsBlocked && tile.Slot == null
            && tiles.Count < slots.Count && gameManager.IsPlaying;
        if (!canTake) return;

        flying = true;
        Place(tile, () => OnArrived(tile));
    }

    // The tile reached its slot: merge, unlock, or lose if the tray is full.
    private void OnArrived(Tile tile)
    {
        if (MergeIfComplete(tile.FaceId) || tiles.Count < slots.Count) flying = false;
        else trayFull?.Invoke();
    }

    // Puts a tile next to its face mates and flies it into its slot.
    private void Place(Tile tile, Action onArrived)
    {
        tilePickedUp?.Invoke(tile);
        tile.PrepareForTray();

        int index = InsertIndex(tile.FaceId);
        tiles.Insert(index, tile);
        Compact(index + 1);

        arrangementManager.MoveTileToSlot(tile, slots[index], tilePose, () =>
        {
            if (tile.Slot != null) tile.Slot.PlayBumpAnimation();
            onArrived?.Invoke();
        });
    }

    // Right after the last tile of the same face, or at the end if there is none.
    private int InsertIndex(int faceId)
    {
        int last = tiles.FindLastIndex(t => t.FaceId == faceId);
        return last >= 0 ? last + 1 : tiles.Count;
    }

    // Slides every tile from this index on into its own slot.
    private void Compact(int start)
    {
        for (int i = start; i < tiles.Count; i++)
        {
            if (tiles[i].Slot != slots[i]) arrangementManager.MoveTileToSlot(tiles[i], slots[i], tilePose, null);
        }
    }

    #endregion

    #region Merging

    // Merges the face if the tray holds a full set of it.
    public bool MergeIfComplete(int faceId)
    {
        if (CountOf(faceId) < TileRules.SetSize) return false;

        List<Tile> group = tiles.FindAll(t => t.FaceId == faceId);
        tiles.RemoveAll(t => t.FaceId == faceId);
        Compact(0);
        startMerge?.Invoke(group);
        return true;
    }

    // How many tiles of this face are in the tray right now.
    public int CountOf(int faceId)
    {
        return tiles.FindAll(t => t.FaceId == faceId).Count;
    }

    #endregion

    #region Booster Support

    // Booster path: pull in a tile from anywhere on the board. Call MergeIfComplete once they have all arrived.
    public void InsertFromBooster(Tile tile, Action onArrived)
    {
        if (tile == null || tiles.Count >= slots.Count) onArrived?.Invoke();
        else Place(tile, onArrived);
    }

    // Undo path: takes the tile out and slides the ones after it to the left.
    public bool RemoveTile(Tile tile)
    {
        int index = tiles.IndexOf(tile);
        if (index < 0) return false;

        tiles.RemoveAt(index);
        Compact(index);
        return true;
    }

    #endregion

    #region Resetting

    // Destroys every tray tile and clears all locks (used when a level loads).
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

    #endregion
}