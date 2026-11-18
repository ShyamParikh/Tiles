/*
 * TileArrangementManager
 * ----------------------
 * Plays every tile movement animation with DOTween: the fall-in when a level starts, flying into the tray
 * (the tile grows and turns into the tray pose while it travels, then boops twice on arrival), sliding to a
 * new slot, the wiggle of a covered tile, the undo flight back to the board, and the shuffle (gather, spin,
 * spread). All the "feel" numbers live in the seven inspector sections here, except the tray pose, which
 * the tray manager owns. Every tween is bound to one id, so KillAll cancels them (a restart does this).
 * It never decides what happens, it only animates: TileTrayManager, the boosters and TileGameManager call it.
 */
using System;
using System.Collections.Generic;
using com.otm.manager;
using DG.Tweening;
using UnityEngine;

public class TileArrangementManager : Manager<TileArrangementManager>
{
    #region Private Variables

    private const string TweenId = "TileArrangement";

    [Header("1. Fall intro")]
    [SerializeField] private float fallHeight = 8f;
    [Tooltip("The direction tiles fall from, in board space.")]
    [SerializeField] private Vector3 fallDirection = Vector3.up;
    [SerializeField] private float fallDuration = 0.45f;
    [SerializeField] private float delayBetweenTiles = 0.03f;
    [Tooltip("The whole fill-in never takes longer than this.")]
    [SerializeField] private float maxTotalDelay = 2.5f;
    [SerializeField] private Ease fallEase = Ease.OutBack;
    [SerializeField] private bool reverseRowOrder;

    [Header("2. Flying to the tray")]
    [SerializeField] private Vector3 tileLocalPositionOnSlot = Vector3.zero;
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private Ease moveEase = Ease.OutExpo;

    [Header("3. Arrival boops")]
    [SerializeField] private float firstBoopScale = 1.18f;
    [SerializeField] private float secondBoopScale = 1.09f;
    [SerializeField] private float singleBoopDuration = 0.12f;

    [Header("4. Tray shift")]
    [SerializeField] private float shiftDuration = 0.2f;
    [SerializeField] private Ease shiftEase = Ease.OutQuad;

    [Header("5. Covered tile wiggle")]
    [SerializeField] private float wiggleDuration = 0.32f;
    [SerializeField] private Vector3 wiggleRotationStrength = new Vector3(0f, 0f, 12f);
    [SerializeField] private Vector3 wigglePositionStrength = new Vector3(0.06f, 0f, 0f);
    [SerializeField] private int wiggleVibrato = 18;
    [SerializeField] private float wiggleRandomness = 45f;

    [Header("6. Undo return")]
    [SerializeField] private float returnDuration = 0.4f;
    [SerializeField] private Ease returnEase = Ease.OutBack;

    [Header("7. Shuffle")]
    [SerializeField] private float shuffleGatherDuration = 0.35f;
    [SerializeField] private Ease shuffleGatherEase = Ease.InOutQuad;
    [Tooltip("A small random offset so the gathered tiles don't sit exactly on top of each other.")]
    [SerializeField] private float shuffleGatherJitter = 0.25f;
    [SerializeField] private float shuffleHoldDuration = 0.3f;
    [Tooltip("Spin axis while gathered: (0,0,1) for vertical boards, (0,1,0) for flat ones.")]
    [SerializeField] private Vector3 shuffleSpinAxis = new Vector3(0f, 0f, 1f);
    [SerializeField] private float shuffleSpreadDuration = 0.4f;
    [SerializeField] private Ease shuffleSpreadEase = Ease.OutBack;
    [SerializeField] private float shuffleSpreadStagger = 0.01f;
    [Tooltip("The staggered spread never takes longer than this in total.")]
    [SerializeField] private float shuffleMaxSpreadDelay = 0.5f;

    #endregion

    #region Common

    // A new sequence that targets the tile and is bound to the shared tween id.
    private static Sequence NewSequence(Tile tile)
    {
        return DOTween.Sequence().SetTarget(tile.transform).Bind(TweenId, tile.gameObject);
    }

    // Cancels every tile animation (used when a level restarts).
    public void KillAll()
    {
        DOTween.Kill(TweenId);
    }

    #endregion

    #region Fall Intro

