/*using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEditor.Progress;
using static UnityEngine.GraphicsBuffer;

public class ItemSpotsManager : BaseManager
{
    [Header("Elements")]
    [SerializeField] private Transform itemSpotsParent;
    private ItemSpots[] spots;

    [Header("Settings")]
    [SerializeField] private Vector3 itemLocalPositionOnSpot;
    [SerializeField] private float itemLocalScaleMultiplierOnSpot;
    private bool isBusy;
    private int pendingAnimations = 0;
    private Item pendingMergeCheckItem = null;

    [Header("Data")]
    private Dictionary<int, ItemMergeData> itemMergeDataDictionary = new Dictionary<int, ItemMergeData>();

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private LeanTweenType animationEase;

    [Header("Actions")]
    public static Action<List<Item>> startMerge;
    public static Action<Item> itemPickedUp;

    protected override void Awake()
    {
        base.Awake();      // runs SubscribeToEvents
        StoreSpots();      // stays in Awake, same as before
    }
    protected override void SubscribeToEvents()
    {
        InputManager.itemClicked += OnItemClicked;
    }
    protected override void UnsubscribeFromEvents()
    {
        InputManager.itemClicked -= OnItemClicked;
    }

    private void OnItemClicked(Item item)
    {
        if (isBusy)
        {
            Debug.Log("Busy");
            return;
        }

        isBusy = true;

        itemPickedUp?.Invoke(item);
        HandleItemClicked(item);
    }

    private void HandleItemClicked(Item item)
    {
        if (itemMergeDataDictionary.ContainsKey(item.ItemId))
        {
            HandleItemMergeData(item);
        }
        else
        {
            MoveItemToFirstAvailableSpot(item);
        }
        isBusy = false;
    }

    private void MoveItemToFirstAvailableSpot(Item item)
    {
        ItemSpots firstAvailableSpot = GetAvaliableSpot();

        CreateItemMergeData(item);

        MoveItemToSpot(item, firstAvailableSpot, () => HandleFirstItemReachedSpot(item));
    }

    private void HandleFirstItemReachedSpot(Item item)
    {
        item.Spot.PlayBumpAnimation();
        CheckForGameOver();
    }

    private void CheckForGameOver()
    {
        if (GetAvaliableSpot() == null)
        {
            GameManager.instance.SetGameState(EGameState.GameOver);
        }
        else
        {
            isBusy = false;
        }
    }

    private void HandleItemMergeData(Item item)
    {
        ItemSpots idealSpot = GetIdealSpot(item);

        if (idealSpot == null) idealSpot = GetAvaliableSpot();

        ItemMergeData data = itemMergeDataDictionary[item.ItemId];
        data.Add(item);
        itemMergeDataDictionary[item.ItemId] = data;

        TryMoveItemToIdealSpot(item, idealSpot);
    }

    private ItemSpots GetIdealSpot(Item item)
    {
        List<Item> items = itemMergeDataDictionary[item.ItemId].items;

        List<ItemSpots> itemSpots = new List<ItemSpots>();

        for (int i = 0; i < items.Count; i++)
        {
            itemSpots.Add(items[i].Spot);
        }

        if (itemSpots.Count >= 2)
        {
            itemSpots.Sort((a, b) => b.transform.GetSiblingIndex().CompareTo(a.transform.GetSiblingIndex()));
        }

        int idealSpotIndex = itemSpots[0].transform.GetSiblingIndex() + 1;

        return spots[idealSpotIndex];
    }

    private void TryMoveItemToIdealSpot(Item item, ItemSpots idealSpot)
    {
        if (!idealSpot.IsAvailable())
        {
            HandleIdealSpotNotAvailable(item, idealSpot);
            return;
        }

        MoveItemToSpot(item, idealSpot, () => HandleItemReachedSpot(item));
    }

    private void HandleIdealSpotNotAvailable(Item item, ItemSpots idealSpot)
    {
        MoveAllItemsToRight(idealSpot, item);
    }

    private void MoveAllItemsToRight(ItemSpots idealSpot, Item itemToPlace)
    {
        int idealSpotIndex = idealSpot.transform.GetSiblingIndex();

        for (int i = spots.Length - 2; i >= idealSpotIndex; i--)
        {
            ItemSpots spot = spots[i];

            if (spot.IsAvailable()) continue;

            Item items = spot.Item;

            spot.RemoveItem();

            ItemSpots targetSpot = spots[i + 1];

            if (!targetSpot.IsAvailable())
            {
                Debug.LogError("Error, Target spot should be Available here");
                isBusy = false;
                return;
            }

            MoveItemToSpot(items, targetSpot, () => HandleItemReachedSpot(items, false));
        }
        MoveItemToSpot(itemToPlace, idealSpot, () => HandleItemReachedSpot(itemToPlace));
    }

    private void MoveAllItemsToLeft(Action completeCallback)
    {
        bool callbackTriggered = false;

        for (int i = 3; i < spots.Length; i++)
        {
            ItemSpots spot = spots[i];

            if (spot.IsAvailable()) continue;

            Item items = spot.Item;

            ItemSpots targetSpot = spots[i - 3];

            if (!targetSpot.IsAvailable()) return;

            spot.RemoveItem();

            completeCallback += () => HandleItemReachedSpot(items, false);

            MoveItemToSpot(items, targetSpot, completeCallback);

            callbackTriggered = true;
        }

        if (!callbackTriggered)
        {
            completeCallback?.Invoke();
        }
    }

    private void HandleItemMovedLeft()
    {
        isBusy = false;
    }

    private void MoveItemToSpot(Item item, ItemSpots targetSpot, Action completeCallback)
    {
        // Store original scale before any modifications
        Vector3 originalScale = item.transform.localScale;
        Vector3 targetScale = originalScale;

        // If this is the first time the item is being placed (no spot assigned yet)
        if (item.Spot == null)
        {
            targetScale = originalScale * itemLocalScaleMultiplierOnSpot;
        }

        targetSpot.StoreItem(item);

        // Cancel any existing tweens on this item
        LeanTween.cancel(item.gameObject);

        // Use custom rotation from item if specified, otherwise use zero
        Vector3 targetRotation = item.RotationOnSpot;

        pendingAnimations++;

        // Animate scale, position, and rotation
        LeanTween.scale(item.gameObject, targetScale, animationDuration).setEase(LeanTweenType.easeOutQuad);
        LeanTween.moveLocal(item.gameObject, itemLocalPositionOnSpot, animationDuration).setEase(animationEase);
        LeanTween.rotateLocal(item.gameObject, targetRotation, animationDuration).setEase(LeanTweenType.easeOutQuad)
        .setOnComplete(() => {
            item.DisableShadows();
            item.DisablePhysics();
            completeCallback?.Invoke();

            pendingAnimations--;

            Debug.Log("Item Placement Animation Completed: " + item.ItemId);

            if (pendingAnimations == 0 && pendingMergeCheckItem != null)
            {
                Item waiting = pendingMergeCheckItem;
                pendingMergeCheckItem = null;
                HandleItemReachedSpot(waiting);
            }
        });
    }

    public void HandleItemReachedSpot(Item item, bool checkForMerge = true)
    {
        item.Spot.PlayBumpAnimation();

        if (!checkForMerge) return;

        // Key may have already been removed by a previous callback for the same item type
        if (!itemMergeDataDictionary.ContainsKey(item.ItemId)) return;

        if (pendingAnimations > 0)
        {
            pendingMergeCheckItem = item;
            return;
        }

        if (itemMergeDataDictionary[item.ItemId].CanMergeItems())
            MergeItems(itemMergeDataDictionary[item.ItemId]);
        else
            CheckForGameOver();
    }

    private void MergeItems(ItemMergeData itemMergeData)
    {
        List<Item> itemsToMerge = itemMergeData.items;

        itemMergeDataDictionary.Remove(itemMergeData.itemId);

        for (int i = 0; i < itemsToMerge.Count; i++)
        {
            itemsToMerge[i].Spot.RemoveItem();
        }

        if (itemMergeDataDictionary.Count <= 0)
        {
            isBusy = false;
        }
        else
        {
            MoveAllItemsToLeft(HandleItemMovedLeft);
        }

        startMerge?.Invoke(itemsToMerge);
        Debug.Log("Merge Started for Item ID: " + itemMergeData.itemId);
    }

    private void CreateItemMergeData(Item item)
    {
        itemMergeDataDictionary.Add(item.ItemId, new ItemMergeData(item));
    }

    private ItemSpots GetAvaliableSpot()
    {
        for (int i = 0; i < spots.Length; i++)
        {
            if (spots[i].IsAvailable())
            {
                return spots[i];
            }
        }
        return null;
    }

    public void StoreSpots()
    {
        spots = new ItemSpots[itemSpotsParent.childCount];

        for (int i = 0; i < itemSpotsParent.childCount; i++)
        {
            spots[i] = itemSpotsParent.GetChild(i).GetComponent<ItemSpots>();
        }
    }

    private bool IsItemSpotAvailable(Item item)
    {
        for (int i = 0; i < spots.Length; i++)
        {
            if (spots[i].IsAvailable())
            {
                return true;
            }
        }
        return false;
    }
}*/