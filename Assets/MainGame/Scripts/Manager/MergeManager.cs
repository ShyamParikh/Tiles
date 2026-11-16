using System;
using System.Collections.Generic;
using com.otm.manager;
using DG.Tweening;
using UnityEngine;

/// <summary>Merge animation: the 3 tiles lift, the outer two smash into the middle one, then all vanish.</summary>
public class TileMergeManager : Manager<TileMergeManager>
{
    private const string TweenId = "TileMerge";

    [Header("Up Merge")]
    [SerializeField] private float upDistance = 0.15f;
    [SerializeField] private float upDuration = 0.3f;
    [SerializeField] private Ease upEase = Ease.OutQuad;
    [Tooltip("Direction the tiles lift in, in the tile's own axes.")]
    [SerializeField] private Vector3 liftLocalDirection = Vector3.up;

    [Header("Smash Merge")]
    [SerializeField] private float smashDuration = 0.3f;
    [SerializeField] private Ease smashEase = Ease.InQuad;

    [Header("Merge Effects")]
    [SerializeField] private ParticleSystem mergeParticles;
    [SerializeField] private float particleLifetime = 2f;

    public static Action mergeFinished;

    private int activeMerges;

    public bool IsMerging => activeMerges > 0;

    private void OnEnable()
    {
        TileTrayManager.startMerge += OnStartMerge;
    }

    private void OnDisable()
    {
        TileTrayManager.startMerge -= OnStartMerge;
    }

    private void OnStartMerge(List<Tile> tiles)
    {
        if (tiles == null || tiles.Count < TileRules.SetSize) return;

        activeMerges++;
        Vector3 lift = liftLocalDirection.normalized;

        for (int i = 0; i < tiles.Count; i++)
        {
            Tile tile = tiles[i];
            tile.transform.DOKill(true);

            Tween tween = tile.transform
                .DOMove(tile.transform.position + tile.transform.TransformDirection(lift) * upDistance, upDuration)
                .SetEase(upEase)
                .Bind(TweenId, tile.gameObject);

            if (i == 0) tween.OnComplete(() => Smash(tiles));
        }

        foreach (Tile tile in tiles) tile.NotifyMergeStarted(tiles);
    }

    private void Smash(List<Tile> tiles)
    {
        tiles.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        float targetX = tiles[1].transform.position.x;

        tiles[0].transform.DOMoveX(targetX, smashDuration).SetEase(smashEase).Bind(TweenId, tiles[0].gameObject);
        tiles[2].transform.DOMoveX(targetX, smashDuration).SetEase(smashEase).Bind(TweenId, tiles[2].gameObject)
            .OnComplete(() => FinishMerge(tiles));
    }

    private void FinishMerge(List<Tile> tiles)
    {
        Vector3 center = tiles[1] != null ? tiles[1].transform.position : transform.position;

        foreach (Tile tile in tiles)
        {
            if (tile != null) Destroy(tile.gameObject);
        }

        if (mergeParticles != null)
        {
            ParticleSystem particles = Instantiate(mergeParticles, center, Quaternion.identity, transform);
            particles.Play();
            Destroy(particles.gameObject, particleLifetime);
        }

        activeMerges = Mathf.Max(0, activeMerges - 1);
        mergeFinished?.Invoke();
    }

    public void KillAll()
    {
        DOTween.Kill(TweenId);
        activeMerges = 0;
    }
}