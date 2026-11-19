/*
 * Tile
 * ----
 * The base tile, drawn with sprites. It knows its face id, its layer and cell, the tiles it covers and how many
 * tiles cover it, which tray slot it sits in, and how it looks. The body is a SpriteRenderer and the face is a
 * second SpriteRenderer in front of it. Looks are handled by TileSpriteLook: the body material and color come
 * from the prefab's inspector or from the level (a board look and a tray look), and covered tiles are dimmed.
 * Draw order comes from the sorting order: higher layers draw over lower ones, and a tile in the tray draws over
 * the whole board. A click finds its tile through FromCollider (a Collider2D). Special tiles (Cloudy, Question,
 * MergeBurst) inherit from this and only override the hooks at the bottom; they must not define Awake or
 * OnDestroy themselves. The tile does no movement: TileArrangementManager animates it, TileBoardManager writes
 * Covered / CoverCount, and TileTrayManager decides when it is picked up.
 */
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    #region Public Variables

    // The tiles lying under this one, and how many tiles cover it. TileBoardManager keeps these up to date.
    public readonly List<Tile> Covered = new List<Tile>();
    public int CoverCount;

    #endregion

    #region Private Variables

    // A raycast hits a collider, so this is how a click finds its tile.
    private static readonly Dictionary<Collider2D, Tile> byCollider = new Dictionary<Collider2D, Tile>();

    [Header("References")]
    [Tooltip("The SpriteRenderer that draws the tile body.")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [Tooltip("Optional: other sprite parts of the prefab that should get the color tint.")]
    [SerializeField] private List<SpriteRenderer> extraBodyRenderers = new List<SpriteRenderer>();
    [Tooltip("The SpriteRenderer that shows the face. Make it a child of the tile, in front of the body.")]
    [SerializeField] private SpriteRenderer faceRenderer;
    [Tooltip("The 2D collider that clicks hit.")]
    [SerializeField] private Collider2D col;

    [Header("Sorting")]
    [Tooltip("Sorting order of the lowest board layer. The body uses it, extra parts +1, the face +2.")]
    [SerializeField] private int baseSortingOrder;
    [Tooltip("How much higher each board layer draws. Keep it at 3 or more so the parts of two layers never mix.")]
    [SerializeField] private int sortingOrderPerLayer = 10;
    [Tooltip("Sorting order while the tile is in or flying to the tray. Above every board layer.")]
    [SerializeField] private int traySortingOrder = 500;

    [Header("Look on the board (a level can override these)")]
    [Tooltip("Replaces the body material. Empty = keep the prefab's own.")]
    [SerializeField] private Material bodyMaterialOverride;
    [SerializeField] private bool overrideBodyColor;
    [SerializeField] private Color bodyColor = Color.white;

    [Header("Covered look")]
    [SerializeField] private bool tintBodyWhenBlocked = true;
    [SerializeField] private Color blockedColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    [Header("Selection outline")]
    [Tooltip("The outline is a copy of the body sprite behind it, this much bigger. Use a flat sprite material for it.")]
    [SerializeField] private float outlineScale = 1.08f;

    private TileSpriteLook look;
    private int faceId;
    private int layerIndex;
    private Vector2 cellPosition;
    private bool isBlocked;
    private TraySlot slot;
    private Vector3 restPosition;
    private Quaternion restRotation;
    private Vector3 trayScale;
    private Vector3 boardScale = Vector3.one;
    private Transform boardParent;
    private GameObject outline;
    private TileLookSpec boardLook;   // what the level wants on the board
    private TileLookSpec trayLook;    // what the level wants once the tile is in the tray

    #endregion

    #region Properties

    public int FaceId => faceId;
    public int LayerIndex => layerIndex;
    public Vector2 CellPosition => cellPosition;
    public bool IsBlocked => isBlocked;
    public TraySlot Slot => slot;
    public Vector3 RestLocalPosition => restPosition;
    public Quaternion RestLocalRotation => restRotation;
    public Vector3 TrayScale => trayScale;
    public Vector3 BoardScale => boardScale;
    public TileSpot Spot => new TileSpot { layer = layerIndex, cell = cellPosition, position = restPosition, rotation = restRotation };

    private bool HasTrayLook => trayLook.material != null || trayLook.hasColor;
    private int BoardSortingOrder => baseSortingOrder + layerIndex * sortingOrderPerLayer;

    #endregion

    #region Static Helpers

    // Finds the tile that owns a collider (null if there is none).
    public static Tile FromCollider(Collider2D collider)
    {
        return collider != null && byCollider.ContainsKey(collider) ? byCollider[collider] : null;
    }

    // Clears the lookup when entering Play Mode without a domain reload.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        byCollider.Clear();
    }

    #endregion

    #region Unity Callbacks

    // Sets up the body look and registers the collider for clicks.
    protected virtual void Awake()
    {
        look = new TileSpriteLook(bodyRenderer, extraBodyRenderers, tintBodyWhenBlocked ? blockedColor : Color.white);
        ApplyBoardLook();

        if (col == null) Debug.LogError($"[TILE] '{name}' has no Collider2D assigned, so it can't be clicked.", this);
        else byCollider[col] = this;
    }

    // Unregisters the collider and removes the outline.
    private void OnDestroy()
    {
        if (col != null) byCollider.Remove(col);
        Deselect();
    }

    #endregion

    #region Setup And Look

    // Called by the spawner right after the tile is created.
    public void Init(int newFaceId, Sprite faceSprite, int layer, Vector2 cell)
    {
        faceId = newFaceId;
        layerIndex = layer;
        cellPosition = cell;
        boardParent = transform.parent;
        boardScale = transform.localScale;
        restPosition = transform.localPosition;
        restRotation = transform.localRotation;

        if (faceRenderer != null)
        {
            faceRenderer.sprite = faceSprite;
            faceRenderer.enabled = faceSprite != null;
        }

        ApplySorting(BoardSortingOrder);
        OnTileInitialized();
    }

    // What the level wants: a look for the board and (optionally) another one for the tray.
    public void SetLevelLooks(TileLookSpec board, TileLookSpec tray)
    {
        boardLook = board;
        trayLook = tray;
        ApplyBoardLook();
    }

    // The level's look wins. Without one, the prefab's own inspector settings apply.
    private void ApplyBoardLook()
    {
        bool fromLevel = boardLook.material != null;
        Material material = fromLevel ? boardLook.material : bodyMaterialOverride;

        look.ResetMaterial();
        if (material != null) look.SetMaterial(material);

        if (boardLook.hasColor) look.SetColor(boardLook.color);
        else if (overrideBodyColor) look.SetColor(bodyColor);
        else look.ClearColor();
    }

    // Applies the level's tray look once the tile is in a slot.
    private void ApplyTrayLook()
    {
        if (trayLook.material != null) look.SetMaterial(trayLook.material);
        if (trayLook.hasColor) look.SetColor(trayLook.color);
    }

    // Puts the body, the extra parts and the face on one sorting order (body lowest, face highest).
    private void ApplySorting(int order)
    {
        if (bodyRenderer != null) bodyRenderer.sortingOrder = order;

        foreach (SpriteRenderer extra in extraBodyRenderers)
        {
            if (extra != null) extra.sortingOrder = order + 1;
        }

        if (faceRenderer != null) faceRenderer.sortingOrder = order + 2;
    }

    #endregion

    #region Board And Tray State

    // Marks the tile as covered (dimmed) or free.
    public void SetBlocked(bool blocked)
    {
        isBlocked = blocked;
        if (faceRenderer != null) faceRenderer.color = blocked ? blockedColor : Color.white;
        look.SetBlocked(blocked);
        OnBlockedChanged(blocked);
    }

    // Puts the tile in a tray slot (or takes it out with null).
    public void SetSlot(TraySlot newSlot)
    {
        bool entering = slot == null && newSlot != null;
        slot = newSlot;
        if (!entering) return;

        ApplySorting(traySortingOrder);
        ApplyTrayLook();
        OnEnteredTray();
    }

    // The tile was picked up: it isn't covered or clickable any more.
    public void PrepareForTray()
    {
        SetBlocked(false);
        DisableInteraction();
    }

    // Undo: leave the tray slot and go back under the board, keeping the world position so it can fly back.
    public void PrepareReturnToBoard()
    {
        slot = null;
        transform.SetParent(boardParent, true);
        if (col != null) col.enabled = true;

        ApplySorting(BoardSortingOrder);
        if (HasTrayLook) ApplyBoardLook();
        OnReturnedToBoard();
    }

    // Gives the tile a new resting spot on the board (used by Shuffle).
    public void SetSpot(TileSpot spot)
    {
        layerIndex = spot.layer;
        cellPosition = spot.cell;
        restPosition = spot.position;
        restRotation = spot.rotation;

        if (slot == null) ApplySorting(BoardSortingOrder);
    }

    // Snaps the tile back to its exact board position, rotation and size.
    public void RestoreBoardTransform()
    {
        transform.localPosition = restPosition;
        transform.localRotation = restRotation;
        transform.localScale = boardScale;
    }

    // Remembers / restores the finished size of the tile inside the tray.
    public void RecordTrayScale() { trayScale = transform.localScale; }
    public void RestoreTrayScale() { transform.localScale = trayScale; }

    // Makes the tile unclickable.
    public void DisableInteraction()
    {
        if (col != null) col.enabled = false;
        Deselect();
    }

    #endregion

    #region Selection Outline

    // A slightly bigger copy of the body sprite behind it, with the outline material, shown while the pointer is over the tile.
    public void Select(Material outlineMaterial)
    {
        Deselect();
        if (outlineMaterial == null || bodyRenderer == null || bodyRenderer.sprite == null) return;

        outline = new GameObject(bodyRenderer.name + "_Outline");
        outline.transform.SetParent(bodyRenderer.transform, false);
        outline.transform.localScale = Vector3.one * outlineScale;

        SpriteRenderer outlineRenderer = outline.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = bodyRenderer.sprite;
        outlineRenderer.sharedMaterial = outlineMaterial;
        outlineRenderer.flipX = bodyRenderer.flipX;
        outlineRenderer.flipY = bodyRenderer.flipY;
        outlineRenderer.sortingLayerID = bodyRenderer.sortingLayerID;
        outlineRenderer.sortingOrder = bodyRenderer.sortingOrder - 1;
    }

    // Removes the outline.
    public void Deselect()
    {
        if (outline != null) Destroy(outline);
        outline = null;
    }

    #endregion

    #region Overridable Hooks

    // The tile was created and has its face.
    protected virtual void OnTileInitialized() { }

    // The tile became covered or free.
    protected virtual void OnBlockedChanged(bool blocked) { }

    // The tile started moving into a tray slot.
    protected virtual void OnEnteredTray() { }

    // Undo sent the tile back to the board.
    protected virtual void OnReturnedToBoard() { }

    // The set this tile belongs to started merging.
    public virtual void OnMergeStarted(List<Tile> group) { }

    #endregion
}