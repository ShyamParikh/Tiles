/*
 * TileInputManager
 * ----------------
 * Turns mouse or touch into tile clicks. Pressing a covered tile wiggles it, hovering shows the selection
 * outline, and releasing on a free tile fires tileClicked (TileTrayManager listens). Input is ignored unless the
 * game is Playing and no booster or tile effect has locked the tray. Tiles are found with a 2D physics
 * raycast against their Collider2D, so the camera, layer mask and ray distance are set here (the first collider
 * along the ray wins, so layers need a different Z). Fires blockedTileClicked for sounds. The pointer ray can be
 * drawn for debugging (Scene view, or Game view with Gizmos on): green = hit a tile-layer collider, yellow =
 * hit something outside the tile layers, red = hit nothing.
 */
using System;
using UnityEngine;

public class TileInputManager : MonoBehaviour
{
    #region Public Variables

    public static Action<Tile> tileClicked;
    public static Action<Tile> blockedTileClicked;

    #endregion

    #region Private Variables

    [Header("Scene references")]
    [SerializeField] private Camera cam;
    [SerializeField] private TileGameManager gameManager;
    [Tooltip("Input is ignored while a booster or tile effect has the tray locked.")]
    [SerializeField] private TileTrayManager trayManager;
    [SerializeField] private TileArrangementManager arrangementManager;
    [SerializeField] private Material selectOutlineMaterial;

    [Header("Raycast")]
    [SerializeField] private LayerMask tileLayers = ~0;
    [SerializeField] private float maxRayDistance = 100f;

    [Header("Ray debug")]
    [Tooltip("Draws the pointer ray every frame.")]
    [SerializeField] private bool drawRayEveryFrame = true;
    [Tooltip("Draws the ray of a press and of a release, and keeps it visible for the time below.")]
    [SerializeField] private bool drawRayOnPressAndRelease = true;
    [Tooltip("How long (seconds) the ray from a press or release stays visible.")]
    [SerializeField] private float eventRayDuration = 3f;

    private Tile current;

    #endregion

    #region Unity Callbacks

    // Reads the pointer every frame while the game is playable.
    private void Update()
    {
        if (drawRayEveryFrame)
        {
            DrawPointerRay(0f);
        }

        if (!gameManager.IsPlaying || trayManager.IsExternallyLocked)
        {
            Select(null);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Press();
        }
        else if (Input.GetMouseButton(0))
        {
            Hover();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Release();
        }
    }

    #endregion

    #region Pointer Handling

    // Pressing a covered tile wiggles it instead of selecting it.
    private void Press()
    {
        if (drawRayOnPressAndRelease)
        {
            DrawPointerRay(eventRayDuration);
        }

        Tile tile = TileUnderPointer();
        if (tile != null && tile.IsBlocked)
        {
            Select(null);
            arrangementManager.PlayBlockedWiggle(tile);
            if (blockedTileClicked != null)
            {
                blockedTileClicked.Invoke(tile);
            }
            return;
        }
        Select(tile);
    }

    // Moving while pressed keeps the free tile under the pointer selected.
    private void Hover()
    {
        Tile tile = TileUnderPointer();
        if (tile != null && !tile.IsBlocked)
        {
            Select(tile);
        }
        else
        {
            Select(null);
        }
    }

    // Releasing on a free tile is a click.
    private void Release()
    {
        if (drawRayOnPressAndRelease)
        {
            DrawPointerRay(eventRayDuration);
        }

        Tile tile = current;
        Select(null);
        if (tile != null && !tile.IsBlocked && tileClicked != null)
        {
            tileClicked.Invoke(tile);
        }
    }

    // The tile under the pointer (null if there is none).
    private Tile TileUnderPointer()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, maxRayDistance, tileLayers);
        if (hit.collider != null)
        {
            return Tile.FromCollider(hit.collider);
        }
        return null;
    }

    // Changes which tile shows the selection outline.
    private void Select(Tile tile)
    {
        if (tile == current)
        {
            return;
        }

        if (current != null)
        {
            current.Deselect();
        }
        current = tile;
        if (current != null)
        {
            current.Select(selectOutlineMaterial);
        }
    }

    #endregion

    #region Ray Debug

    // Draws the pointer ray. Green up to a tile-layer hit, yellow up to a hit outside the tile layers, red for the rest.
    private void DrawPointerRay(float lineDuration)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 rayEnd = ray.origin + ray.direction * maxRayDistance;

        // The first collider on any layer, and the first one on the tile layers (what the game really uses).
        RaycastHit2D anyHit = Physics2D.GetRayIntersection(ray, maxRayDistance, Physics2D.AllLayers);
        RaycastHit2D tileLayerHit = Physics2D.GetRayIntersection(ray, maxRayDistance, tileLayers);

        if (tileLayerHit.collider != null)
        {
            Vector3 point = ray.GetPoint(tileLayerHit.distance);
            Debug.DrawLine(ray.origin, point, Color.green, lineDuration);
            Debug.DrawLine(point, rayEnd, Color.red, lineDuration);
        }
        else if (anyHit.collider != null)
        {
            Vector3 point = ray.GetPoint(anyHit.distance);
            Debug.DrawLine(ray.origin, point, Color.yellow, lineDuration);
            Debug.DrawLine(point, rayEnd, Color.red, lineDuration);
        }
        else
        {
            Debug.DrawLine(ray.origin, rayEnd, Color.red, lineDuration);
        }
    }

    #endregion
}