using System;
using System.Collections.Generic;
using com.otm.manager;
using DG.Tweening;
using UnityEngine;

/// <summary>All tile movement animations: fall intro, tray moves, wiggle, undo return and shuffle.</summary>
public class TileArrangementManager : Manager<TileArrangementManager>
{
    private const string TweenId = "TileArrangement";

    [Header("Editor Testing (Optional)")]
    [SerializeField] private TileGameManager gameManager;

    [Header("1. Fall Intro")]
    [SerializeField] private float fallHeight = 8f;
    [Tooltip("Direction tiles fall from in board local space.")]
    [SerializeField] private Vector3 fallDirection = Vector3.up;
    [SerializeField] private float fallDuration = 0.45f;
    [SerializeField] private float delayBetweenTiles = 0.03f;
    [Tooltip("The whole fill-in never takes longer than this.")]
    [SerializeField] private float maxTotalDelay = 2.5f;
    [SerializeField] private Ease fallEase = Ease.OutBack;
    [SerializeField] private bool reverseRowOrder = false;

    [Header("2. Move To Tray Slot")]
    [SerializeField] private Vector3 tileLocalPositionOnSlot = Vector3.zero;
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private Ease moveEase = Ease.OutExpo;

    [Header("3. Arrival Double-Boop")]
    [SerializeField] private float firstBoopScale = 1.18f;
    [SerializeField] private float secondBoopScale = 1.09f;
    [SerializeField] private float singleBoopDuration = 0.12f;

    [Header("4. Tray Shift")]
    [SerializeField] private float shiftDuration = 0.2f;
    [SerializeField] private Ease shiftEase = Ease.OutQuad;

    [Header("5. Blocked Tile Wiggle")]
    [SerializeField] private float wiggleDuration = 0.32f;
    [SerializeField] private Vector3 wiggleRotationStrength = new Vector3(0f, 0f, 12f);
    [SerializeField] private Vector3 wigglePositionStrength = new Vector3(0.06f, 0f, 0f);
    [SerializeField] private int wiggleVibrato = 18;
    [SerializeField] private float wiggleRandomness = 45f;

    [Header("6. Return To Board (Undo)")]
    [SerializeField] private float returnDuration = 0.4f;
    [SerializeField] private Ease returnEase = Ease.OutBack;

    [Header("7. Shuffle")]
    [SerializeField] private float shuffleGatherDuration = 0.35f;
    [SerializeField] private Ease shuffleGatherEase = Ease.InOutQuad;
    [Tooltip("Small random offset so gathered tiles do not sit exactly on top of each other.")]
    [SerializeField] private float shuffleGatherJitter = 0.25f;
    [SerializeField] private float shuffleHoldDuration = 0.3f;
    [Tooltip("Spin axis in tile space while gathered. (0,0,1) for XY_Vertical boards, (0,1,0) for XZ_Flat.")]
    [SerializeField] private Vector3 shuffleSpinAxis = new Vector3(0f, 0f, 1f);
    [SerializeField] private float shuffleSpreadDuration = 0.4f;
    [SerializeField] private Ease shuffleSpreadEase = Ease.OutBack;
    [SerializeField] private float shuffleSpreadStagger = 0.01f;
    [SerializeField] private float shuffleMaxSpreadDelay = 0.5f;

    private Sequence NewSequence(Tile tile)
    {
        return DOTween.Sequence().SetTarget(tile.transform).Bind(TweenId, tile.gameObject);
    }

    public void PlayFallIntro(List<Tile> tiles, Action onComplete)
    {
        List<Tile> ordered = tiles.FindAll(t => t != null);
        if (ordered.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

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
            tile.gameObject.SetActive(false);
            tile.transform.localPosition = tile.RestLocalPosition + offset;

            tile.transform.DOLocalMove(tile.RestLocalPosition, fallDuration)
                .SetEase(fallEase)
                .SetDelay(i * delay)
                .Bind(TweenId, tile.gameObject)
                .OnStart(() => { if (tile != null) tile.gameObject.SetActive(true); })
                .OnComplete(() => { if (--remaining == 0) onComplete?.Invoke(); });
        }
    }

    /// <summary>Flies a new tile into its slot (with the arrival boops), or just shifts a tile already in the tray.</summary>
    public void MoveTileToSlot(Tile tile, TraySlot slot, Action onComplete)
    {
        if (tile == null || slot == null) return;

        bool inTray = tile.Slot != null;
        tile.transform.DOKill();

        if (inTray) tile.RestoreTrayScale();
        else tile.RestoreBoardTransform();

        tile.SetSlot(slot);
        tile.transform.SetParent(slot.TileParent, true);
        if (!inTray) tile.RecordTrayScale();

        if (inTray) Shift(tile, onComplete);
        else Fly(tile, onComplete);
    }

    private void Shift(Tile tile, Action onComplete)
    {
        tile.transform.DOLocalMove(tileLocalPositionOnSlot, shiftDuration)
            .SetEase(shiftEase)
            .Bind(TweenId, tile.gameObject)
            .OnComplete(() => Settle(tile, onComplete));
    }

    private void Fly(Tile tile, Action onComplete)
    {
        Vector3 scale = tile.TrayScale;
        float half = Mathf.Max(0.01f, singleBoopDuration * 0.5f);

        Sequence sequence = NewSequence(tile);
        sequence.Append(tile.transform.DOLocalMove(tileLocalPositionOnSlot, moveDuration).SetEase(moveEase));
        AppendBoop(sequence, tile.transform, scale, firstBoopScale, half);
        AppendBoop(sequence, tile.transform, scale, secondBoopScale, half);
        sequence.OnComplete(() => Settle(tile, onComplete));
    }

    private static void AppendBoop(Sequence sequence, Transform target, Vector3 scale, float multiplier, float half)
    {
        sequence.Append(target.DOScale(scale * multiplier, half).SetEase(Ease.OutQuad));
        sequence.Append(target.DOScale(scale, half).SetEase(Ease.InOutQuad));
    }

    private void Settle(Tile tile, Action onComplete)
    {
        if (tile != null)
        {
            tile.transform.localPosition = tileLocalPositionOnSlot;
            tile.RestoreTrayScale();
        }
        onComplete?.Invoke();
    }

    /// <summary>Shakes a covered tile that was clicked, then snaps it back to its spot.</summary>
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

    /// <summary>Undo: flies a tile from the tray back to its board spot.</summary>
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

    /// <summary>Shuffle: all tiles gather in the middle, spin, then spread to their new rest spots (call SetSpot first).</summary>
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

    public void KillAll()
    {
        DOTween.Kill(TweenId);
    }

    public void RestartViaGameManager()
    {
        if (gameManager != null) gameManager.RestartLevel();
        else Debug.LogError("[ARRANGE] Assign 'Game Manager' to use the Restart button.", this);
    }
}