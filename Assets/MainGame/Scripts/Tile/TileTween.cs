using DG.Tweening;
using UnityEngine;

public static class TileTween
{
    /// <summary>Tags a tween with an id (for DOTween.Kill) and ties its life to the owner object.</summary>
    public static T Bind<T>(this T tween, string id, GameObject owner) where T : Tween
    {
        return tween.SetId(id).SetLink(owner);
    }
}