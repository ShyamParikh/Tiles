/*
 * TraySlot
 * --------
 * One slot of the tray. It knows where tiles are parented (Tile Parent, keep its scale at 1) and how to play
 * a small bump when a tile lands: either an Animator state called "bump", or, if there is no animator, a quick
 * scale punch on the slot's art SpriteRenderer. Keep the art on its own child object, never on the slot root,
 * because tiles are parented under the slot and would be scaled with it. TileTrayManager owns the list of
 * slots (their order is the order in the tray) and TileArrangementManager flies tiles into them.
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

    #endregion

    #region Properties

    public Transform TileParent => tileParent != null ? tileParent : transform;

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