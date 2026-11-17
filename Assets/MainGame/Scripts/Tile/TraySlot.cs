using UnityEngine;

// One slot of the tray. It only knows where tiles are parented and how to play its bump animation.
public class TraySlot : MonoBehaviour
{
    [Tooltip("Tiles in this slot become children of this transform. Keep its scale at 1.")]
    [SerializeField] private Transform tileParent;
    [Tooltip("Optional. Needs a state called 'bump'.")]
    [SerializeField] private Animator animator;

    public Transform TileParent => tileParent != null ? tileParent : transform;

    public void PlayBumpAnimation()
    {
        if (animator != null) animator.Play("bump", 0, 0);
    }
}