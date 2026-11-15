using System;
using System.Collections.Generic;
using com.otm.manager;
using DG.Tweening;
using UnityEngine;

public class TileArrangementManager : Manager<TileArrangementManager>
{
    private const string TweenId = "TileArrangement";

    [Header("Editor Testing (Optional)")]
    [SerializeField] private TileGameManager gameManager;

    [Header("1. Fall Intro (DOTween)")]
    [Tooltip("How far along Fall Direction the tiles start before dropping into place.")]
    [SerializeField] private float fallHeight = 8f;
    [Tooltip("Direction tiles fall from in board local space. (0,1,0) = top-down drop, (0,0,-1) = front-to-back drop.")]
    [SerializeField] private Vector3 fallDirection = Vector3.up;
    [SerializeField] private float fallDuration = 0.45f;
    [Tooltip("Pause between one tile starting to fall and the next one.")]
    [SerializeField] private float delayBetweenTiles = 0.03f;
    [Tooltip("The whole fill-in never takes longer than this (the delay shrinks for big boards).")]
    [SerializeField] private float maxTotalDelay = 2.5f;
    [SerializeField] private Ease fallEase = Ease.OutBack;
    [SerializeField] private bool reverseRowOrder = false;

    [Header("2. Move To Tray Slot (Straight Line - Keeps Scale & Rotation)")]
    [Tooltip("Local position offset inside the target TraySlot.")]
    [SerializeField] private Vector3 tileLocalPositionOnSlot = Vector3.zero;
    [SerializeField] private float moveDuration = 0.3f;
    [Tooltip("OutExpo / OutQuint accelerates fast at the start and slows down smoothly into the spot.")]
    [SerializeField] private Ease moveEase = Ease.OutExpo;

    [Header("3. Arrival Double-Boop")]
    [Tooltip("Scale multiplier for the 1st boop when reaching the slot (e.g., 1.18 = +18% size).")]
    [SerializeField] private float firstBoopScale = 1.18f;
    [Tooltip("Scale multiplier for the 2nd boop (e.g., 1.09 = +9% size).")]
    [SerializeField] private float secondBoopScale = 1.09f;
    [Tooltip("Total duration of each grow-and-return boop cycle.")]
    [SerializeField] private float singleBoopDuration = 0.12f;

    [Header("4. Tray Shift (Position Only)")]
    [SerializeField] private float shiftDuration = 0.2f;
    [SerializeField] private Ease shiftEase = Ease.OutQuad;

    [Header("5. Blocked Tile Wiggle (DOTween)")]
    [SerializeField] private float wiggleDuration = 0.32f;
    [Tooltip("Rotational shake angle in degrees (X, Y, Z) when a covered tile is clicked.")]
    [SerializeField] private Vector3 wiggleRotationStrength = new Vector3(0f, 0f, 12f);
    [Tooltip("Positional shake offset in local units when a covered tile is clicked.")]
    [SerializeField] private Vector3 wigglePositionStrength = new Vector3(0.06f, 0f, 0f);
    [SerializeField] private int wiggleVibrato = 18;
    [SerializeField] private float wiggleRandomness = 45f;

    [Header("6. Return To Board (Undo Booster)")]
    [SerializeField] private float returnDuration = 0.4f;
    [SerializeField] private Ease returnEase = Ease.OutBack;

    [Header("7. Shuffle Booster")]
    [Tooltip("Time for all tiles to fly into the middle of the board.")]
    [SerializeField] private float shuffleGatherDuration = 0.35f;
    [SerializeField] private Ease shuffleGatherEase = Ease.InOutQuad;
    [Tooltip("Small random offset so gathered tiles don't sit exactly on top of each other.")]
    [SerializeField] private float shuffleGatherJitter = 0.25f;
    [Tooltip("How long the tiles stay (spinning) in the middle before spreading out.")]
    [SerializeField] private float shuffleHoldDuration = 0.3f;
    [Tooltip("Spin axis in the tile's local space while gathered. (0,0,1) for XY_Vertical boards, (0,1,0) for XZ_Flat.")]
    [SerializeField] private Vector3 shuffleSpinAxis = new Vector3(0f, 0f, 1f);
    [SerializeField] private float shuffleSpreadDuration = 0.4f;
    [SerializeField] private Ease shuffleSpreadEase = Ease.OutBack;
    [Tooltip("Delay between one tile starting to spread and the next (bottom layers first).")]
    [SerializeField] private float shuffleSpreadStagger = 0.01f;
    [Tooltip("The staggered spread never takes longer than this in total.")]
    [SerializeField] private float shuffleMaxSpreadDelay = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    public void PlayFallIntro(List<Tile> tiles, Action onComplete)
    {
        List<Tile> ordered = new List<Tile>();
        foreach (Tile tile in tiles)
        {
            if (tile != null) ordered.Add(tile);
        }

        if (ordered.Count == 0)
        {
            if (enableDebugLogs) Debug.LogWarning("[ARRANGE] PlayFallIntro called with 0 tiles.");
            onComplete?.Invoke();
            return;
        }

        float rowSign = reverseRowOrder ? -1f : 1f;
        ordered.Sort((a, b) =>
        {
            int result = a.LayerIndex.CompareTo(b.LayerIndex);
            if (result != 0) return result;

            result = (rowSign * a.CellPosition.y).CompareTo(rowSign * b.CellPosition.y);
            if (result != 0) return result;

            return a.CellPosition.x.CompareTo(b.CellPosition.x);
        });

        Vector3 dir = fallDirection.sqrMagnitude > 0.0001f ? fallDirection.normalized : Vector3.up;
        Vector3 startOffset = dir * fallHeight;

        float delay = Mathf.Min(delayBetweenTiles, maxTotalDelay / ordered.Count);
        int remaining = ordered.Count;

        for (int i = 0; i < ordered.Count; i++)
        {
            Tile tile = ordered[i];
            Vector3 finalPosition = tile.RestLocalPosition;

            tile.gameObject.SetActive(false);
            tile.transform.localPosition = finalPosition + startOffset;

            tile.transform.DOLocalMove(finalPosition, fallDuration)
                .SetEase(fallEase)
                .SetDelay(i * delay)
                .SetId(TweenId)
                .SetLink(tile.gameObject)
                .OnStart(() =>
                {
                    if (tile != null) tile.gameObject.SetActive(true);
                })
                .OnComplete(() =>
                {
                    remaining--;
                    if (remaining == 0)
                    {
                        if (enableDebugLogs) Debug.Log("[ARRANGE] Fall Intro COMPLETE.");
                        onComplete?.Invoke();
                    }
                });
        }
    }

