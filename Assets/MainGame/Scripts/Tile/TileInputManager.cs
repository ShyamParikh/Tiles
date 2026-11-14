using System;
using UnityEngine;

public class TileInputManager : MonoBehaviour
{
    public static Action<Tile> tileClicked;
    public static Action<Tile> blockedTileClicked;

    [Header("Scene References (Assign in Inspector)")]
    [Tooltip("REQUIRED: Drag your Main Camera here.")]
    [SerializeField] private Camera cam;
    [Tooltip("REQUIRED: Drag your TileGameManager object here.")]
    [SerializeField] private TileGameManager gameManager;
    [Tooltip("REQUIRED: Drag your TileBoardManager object here.")]
    [SerializeField] private TileBoardManager boardManager;
    [Tooltip("REQUIRED: Drag your TileArrangementManager object here (plays wiggle on blocked tiles).")]
    [SerializeField] private TileArrangementManager arrangementManager;
    [SerializeField] private Material selectOutlineMaterial;

    [Header("Raycast Settings")]
    [Tooltip("Set to Everything (or include your Tile prefab's layer).")]
    [SerializeField] private LayerMask tileLayers = ~0;
    [SerializeField] private float maxRayDistance = 100f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private Tile currentTile;

    private void Start()
    {
        if (cam == null)
            Debug.LogError("[INPUT] CRITICAL: 'Cam' is NOT assigned in TileInputManager Inspector!", this);

        if (gameManager == null)
            Debug.LogError("[INPUT] CRITICAL: 'Game Manager' is NOT assigned in TileInputManager Inspector!", this);

        if (arrangementManager == null)
            Debug.LogError("[INPUT] CRITICAL: 'Arrangement Manager' is NOT assigned in TileInputManager Inspector!", this);
    }

    private void Update()
    {
        if (!IsGamePlaying())
        {
            DeselectCurrentTile();
            return;
        }

        HandleControl();
    }

    private bool IsGamePlaying()
    {
        return gameManager != null && gameManager.IsPlaying;
    }

    private void HandleControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }
        else if (Input.GetMouseButton(0))
        {
            HandleMouseHover();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (currentTile != null && !currentTile.IsBlocked)
            {
                HandleMouseUp();
            }
            else
            {
                DeselectCurrentTile();
            }
        }
    }

    private void HandleMouseDown()
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, tileLayers))
        {
            if (enableDebugLogs && Physics.Raycast(ray, out RaycastHit anyHit, maxRayDistance, ~0, QueryTriggerInteraction.Collide))
            {
                int hitLayer = anyHit.collider.gameObject.layer;
                Debug.LogError($"[INPUT] LAYERMASK MISMATCH! Hit '{anyHit.collider.name}' on Layer '{LayerMask.LayerToName(hitLayer)}' ({hitLayer}). Set TileInputManager -> Tile Layers to Everything!");
            }
            DeselectCurrentTile();
            return;
        }

        Tile tile = Tile.FromCollider(hit.collider);
        if (tile == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning($"[INPUT] Hit '{hit.collider.name}' which is not a registered Tile collider.");
            DeselectCurrentTile();
            return;
        }

        // If covered (even slightly), do NOT select or move—play wiggle instead!
        if (tile.IsBlocked)
        {
            DeselectCurrentTile();
            if (enableDebugLogs)
            {
                string blockers = boardManager != null ? boardManager.GetBlockerNames(tile) : "Unknown";
                Debug.Log($"[INPUT] '{tile.name}' is covered by [{blockers}] -> Wiggling!");
            }

            if (arrangementManager != null)
                arrangementManager.PlayBlockedWiggle(tile);

            blockedTileClicked?.Invoke(tile);
            return;
        }

        SelectTile(tile);
    }

    private void HandleMouseHover()
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, tileLayers))
        {
            DeselectCurrentTile();
            return;
        }

        Tile tile = Tile.FromCollider(hit.collider);
        if (tile != null && !tile.IsBlocked)
        {
            SelectTile(tile);
        }
        else
        {
            DeselectCurrentTile();
        }
    }

    private void SelectTile(Tile tile)
    {
        if (currentTile == tile) return;
        DeselectCurrentTile();
        currentTile = tile;
        currentTile.Select(selectOutlineMaterial);
    }

    private void DeselectCurrentTile()
    {
        if (currentTile != null)
        {
            currentTile.Deselect();
            currentTile = null;
        }
    }

    private void HandleMouseUp()
    {
        Tile tile = currentTile;
        DeselectCurrentTile();

        if (tile == null || tile.IsBlocked) return;

        if (enableDebugLogs)
            Debug.Log($"[INPUT] >>> Moving playable tile '{tile.name}' (FaceId: {tile.FaceId})");

        tileClicked?.Invoke(tile);
    }
}