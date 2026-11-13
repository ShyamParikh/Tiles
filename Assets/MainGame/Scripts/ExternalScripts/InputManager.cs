/*using System;
using UnityEngine;

public class InputManager : BaseManager
{
    public static Action<Item> itemClicked;

    [Header("Settings")]
    private Item currentItem;
    [SerializeField] private Material selectOutlineMaterial;

    void Update()
    {
        if (IsGame())
        {
            HandleControl();
        }
    }

    private void HandleControl()
    {
        if (Input.GetMouseButton(0))
        {
            HandleMouseDrag();
        }
        else if (Input.GetMouseButtonUp(0) && currentItem != null)
        {
            HandleMouseUp();
        }
    }

    private void HandleMouseDrag()
    {
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 20))
        {
            DeselectCurrentItem();
            return;
        }

        // Try to find Item component in the hit object or its parents
        Item item = GetItemFromHit(hit);

        if (item != null)
        {
            // Only change selection if it's a different item
            if (currentItem != item)
            {
                DeselectCurrentItem();
                currentItem = item;
                currentItem.Select(selectOutlineMaterial);
            }
        }
        else
        {
            DeselectCurrentItem();
        }
    }

    private Item GetItemFromHit(RaycastHit hit)
    {
        // Start from the hit transform and search upwards in hierarchy
        Transform current = hit.collider.transform;

        // Search up to 3 levels (covers most cases: collider -> visual child -> item parent)
        for (int i = 0; i < 3; i++)
        {
            if (current == null) break;

            Item item = current.GetComponent<Item>();
            if (item != null)
            {
                return item;
            }

            current = current.parent;
        }

        return null;
    }

    private void DeselectCurrentItem()
    {
        if (currentItem != null)
        {
            currentItem.Deselect();
            currentItem = null;
        }
    }

    private void HandleMouseUp()
    {
        currentItem.Deselect();
        itemClicked?.Invoke(currentItem);
        currentItem = null;
    }
}*/