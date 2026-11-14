using System;
using System.Collections.Generic;
using com.otm.manager;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Plays the merge animation when 3 identical tiles are in the tray:
/// the tiles lift up, the outer two smash into the middle one, then all three disappear.
/// </summary>
public class TileMergeManager : Manager<TileMergeManager>
{
    private const string MergeTweenId = "TileMerge";

    [Header("Up Merge")]
    [SerializeField] private float upDistance = 0.15f;
    [SerializeField] private float upDuration = 0.3f;
    [SerializeField] private Ease upEase = Ease.OutQuad;
    [Tooltip("Direction the tiles lift in, in the tile's own axes. (0,1,0) = the tile's up.")]
    [SerializeField] private Vector3 liftLocalDirection = Vector3.up;

    [Header("Smash Merge")]
    [SerializeField] private float smashDuration = 0.3f;
    [SerializeField] private Ease smashEase = Ease.InQuad;

    [Header("Merge Effects")]
    [SerializeField] private ParticleSystem mergeParticles;
    [SerializeField] private float particleLifetime = 2f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Actions")]
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
        if (tiles == null || tiles.Count < 3)
        {
            Debug.LogError("[MERGE] OnStartMerge requires at least 3 tiles.");
            return;
        }

        if (enableDebugLogs)
            Debug.Log($"[MERGE] Starting merge animation for {tiles.Count} tiles (FaceId: {tiles[0].FaceId}).");

        activeMerges++;

        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].transform.DOKill(true);

            Vector3 liftDirection = tiles[i].transform.TransformDirection(liftLocalDirection.normalized);
            Vector3 targetPosition = tiles[i].transform.position + liftDirection * upDistance;

            Tween tween = tiles[i].transform
                .DOMove(targetPosition, upDuration)
                .SetEase(upEase)
                .SetId(MergeTweenId)
                .SetLink(tiles[i].gameObject);

            if (i == 0)
            {
                tween.OnComplete(() => SmashTiles(tiles));
            }
        }
    }

    private void SmashTiles(List<Tile> tiles)
    {
        tiles.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        float targetX = tiles[1].transform.position.x;

        tiles[0].transform
            .DOMoveX(targetX, smashDuration)
            .SetEase(smashEase)
            .SetId(MergeTweenId)
            .SetLink(tiles[0].gameObject);

        tiles[2].transform
            .DOMoveX(targetX, smashDuration)
            .SetEase(smashEase)
            .SetId(MergeTweenId)
            .SetLink(tiles[2].gameObject)
            .OnComplete(() => FinalizeMerge(tiles));
    }

    private void FinalizeMerge(List<Tile> tiles)
    {
        Vector3 center = tiles[1] != null ? tiles[1].transform.position : transform.position;

        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] != null) Destroy(tiles[i].gameObject);
        }

        if (mergeParticles != null)
        {
            ParticleSystem particles = Instantiate(mergeParticles, center, Quaternion.identity, transform);
            particles.Play();
            Destroy(particles.gameObject, particleLifetime);
        }

        activeMerges = Mathf.Max(0, activeMerges - 1);
        if (enableDebugLogs) Debug.Log("[MERGE] Merge finished.");
        mergeFinished?.Invoke();
    }

    public void KillAll()
    {
        DOTween.Kill(MergeTweenId);
        activeMerges = 0;
    }
}