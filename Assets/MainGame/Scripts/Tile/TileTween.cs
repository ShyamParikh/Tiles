using DG.Tweening;
using UnityEngine;

public static class TileTween
{
    // Gives a tween an id (so DOTween.Kill(id) can find it) and ties it to an object so it can't outlive it.
    public static T Bind<T>(this T tween, string id, GameObject owner) where T : Tween
    {
        return tween.SetId(id).SetLink(owner);
    }
}