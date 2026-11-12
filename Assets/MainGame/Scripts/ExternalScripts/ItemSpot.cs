/*using UnityEngine;
using static UnityEditor.Progress;

public class ItemSpots : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Transform itemParent;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    private Item item;
    public Item Item => item;

    public void StoreItem(Item item)
    {
        this.item = item;
        item.transform.SetParent(itemParent);

        item.SetSpot(this);
    }

    public void RemoveItem()
    {
        item = null;
    }

    public void PlayBumpAnimation()
    {
        animator.Play("bump", 0, 0);
    }

    public bool IsAvailable() => item == null;
}*/