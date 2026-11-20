/*
 * TraySlot
 * --------
 * One slot of the tray. It knows where tiles are parented (Tile Parent, keep its scale at 1) and how to play
 * a small bump when a tile lands: either an Animator state called "bump", or, if there is no animator, a quick
 * scale punch on the slot's art SpriteRenderer. Keep the art on its own child object, never on the slot root,
 * because tiles are parented under the slot and would be scaled with it. It also tells a tile in the tray where
 * to draw: on the same Sorting Layer as the tray art (or the tray Canvas) and a little above it, so a tile can
 * never go beneath the tray. Assign either the art SpriteRenderer or the Tray Canvas (the one with the sorting)
 * for this. WorldSize tells TrayPose how big the slot is on screen, so a tile can be scaled to fit it: it comes
 * from the Size Rect (best for a UI slot), else the art sprite, else the Manual World Size. TileTrayManager owns
 * the list of slots (their order is the order in the tray) and TileArrangementManager flies tiles into them.
 */
using DG.Tweening;
using UnityEngine;

public class TraySlot : MonoBehaviour
{
    #region Private Variables

    [Tooltip("Tiles in this slot become children of this transform. Keep its scale at 1.")]
    [SerializeField] private Transform tileParent;

    [Header("Bump when a tile lands (use either)")]
    [Tooltip("Optional animator with a state called 'bump'.")]
    [SerializeField] private Animator animator;
    [Tooltip("Optional: the slot's art, on its own child object. With no animator it gets a quick scale punch.")]
    [SerializeField] private SpriteRenderer art;
    [SerializeField] private float bumpStrength = 0.12f;
    [SerializeField] private float bumpDuration = 0.2f;

    [Header("Tile sorting (a tile in this slot draws above the tray)")]
    [Tooltip("Use this when the tray art is a UI Image: the Canvas that holds it. Not needed when the art SpriteRenderer above is set.")]
    [SerializeField] private Canvas trayCanvas;
    [Tooltip("How far above the tray art or canvas a tile in this slot draws.")]
    [SerializeField] private int tileOrderAboveTray = 10;

    [Header("Tile size (what a tile is fitted into)")]
    [Tooltip("Best for a UI slot: the RectTransform of the slot image. Empty = the Art sprite's size is used.")]
    [SerializeField] private RectTransform sizeRect;
    [Tooltip("Only used when there is no Size Rect and no Art: the slot size in world units. 0 = tiles are not fitted.")]
    [SerializeField] private Vector2 manualWorldSize;

    private readonly Vector3[] corners = new Vector3[4];

    #endregion

    #region Properties

    public Transform TileParent => tileParent != null ? tileParent : transform;

    // True when the slot knows the tray's sorting (an art sprite or a canvas is assigned).
    public bool HasSorting => art != null || trayCanvas != null;

    // The Sorting Layer a tile in this slot must use.
    public int SortingLayerId
    {
        get { return art != null ? art.sortingLayerID : SortingCanvas.sortingLayerID; }
    }

    // The sorting order a tile in this slot must use.
    public int TileSortingOrder
    {
        get { return (art != null ? art.sortingOrder : SortingCanvas.sortingOrder) + tileOrderAboveTray; }
    }

    // The slot's width and height on screen, in world units (0 = unknown).
    public Vector2 WorldSize
    {
        get
        {
            if (sizeRect != null)
            {
                sizeRect.GetWorldCorners(corners);
                return new Vector2(Vector3.Distance(corners[0], corners[3]), Vector3.Distance(corners[0], corners[1]));
            }
            if (art != null)
            {
                return art.bounds.size;
            }
            return manualWorldSize;
        }
    }

    // The canvas that really decides the order: the nested one if it overrides sorting, otherwise the root one.
    private Canvas SortingCanvas => trayCanvas.overrideSorting ? trayCanvas : trayCanvas.rootCanvas;

    #endregion

    #region Methods

    // Plays the bump (animator state, or a scale punch on the art).
    public void PlayBumpAnimation()
    {
        if (animator != null)
        {
            animator.Play("bump", 0, 0);
        }
        else if (art != null)
        {
            art.transform.DOKill(true);   // finish a punch that is still running so the scale resets
            art.transform.DOPunchScale(Vector3.one * bumpStrength, bumpDuration);
        }
    }

    #endregion
}