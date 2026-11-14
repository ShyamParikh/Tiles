using UnityEngine;

/// <summary>
/// One slot of the tray (replaces ItemSpots). It only knows where tiles are parented and how to play the bump animation.
/// Which tile sits in which slot is tracked by TileTrayManager.
/// </summary>
public class TraySlot : MonoBehaviour
{
    [Header("Elements")]
    [Tooltip("Tiles placed in this slot become children of this transform. Keep its scale at 1.")]
    [SerializeField] private Transform tileParent;
    [Tooltip("Optional. Needs a state called 'bump'.")]
    [SerializeField] private Animator animator;

    public Transform TileParent => tileParent != null ? tileParent : transform;

    public void PlayBumpAnimation()
    {
        if (animator != null) animator.Play("bump", 0, 0);
    }
}