    // Every tile drops into place one after another, bottom layers first.
    public void PlayFallIntro(List<Tile> tiles, Action onComplete)
    {
        List<Tile> ordered = tiles.FindAll(t => t != null);
        if (ordered.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // Bottom layers first, then row by row.
        float rowSign = reverseRowOrder ? -1f : 1f;
        ordered.Sort((a, b) =>
        {
            int result = a.LayerIndex.CompareTo(b.LayerIndex);
            if (result == 0) result = (rowSign * a.CellPosition.y).CompareTo(rowSign * b.CellPosition.y);
            return result != 0 ? result : a.CellPosition.x.CompareTo(b.CellPosition.x);
        });

        Vector3 offset = (fallDirection.sqrMagnitude > 0.0001f ? fallDirection.normalized : Vector3.up) * fallHeight;
        float delay = Mathf.Min(delayBetweenTiles, maxTotalDelay / ordered.Count);
        int remaining = ordered.Count;

        for (int i = 0; i < ordered.Count; i++)
        {
            Tile tile = ordered[i];
            tile.gameObject.SetActive(false);   // stays hidden until its own turn to fall
            tile.transform.localPosition = tile.RestLocalPosition + offset;

            tile.transform.DOLocalMove(tile.RestLocalPosition, fallDuration)
                .SetEase(fallEase)
                .SetDelay(i * delay)
                .Bind(TweenId, tile.gameObject)
                .OnStart(() => { if (tile != null) tile.gameObject.SetActive(true); })
                .OnComplete(() => { if (--remaining == 0) onComplete?.Invoke(); });
        }
    }

    #endregion

    #region Tray Movement

    // A tile new to the tray flies in and grows / turns into the tray pose on the way.
    // A tile already in the tray just slides over to its new slot.
    public void MoveTileToSlot(Tile tile, TraySlot slot, TrayPose pose, Action onComplete)
    {
        if (tile == null || slot == null) return;

        bool inTray = tile.Slot != null;
        tile.transform.DOKill();

        if (inTray) tile.RestoreTrayScale();
        else tile.RestoreBoardTransform();

        tile.SetSlot(slot);
        tile.transform.SetParent(slot.TileParent, true);

        if (inTray)
        {
            Shift(tile, onComplete);
            return;
        }

        // Remember the finished tray size, then go back to the starting size so the tween can grow into it.
        Vector3 startScale = tile.transform.localScale;
        tile.transform.localScale = Vector3.Scale(startScale, pose.scale);
        tile.RecordTrayScale();
        tile.transform.localScale = startScale;

        Fly(tile, pose, onComplete);
    }

    // Slides a tile that is already in the tray to its new slot.
    private void Shift(Tile tile, Action onComplete)
    {
        tile.transform.DOLocalMove(tileLocalPositionOnSlot, shiftDuration)
            .SetEase(shiftEase)
            .Bind(TweenId, tile.gameObject)
            .OnComplete(() => Settle(tile, onComplete));
    }

    // Flies a new tile into its slot: move + pose together, then two boops.
    private void Fly(Tile tile, TrayPose pose, Action onComplete)
    {
        Vector3 scale = tile.TrayScale;   // the finished size, pose included
        float half = Mathf.Max(0.01f, singleBoopDuration * 0.5f);

        Sequence sequence = NewSequence(tile);
        sequence.Append(tile.transform.DOLocalMove(tileLocalPositionOnSlot, moveDuration).SetEase(moveEase));

        // The size and rotation change while the tile travels, not after it arrives.
        sequence.Join(tile.transform.DOScale(scale, pose.duration).SetEase(pose.ease));
        if (pose.setRotation)
            sequence.Join(tile.transform.DOLocalRotateQuaternion(Quaternion.Euler(pose.rotation), pose.duration).SetEase(pose.ease));

        AppendBoop(sequence, tile.transform, scale, firstBoopScale, half);
        AppendBoop(sequence, tile.transform, scale, secondBoopScale, half);
        sequence.OnComplete(() => Settle(tile, onComplete));
    }

    // One boop: grow a little, then return to the normal size.
    private static void AppendBoop(Sequence sequence, Transform target, Vector3 scale, float multiplier, float half)
    {
        sequence.Append(target.DOScale(scale * multiplier, half).SetEase(Ease.OutQuad));
        sequence.Append(target.DOScale(scale, half).SetEase(Ease.InOutQuad));
    }

    // Snaps to the exact final spot so nothing drifts after the tweens.
    private void Settle(Tile tile, Action onComplete)
    {
        if (tile != null)
        {
            tile.transform.localPosition = tileLocalPositionOnSlot;
            tile.RestoreTrayScale();
        }
        onComplete?.Invoke();
    }

    #endregion

    #region Wiggle And Undo

    // A covered tile was clicked: shake it, then snap it back to its spot.
    public void PlayBlockedWiggle(Tile tile)
    {
        if (tile == null) return;

        tile.transform.DOKill();
        tile.RestoreBoardTransform();

        Sequence sequence = NewSequence(tile);
        if (wiggleRotationStrength.sqrMagnitude > 0.0001f)
            sequence.Join(tile.transform.DOShakeRotation(wiggleDuration, wiggleRotationStrength, wiggleVibrato, wiggleRandomness, true));
        if (wigglePositionStrength.sqrMagnitude > 0.0001f)
            sequence.Join(tile.transform.DOShakePosition(wiggleDuration, wigglePositionStrength, wiggleVibrato, wiggleRandomness, false, true));

        sequence.OnComplete(() => { if (tile != null) tile.RestoreBoardTransform(); });
    }

    // Undo: flies a tile from the tray back to its board spot, with its original size and rotation.
    public void ReturnTileToBoard(Tile tile, Action onComplete)
    {
        tile.transform.DOKill();
        tile.PrepareReturnToBoard();

        Sequence sequence = NewSequence(tile);
        sequence.Join(tile.transform.DOLocalMove(tile.RestLocalPosition, returnDuration).SetEase(returnEase));
        sequence.Join(tile.transform.DOLocalRotateQuaternion(tile.RestLocalRotation, returnDuration).SetEase(Ease.OutQuad));
        sequence.Join(tile.transform.DOScale(tile.BoardScale, returnDuration).SetEase(Ease.OutQuad));
        sequence.OnComplete(() =>
        {
            if (tile != null) tile.RestoreBoardTransform();
            onComplete?.Invoke();
        });
    }

    #endregion

    #region Shuffle

    // Shuffle: everything gathers in the middle, spins, then spreads to its new spot (call SetSpot on the tiles first).
    public void PlayShuffle(List<Tile> tiles, Action onComplete)
    {
        List<Tile> ordered = tiles.FindAll(t => t != null);
        if (ordered.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }
        ordered.Sort((a, b) => a.LayerIndex.CompareTo(b.LayerIndex));

        Vector3 center = Vector3.zero;
        foreach (Tile tile in ordered) center += tile.transform.localPosition;
        center /= ordered.Count;

        Vector3 spin = (shuffleSpinAxis.sqrMagnitude > 0.0001f ? shuffleSpinAxis.normalized : Vector3.forward) * 360f;
        float stagger = Mathf.Min(shuffleSpreadStagger, shuffleMaxSpreadDelay / ordered.Count);
        int remaining = ordered.Count;

        for (int i = 0; i < ordered.Count; i++)
        {
            Tile tile = ordered[i];
            tile.transform.DOKill();

            Sequence sequence = NewSequence(tile);
            sequence.Append(tile.transform.DOLocalMove(center + UnityEngine.Random.insideUnitSphere * shuffleGatherJitter, shuffleGatherDuration).SetEase(shuffleGatherEase));
            sequence.Join(tile.transform.DOLocalRotate(spin, shuffleGatherDuration + shuffleHoldDuration, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine));
            sequence.AppendInterval(shuffleHoldDuration + i * stagger);
            sequence.Append(tile.transform.DOLocalMove(tile.RestLocalPosition, shuffleSpreadDuration).SetEase(shuffleSpreadEase));
            sequence.Join(tile.transform.DOLocalRotateQuaternion(tile.RestLocalRotation, shuffleSpreadDuration).SetEase(Ease.OutQuad));
            sequence.OnComplete(() =>
            {
                if (tile != null) tile.RestoreBoardTransform();
                if (--remaining == 0) onComplete?.Invoke();
            });
        }
    }

    #endregion
}