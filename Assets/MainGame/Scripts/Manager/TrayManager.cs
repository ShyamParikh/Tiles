using System;
using System.Collections.Generic;
using com.otm.manager;
using DG.Tweening;
using UnityEngine;

// Holds the picked tiles: same faces sit together, three of a kind merge, a full tray is a loss.
public class TileTrayManager : Manager<TileTrayManager>
{
    [SerializeField] private TileGameManager gameManager;
    [SerializeField] private TileArrangementManager arrangementManager;
    [Tooltip("The tray slots, left to right. The number of slots is the size of the tray.")]
    [SerializeField] private List<TraySlot> slots = new List<TraySlot>();

    [Header("Tile in the tray")]
    [Tooltip("The size and rotation a tile gets when it reaches a slot, and the animation into that pose.")]
    [SerializeField] private TrayPose tilePose = new TrayPose();

    public static Action<Tile> tilePickedUp;
    public static Action<List<Tile>> startMerge;
    public static Action trayFull;

    private readonly List<Tile> tiles = new List<Tile>();
    private bool flying;
    private int externalLocks;

    public int TileCount => tiles.Count;
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

    // Boosters and effects lock the tray while they run. Every true needs its own false.
    public void SetExternalLock(bool locked)
    {
        externalLocks = locked ? externalLocks + 1 : Mathf.Max(0, externalLocks - 1);
    }

    // How many tiles of this face are in the tray right now.
    public int CountOf(int faceId)
    {
        return tiles.FindAll(t => t.FaceId == faceId).Count;
    }

    private void OnTileClicked(Tile tile)
    {
        bool canTake = tile != null && !IsLocked && !tile.IsBlocked && tile.Slot == null
            && tiles.Count < slots.Count && gameManager.IsPlaying;
        if (!canTake) return;

        flying = true;
        Place(tile, () => OnArrived(tile));
    }

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