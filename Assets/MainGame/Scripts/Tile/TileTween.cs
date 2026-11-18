/*
 * TileTween
 * ---------
 * One tiny extension method used by every animation script. Bind(id, owner) gives a tween an id, so
 * DOTween.Kill(id) can cancel a whole group at once (a level restart does this), and ties the tween to an
 * object, so the tween can never outlive the tile it moves.
 */
using DG.Tweening;
using UnityEngine;

public static class TileTween
{
    #region Methods

    // Tags a tween with an id and links its lifetime to an object.
    public static T Bind<T>(this T tween, string id, GameObject owner) where T : Tween
    {
        return tween.SetId(id).SetLink(owner);
    }

    #endregion
}