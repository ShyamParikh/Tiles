/*
 * TileMergeManager
 * ----------------
 * Plays the merge animation when the tray holds a full set: the three tiles lift up, the two outer ones smash
 * into the middle one, then all three vanish with a particle burst, and mergeFinished fires (the game manager
 * checks for a win then). IsMerging tells other scripts a merge is still playing. At the start of a merge it
 * also tells every tile through OnMergeStarted, which is how a MergeBurstTile asks for its extra merges. The
 * animation is written for exactly three tiles (it sorts them by X and smashes tiles 0 and 2 into tile 1).
 * The burst manager reuses this same animation for its extra sets by raising TileTrayManager.startMerge.
 */
using System;
using System.Collections.Generic;
using com.otm.manager;
using DG.Tweening;
using UnityEngine;

public class TileMergeManager : Manager<TileMergeManager>
{
    #region Public Variables

    public static Action mergeFinished;

    #endregion

    #region Private Variables

    private const string TweenId = "TileMerge";

    [Header("Lift")]
    [SerializeField] private float upDistance = 0.15f;
    [SerializeField] private float upDuration = 0.3f;
    [SerializeField] private Ease upEase = Ease.OutQuad;
    [Tooltip("The direction tiles lift in, in the tile's own axes.")]
    [SerializeField] private Vector3 liftLocalDirection = Vector3.up;

    [Header("Smash")]
    [SerializeField] private float smashDuration = 0.3f;
    [SerializeField] private Ease smashEase = Ease.InQuad;

    [Header("Effects")]
    [SerializeField] private ParticleSystem mergeParticles;
    [SerializeField] private float particleLifetime = 2f;

    private int activeMerges;

    #endregion

    #region Properties

    public bool IsMerging => activeMerges > 0;

    #endregion

    #region Unity Callbacks

    // Starts listening for merges.
    private void OnEnable()
    {
        TileTrayManager.startMerge += OnStartMerge;
    }

    // Stops listening.
    private void OnDisable()
    {
        TileTrayManager.startMerge -= OnStartMerge;
    }

    #endregion

    #region Merge Animation

    // A set started merging: lift the tiles, then smash them together.
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

            // One tween is enough to know when the lift is done.
            if (i == 0) tween.OnComplete(() => Smash(tiles));
        }

        // Let special tiles react (a merge burst tile asks for its extra merges here).
        foreach (Tile tile in tiles) tile.OnMergeStarted(tiles);
    }

    // The two outer tiles slide into the middle one.
    private void Smash(List<Tile> tiles)
    {
        tiles.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        float targetX = tiles[1].transform.position.x;

        tiles[0].transform.DOMoveX(targetX, smashDuration).SetEase(smashEase).Bind(TweenId, tiles[0].gameObject);
        tiles[2].transform.DOMoveX(targetX, smashDuration).SetEase(smashEase).Bind(TweenId, tiles[2].gameObject)
            .OnComplete(() => Vanish(tiles));
    }

    // The tiles disappear with particles and the merge is over.
    private void Vanish(List<Tile> tiles)
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

    // Cancels every merge animation (used when a level restarts).
    public void KillAll()
    {
        DOTween.Kill(TweenId);
        activeMerges = 0;
    }

    #endregion
}