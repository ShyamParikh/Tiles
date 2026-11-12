/*using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public struct ItemMergeData
{
    public int itemId;
    public List<Item> items;

    public ItemMergeData(Item firstItem)
    {
        itemId = firstItem.ItemId;
        items = new List<Item>(); 
        items.Add(firstItem);
    }

    public void Add(Item item)
    {
        items.Add(item);
    }

    public bool CanMergeItems()
    {
        return items.Count >= 3;
    }
}*/