    /// <summary>
    /// Moves a tile in a straight line to its tray slot (fast start, smooth slow-down),
    /// keeps original scale and rotation, and boops twice on arrival.
    /// </summary>
    public void MoveTileToSlot(Tile tile, TraySlot slot, Action onComplete)
    {
        if (tile == null || slot == null) return;

        bool wasAlreadyInTray = tile.Slot != null;

        tile.transform.DOKill();

        if (!wasAlreadyInTray)
        {
            tile.RestoreBoardRestTransform();
            tile.SetSlot(slot);
            // worldPositionStays = true keeps exact original scale, rotation, and orientation
            tile.transform.SetParent(slot.TileParent, true);
            tile.RecordTrayRestScale();
        }
        else
        {
            tile.RestoreTrayRestScale();
            tile.SetSlot(slot);
            tile.transform.SetParent(slot.TileParent, true);
        }

        if (wasAlreadyInTray)
        {
            tile.transform.DOLocalMove(tileLocalPositionOnSlot, shiftDuration)
                .SetEase(shiftEase)
                .SetId(TweenId)
                .SetLink(tile.gameObject)
                .OnComplete(() =>
                {
                    if (tile != null)
                    {
                        tile.transform.localPosition = tileLocalPositionOnSlot;
                        tile.RestoreTrayRestScale();
                    }
                    onComplete?.Invoke();
                });
        }
        else
        {
            Vector3 baseScale = tile.TrayRestLocalScale;
            float halfBoop = Mathf.Max(0.01f, singleBoopDuration * 0.5f);

            Sequence flySeq = DOTween.Sequence();
            flySeq.SetTarget(tile.transform);
            flySeq.SetId(TweenId);
            flySeq.SetLink(tile.gameObject);

            // 1. Straight-line flight: fast initial acceleration, smooth deceleration into slot
            flySeq.Append(
                tile.transform.DOLocalMove(tileLocalPositionOnSlot, moveDuration)
                    .SetEase(moveEase)
            );

            // 2. Arrival Boop #1 (grow slightly -> return to original scale)
            flySeq.Append(
                tile.transform.DOScale(baseScale * firstBoopScale, halfBoop)
                    .SetEase(Ease.OutQuad)
            );
            flySeq.Append(
                tile.transform.DOScale(baseScale, halfBoop)
                    .SetEase(Ease.InOutQuad)
            );

            // 3. Arrival Boop #2 (grow slightly -> return to original scale)
            flySeq.Append(
                tile.transform.DOScale(baseScale * secondBoopScale, halfBoop)
                    .SetEase(Ease.OutQuad)
            );
            flySeq.Append(
                tile.transform.DOScale(baseScale, halfBoop)
                    .SetEase(Ease.InOutQuad)
            );

            flySeq.OnComplete(() =>
            {
                if (tile != null)
                {
                    tile.transform.localPosition = tileLocalPositionOnSlot;
                    tile.RestoreTrayRestScale();
                }
                onComplete?.Invoke();
            });
        }
    }

