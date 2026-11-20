/*
 * TileCameraFitter
 * ----------------
 * Fits an orthographic camera to the board every time a level is built. It measures the bounds of all the tiles
 * (their body sprites) when the board is generated, then sets the camera's Orthographic Size so the whole board
 * fills the free part of the screen: few tiles zoom in, many tiles zoom out. The four margins say how much of
 * the screen the UI keeps for itself (top bar and tray, booster buttons, the sides), so the board is centered
 * in what is left. Padding adds a little space around the tiles. The camera glides to the new size (Fit
 * Duration, 0 = instant) and refits by itself if the screen shape changes. It assumes a 2D camera that looks
 * straight along Z. Anything that must keep its place on screen (the tray, the board frame, UI) has to follow the
 * camera: use a Screen Space - Camera canvas, or make it a child of the camera, otherwise it zooms with the board.
 */
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TileCameraFitter : MonoBehaviour
{
    #region Private Variables

    private const string TweenId = "TileCameraFit";

    [Tooltip("The orthographic camera that shows the board. Empty = the Main Camera.")]
    [SerializeField] private Camera cam;

    [Header("Board area (share of the screen the UI keeps for itself)")]
    [Range(0f, 0.9f)][SerializeField] private float leftMargin = 0.06f;
    [Range(0f, 0.9f)][SerializeField] private float rightMargin = 0.06f;
    [Tooltip("Top bar and tray.")]
    [Range(0f, 0.9f)][SerializeField] private float topMargin = 0.2f;
    [Tooltip("Booster buttons.")]
    [Range(0f, 0.9f)][SerializeField] private float bottomMargin = 0.17f;

    [Header("Fit")]
    [Tooltip("Extra space around the tiles, as a share of the board size.")]
    [SerializeField] private float padding = 0.05f;
    [Tooltip("The camera never zooms in closer than this Orthographic Size.")]
    [SerializeField] private float minSize = 0.1f;
    [Tooltip("The camera never zooms out further than this Orthographic Size.")]
    [SerializeField] private float maxSize = 10000f;
    [Tooltip("On = the camera also moves so the board sits in the middle of the free area.")]
    [SerializeField] private bool moveCamera = true;

    [Header("Animation")]
    [Tooltip("Seconds the camera takes to glide to the new size. 0 = instant.")]
    [SerializeField] private float fitDuration = 0.3f;
    [SerializeField] private Ease fitEase = Ease.OutQuad;

    private Bounds boardBounds;
    private bool hasBounds;
    private float lastAspect;
    private float startSize;
    private float targetSize;
    private Vector3 startPosition;
    private Vector3 targetPosition;

    #endregion

    #region Unity Callbacks

    // Starts listening for a new board.
    private void OnEnable()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
        TileVisualSpawner.boardGenerated += OnBoardGenerated;
    }

    // Stops listening and stops the glide.
    private void OnDisable()
    {
        TileVisualSpawner.boardGenerated -= OnBoardGenerated;
        DOTween.Kill(TweenId);
    }

    // Refits when the screen shape changes (rotating a phone, resizing the Game view).
    private void Update()
    {
        if (hasBounds && cam != null && !Mathf.Approximately(cam.aspect, lastAspect))
        {
            Fit(0f);
        }
    }

    #endregion

    #region Fitting

    // A new board exists: measure it and fit the camera.
    private void OnBoardGenerated(List<Tile> tiles)
    {
        hasBounds = false;
        foreach (Tile tile in tiles)
        {
            if (tile == null)
            {
                continue;
            }

            if (hasBounds)
            {
                boardBounds.Encapsulate(tile.WorldBounds);
            }
            else
            {
                boardBounds = tile.WorldBounds;
                hasBounds = true;
            }
        }

        if (hasBounds)
        {
            Fit(fitDuration);
        }
    }

    // Works out the size and position that fit the board into the free area, then glides there.
    private void Fit(float duration)
    {
        if (cam == null || !cam.orthographic)
        {
            return;
        }

        DOTween.Kill(TweenId);
        lastAspect = cam.aspect;

        // The free part of the screen, as a share of the whole view.
        float areaWidth = Mathf.Max(0.05f, 1f - leftMargin - rightMargin);
        float areaHeight = Mathf.Max(0.05f, 1f - topMargin - bottomMargin);

        // The view is 2 * size tall and 2 * size * aspect wide, so the board must fit its share of that.
        float boardScale = 1f + padding;
        float sizeForHeight = boardBounds.size.y * boardScale / (2f * areaHeight);
        float sizeForWidth = boardBounds.size.x * boardScale / (2f * cam.aspect * areaWidth);
        targetSize = Mathf.Clamp(Mathf.Max(sizeForHeight, sizeForWidth), minSize, maxSize);

        // Put the middle of the board at the middle of the free area.
        targetPosition = cam.transform.position;
        if (moveCamera)
        {
            float viewX = leftMargin + areaWidth * 0.5f;
            float viewY = bottomMargin + areaHeight * 0.5f;
            targetPosition.x = boardBounds.center.x - (viewX - 0.5f) * 2f * targetSize * cam.aspect;
            targetPosition.y = boardBounds.center.y - (viewY - 0.5f) * 2f * targetSize;
        }

        startSize = cam.orthographicSize;
        startPosition = cam.transform.position;

        if (duration <= 0f)
        {
            ApplyBlend(1f);
            return;
        }
        DOVirtual.Float(0f, 1f, duration, ApplyBlend).SetEase(fitEase).SetUpdate(true).SetId(TweenId).SetLink(gameObject);
    }

    // Moves the camera part of the way (0 = where it was, 1 = fitted).
    private void ApplyBlend(float blend)
    {
        cam.orthographicSize = Mathf.Lerp(startSize, targetSize, blend);
        cam.transform.position = Vector3.Lerp(startPosition, targetPosition, blend);
    }

    #endregion
}