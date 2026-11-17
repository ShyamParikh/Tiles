using System;
using UnityEngine;

// Turns mouse / touch into tile clicks.
public class TileInputManager : MonoBehaviour
{
    public static Action<Tile> tileClicked;
    public static Action<Tile> blockedTileClicked;

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

    private Tile current;

    private void Update()
    {
        if (!gameManager.IsPlaying || trayManager.IsExternallyLocked)
        {
            Select(null);
            return;
        }

        if (Input.GetMouseButtonDown(0)) Press();
        else if (Input.GetMouseButton(0)) Hover();
        else if (Input.GetMouseButtonUp(0)) Release();
    }

    private Tile TileUnderPointer()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, tileLayers) ? Tile.FromCollider(hit.collider) : null;
    }

    // Pressing a covered tile wiggles it instead of selecting it.
    private void Press()
    {
        Tile tile = TileUnderPointer();
        if (tile != null && tile.IsBlocked)
        {
            Select(null);
            arrangementManager.PlayBlockedWiggle(tile);
            blockedTileClicked?.Invoke(tile);
            return;
        }
        Select(tile);
    }

    private void Hover()
    {
        Tile tile = TileUnderPointer();
        Select(tile != null && !tile.IsBlocked ? tile : null);
    }

    private void Release()
    {
        Tile tile = current;
        Select(null);
        if (tile != null && !tile.IsBlocked) tileClicked?.Invoke(tile);
    }

    private void Select(Tile tile)
    {
        if (tile == current) return;

        if (current != null) current.Deselect();
        current = tile;
        if (current != null) current.Select(selectOutlineMaterial);
    }
}