    /// <summary>
    /// Plays a snappy DOTween wiggle when a player clicks a tile that is covered/blocked.
    /// Always snaps back to the tile's exact rest position and rotation.
    /// </summary>
    public void PlayBlockedWiggle(Tile tile)
    {
        if (tile == null) return;

        tile.transform.DOKill();
        tile.RestoreBoardRestTransform();

        if (enableDebugLogs)
            Debug.Log($"[ARRANGE] Wiggling blocked tile '{tile.name}'.");

        Sequence wiggleSeq = DOTween.Sequence();
        wiggleSeq.SetTarget(tile.transform);
        wiggleSeq.SetId(TweenId);
        wiggleSeq.SetLink(tile.gameObject);

        if (wiggleRotationStrength.sqrMagnitude > 0.0001f)
        {
            wiggleSeq.Join(
                tile.transform.DOShakeRotation(wiggleDuration, wiggleRotationStrength, wiggleVibrato, wiggleRandomness, true)
            );
        }

        if (wigglePositionStrength.sqrMagnitude > 0.0001f)
        {
            wiggleSeq.Join(
                tile.transform.DOShakePosition(wiggleDuration, wigglePositionStrength, wiggleVibrato, wiggleRandomness, false, true)
            );
        }

        wiggleSeq.OnComplete(() =>
        {
            if (tile != null) tile.RestoreBoardRestTransform();
        });
    }

    /// <summary>
    /// Undo booster: flies a tile from the tray back to its board spot (position, rotation and scale).
    /// Re-parents the tile to the board and makes it clickable again.
    /// </summary>
    public void ReturnTileToBoard(Tile tile, Action onComplete)
    {
        if (tile == null)
        {
            onComplete?.Invoke();
            return;
        }

        tile.transform.DOKill();
        tile.PrepareReturnToBoard();

        Sequence seq = DOTween.Sequence();
        seq.SetTarget(tile.transform);
        seq.SetId(TweenId);
        seq.SetLink(tile.gameObject);

        seq.Join(tile.transform.DOLocalMove(tile.RestLocalPosition, returnDuration).SetEase(returnEase));
        seq.Join(tile.transform.DOLocalRotateQuaternion(tile.RestLocalRotation, returnDuration).SetEase(Ease.OutQuad));
        seq.Join(tile.transform.DOScale(tile.BoardRestLocalScale, returnDuration).SetEase(Ease.OutQuad));

        seq.OnComplete(() =>
        {
            if (tile != null)
            {
                tile.RestoreBoardRestTransform();
                tile.transform.localScale = tile.BoardRestLocalScale;
            }
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// Shuffle booster: every tile flies to the middle of the board, spins, then spreads out to its NEW rest position.
    /// Call Tile.SetBoardPlacement on each tile first, so RestLocalPosition already holds the new spot.
    /// </summary>
    public void PlayShuffle(List<Tile> tiles, Action onComplete)
    {
        List<Tile> ordered = new List<Tile>();
        if (tiles != null)
        {
            foreach (Tile tile in tiles)
            {
                if (tile != null) ordered.Add(tile);
            }
        }

        if (ordered.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // Bottom layers spread out first.
        ordered.Sort((a, b) => a.LayerIndex.CompareTo(b.LayerIndex));

        Vector3 center = Vector3.zero;
        for (int i = 0; i < ordered.Count; i++) center += ordered[i].transform.localPosition;
        center /= ordered.Count;

        Vector3 spinAxis = shuffleSpinAxis.sqrMagnitude > 0.0001f ? shuffleSpinAxis.normalized : Vector3.forward;
        float stagger = Mathf.Min(shuffleSpreadStagger, shuffleMaxSpreadDelay / ordered.Count);
        float spinDuration = shuffleGatherDuration + shuffleHoldDuration;
        int remaining = ordered.Count;

        for (int i = 0; i < ordered.Count; i++)
        {
            Tile tile = ordered[i];
            tile.transform.DOKill();

            Vector3 gatherPosition = center + UnityEngine.Random.insideUnitSphere * shuffleGatherJitter;

            Sequence seq = DOTween.Sequence();
            seq.SetTarget(tile.transform);
            seq.SetId(TweenId);
            seq.SetLink(tile.gameObject);

            // 1. Gather in the middle while spinning a full turn
            seq.Append(tile.transform.DOLocalMove(gatherPosition, shuffleGatherDuration).SetEase(shuffleGatherEase));
            seq.Join(tile.transform.DOLocalRotate(spinAxis * 360f, spinDuration, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine));

            // 2. Hold, then spread to the new rest spot (staggered per tile)
            seq.AppendInterval(shuffleHoldDuration + i * stagger);
            seq.Append(tile.transform.DOLocalMove(tile.RestLocalPosition, shuffleSpreadDuration).SetEase(shuffleSpreadEase));
            seq.Join(tile.transform.DOLocalRotateQuaternion(tile.RestLocalRotation, shuffleSpreadDuration).SetEase(Ease.OutQuad));

            seq.OnComplete(() =>
            {
                if (tile != null) tile.RestoreBoardRestTransform();

                remaining--;
                if (remaining == 0)
                {
                    if (enableDebugLogs) Debug.Log("[ARRANGE] Shuffle COMPLETE.");
                    onComplete?.Invoke();
                }
            });
        }
    }

    public void KillAll()
    {
        DOTween.Kill(TweenId);
    }

    public void RestartViaGameManager()
    {
        if (gameManager != null)
            gameManager.RestartLevel();
        else
            Debug.LogError("[ARRANGE] Assign 'Game Manager' in TileArrangementManager Inspector to use the Restart button.");
    }
}