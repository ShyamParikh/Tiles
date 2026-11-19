/*
 * TileTrayManager
 * ---------------
 * Holds the tiles that were picked up. Tiles sit in slot order and equal faces are kept together. When the
 * player clicks a free tile it is registered (tilePickedUp), put next to its face mates and flown into its
 * slot. Several tiles can fly at the same time: clicking is only blocked while a booster or effect runs
 * (external locks, which are counted so they never unlock each other). A set merges (startMerge) once three
 * tiles of a face have ARRIVED, tiles still flying don't count. A full tray with nothing flying and no merge
 * fires trayFull. If a merge or an insert moves a slot under a tile that is still flying, its arrival is kept
 * and the tile flies on to the new slot. The Tile Pose (size, rotation and animation inside the tray) is set
 * here and handed to TileArrangementManager. Boosters use InsertFromBooster, MergeIfComplete, RemoveTile and
 * CountOf, and wait while IsLocked (a booster or a flying tile).
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

    #region Nested Types

    // A tile that is still flying to its slot, and what to do when it lands.
    private class PendingArrival
    {
        public Tile tile;
        public Action onArrived;
        public bool fromClick;
        public TileTrayManager owner;

        public void Arrive()
        {
            owner.HandleArrival(this);
        }
    }

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
    private readonly Dictionary<Tile, PendingArrival> pendingArrivals = new Dictionary<Tile, PendingArrival>();
    private int externalLocks;

    #endregion

    #region Properties

    public int TileCount
    {
        get { return tiles.Count; }
    }

    public int FreeSlotCount
    {
        get { return slots.Count - tiles.Count; }
    }

    // True while a booster or effect runs. Player clicks are only blocked by this.
    public bool IsExternallyLocked
    {
        get { return externalLocks > 0; }
    }

    // True while any tile is still flying to its slot.
    public bool IsFlying
    {
        get { return pendingArrivals.Count > 0; }
    }

    // Boosters wait for this: a booster or effect is running, or a tile is still flying.
    public bool IsLocked
    {
        get { return IsFlying || externalLocks > 0; }
    }

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

    // A free tile was clicked: take it if the tray can. Tiles that are still flying don't stop this.
    private void OnTileClicked(Tile tile)
    {
        bool canTake = tile != null && !IsExternallyLocked && !tile.IsBlocked && tile.Slot == null
            && tiles.Count < slots.Count && gameManager.IsPlaying;
        if (!canTake)
        {
            return;
        }

        Place(tile, null, true);
    }

    // The tile reached its slot: bump the slot, then merge or lose (click path) or tell the booster.
    private void HandleArrival(PendingArrival pending)
    {
        pendingArrivals.Remove(pending.tile);

        if (pending.tile.Slot != null)
        {
            pending.tile.Slot.PlayBumpAnimation();
        }
        if (pending.onArrived != null)
        {
            pending.onArrived.Invoke();
        }
        if (!pending.fromClick)
        {
            return;
        }

        if (MergeIfComplete(pending.tile.FaceId))
        {
            return;
        }

        // Full tray, nothing else on its way and no merge: the player lost.
        if (tiles.Count >= slots.Count && !IsFlying && trayFull != null)
        {
            trayFull.Invoke();
        }
    }

    // Puts a tile next to its face mates and flies it into its slot.
    private void Place(Tile tile, Action onArrived, bool fromClick)
    {
        if (tilePickedUp != null)
        {
            tilePickedUp.Invoke(tile);
        }
        tile.PrepareForTray();

        int index = InsertIndex(tile.FaceId);
        tiles.Insert(index, tile);

        PendingArrival pending = new PendingArrival();
        pending.tile = tile;
        pending.onArrived = onArrived;
        pending.fromClick = fromClick;
        pending.owner = this;
        pendingArrivals[tile] = pending;

        Compact(index + 1);
        arrangementManager.MoveTileToSlot(tile, slots[index], tilePose, false, pending.Arrive);
    }

    // Right after the last tile of the same face, or at the end if there is none.
    private int InsertIndex(int faceId)
    {
        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            if (tiles[i].FaceId == faceId)
            {
                return i + 1;
            }
        }
        return tiles.Count;
    }

    // Slides every tile from this index on into its own slot. A tile still flying keeps its arrival and flies on.
    private void Compact(int start)
    {
        for (int i = start; i < tiles.Count; i++)
        {
            Tile tile = tiles[i];
            if (tile.Slot == slots[i])
            {
                continue;
            }

            PendingArrival pending;
            bool stillFlying = pendingArrivals.TryGetValue(tile, out pending);

            Action onArrived = null;
            if (stillFlying)
            {
                onArrived = pending.Arrive;
            }
            arrangementManager.MoveTileToSlot(tile, slots[i], tilePose, stillFlying, onArrived);
        }
    }

    #endregion

    #region Merging

    // Merges the face if the tray holds a full set of it that has arrived.
    public bool MergeIfComplete(int faceId)
    {
        List<Tile> group = new List<Tile>();
        foreach (Tile tile in tiles)
        {
            if (tile.FaceId == faceId && !pendingArrivals.ContainsKey(tile) && group.Count < TileRules.SetSize)
            {
                group.Add(tile);
            }
        }
        if (group.Count < TileRules.SetSize)
        {
            return false;
        }

        foreach (Tile tile in group)
        {
            tiles.Remove(tile);
        }
        Compact(0);

        if (startMerge != null)
        {
            startMerge.Invoke(group);
        }
        return true;
    }

    // How many tiles of this face have arrived in the tray.
    public int CountOf(int faceId)
    {
        int count = 0;
        foreach (Tile tile in tiles)
        {
            if (tile.FaceId == faceId && !pendingArrivals.ContainsKey(tile))
            {
                count++;
            }
        }
        return count;
    }

    #endregion

    #region Booster Support

    // Booster path: pull in a tile from anywhere on the board. Call MergeIfComplete once they have all arrived.
    public void InsertFromBooster(Tile tile, Action onArrived)
    {
        if (tile == null || tiles.Count >= slots.Count)
        {
            if (onArrived != null)
            {
                onArrived.Invoke();
            }
        }
        else
        {
            Place(tile, onArrived, false);
        }
    }

    // Undo path: takes the tile out and slides the ones after it to the left.
    public bool RemoveTile(Tile tile)
    {
        int index = tiles.IndexOf(tile);
        if (index < 0)
        {
            return false;
        }

        pendingArrivals.Remove(tile);
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
            if (tile == null)
            {
                continue;
            }
            tile.transform.DOKill();
            Destroy(tile.gameObject);
        }

        tiles.Clear();
        pendingArrivals.Clear();
        externalLocks = 0;
    }

    #endregion
}