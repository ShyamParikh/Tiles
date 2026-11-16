using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base tile: identity, board / tray state and body look. Subclasses add behaviour by overriding the hooks
/// (they must not define Awake or OnDestroy themselves).
/// </summary>
public class Tile : MonoBehaviour
{
    private static readonly Dictionary<Collider, Tile> tilesByCollider = new Dictionary<Collider, Tile>();

    public static Tile FromCollider(Collider collider)
    {
        return tilesByCollider.ContainsKey(collider) ? tilesByCollider[collider] : null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        tilesByCollider.Clear();
    }

    [Header("Visual References (Assign in Inspector)")]
    [Tooltip("The RoundedCube child object (the one with the MeshFilter).")]
    [SerializeField] private MeshFilter bodyMesh;
    [Tooltip("The MeshRenderer on that same RoundedCube child.")]
    [SerializeField] private MeshRenderer bodyRenderer;
    [Tooltip("Optional: other renderers of the prefab. They get the color tint but keep their own materials.")]
    [SerializeField] private List<MeshRenderer> extraBodyRenderers = new List<MeshRenderer>();
    [Tooltip("The UI Image on the Canvas child. The face sprite is drawn here.")]
    [SerializeField] private Image faceImage;

    [Header("Interaction (Assign in Inspector)")]
    [Tooltip("REQUIRED: the collider used for clicking.")]
    [SerializeField] private Collider col;

    [Header("Body Look (Prefab Defaults - levels can override these)")]
    [Tooltip("Optional: replaces the body material. Leave empty to keep the prefab's own.")]
    [SerializeField] private Material bodyMaterialOverride;
    [Tooltip("-1 = replace ALL material slots. 0 = Element 0 only, etc.")]
    [SerializeField] private int bodyMaterialSlot = -1;
    [SerializeField] private bool overrideBodyColor = false;
    [SerializeField] private Color bodyColor = Color.white;
    [Tooltip("-1 = tint ALL material slots. 0 = Element 0 only, etc.")]
    [SerializeField] private int bodyColorSlot = -1;

    [Header("Color Mode")]
    [Tooltip("MaterialInstance: every tinted tile gets its own material copy, so colors can never be shared. PropertyBlock: lighter, but some shaders / pipelines ignore it.")]
    [SerializeField] private TileColorMode colorMode = TileColorMode.MaterialInstance;

    [Header("Blocked Look (tile covered by another tile)")]
    [SerializeField] private bool tintBodyWhenBlocked = true;
    [SerializeField] private Color blockedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    [Tooltip("Color property of the body material. URP Lit = _BaseColor, Built-in = _Color. Shader Graph: the Reference name (Exposed).")]
    [SerializeField] private string bodyColorProperty = "_BaseColor";

    /// <summary>Tiles lying under this one. Written by TileBoardManager.</summary>
    public readonly List<Tile> Covered = new List<Tile>();
    /// <summary>How many tiles currently cover this one. Written by TileBoardManager.</summary>
    public int CoverCount;

    private TileBodyLook look;
    private int faceId;
    private int layerIndex;
    private Vector2 cellPosition;
    private bool isBlocked;
    private TraySlot slot;
    private Vector3 restPosition;
    private Quaternion restRotation;
    private Vector3 trayScale;
    private Transform boardParent;
    private Vector3 boardScale = Vector3.one;
    private GameObject outline;
    private TileLookSpec boardLook;   // from the level: what the tile looks like on the board
    private TileLookSpec trayLook;    // from the level: what the tile changes to in the tray

    public int FaceId => faceId;
    public Sprite FaceSprite => faceImage != null ? faceImage.sprite : null;
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

    // ---------------- Hooks for subclasses ----------------

    protected virtual void OnTileInitialized() { }
    protected virtual void OnBlockedChanged(bool blocked) { }
    protected virtual void OnEnteredTray() { }
    protected virtual void OnReturnedToBoard() { }
    protected virtual void OnMergeStarted(List<Tile> group) { }

    public void NotifyMergeStarted(List<Tile> group)
    {
        OnMergeStarted(group);
    }

    // ---------------- Unity ----------------

    protected virtual void Awake()
    {
        look = new TileBodyLook(bodyRenderer, extraBodyRenderers, bodyColorProperty, colorMode, tintBodyWhenBlocked ? blockedColor : Color.white);
        ApplyBoardLook();

        if (col == null) Debug.LogError($"[TILE] '{name}' has no Collider assigned. It cannot be clicked.", this);
        else tilesByCollider[col] = this;
    }

    private void OnValidate()
    {
        if (Application.isPlaying && look != null && slot == null) ApplyBoardLook();
    }

    private void OnDestroy()
    {
        if (col != null) tilesByCollider.Remove(col);
        if (look != null) look.Release();
        Deselect();
    }

    // ---------------- Look ----------------

    /// <summary>Called by the spawner with what the level wants: the board look and the (optional) tray look.</summary>
    public void SetLevelLooks(TileLookSpec board, TileLookSpec tray)
    {
        boardLook = board;
        trayLook = tray;
        ApplyBoardLook();
    }

    /// <summary>Material and color on the board: the level's override first, then the prefab's own inspector look.</summary>
    private void ApplyBoardLook()
    {
        bool levelMaterial = boardLook.material != null;
        Material material = levelMaterial ? boardLook.material : bodyMaterialOverride;

        look.ResetMaterial();
        if (material != null) look.SetMaterial(material, levelMaterial ? boardLook.materialSlot : bodyMaterialSlot);

        if (boardLook.hasColor) look.SetColor(boardLook.color, boardLook.colorSlot);
        else if (overrideBodyColor) look.SetColor(bodyColor, bodyColorSlot);
        else look.ClearColor();
    }

    private void ApplyTrayLook()
    {
        if (trayLook.material != null) look.SetMaterial(trayLook.material, trayLook.materialSlot);
        if (trayLook.hasColor) look.SetColor(trayLook.color, trayLook.colorSlot);
    }

    // ---------------- Setup ----------------

    public void Init(int newFaceId, Sprite faceSprite, int layer, Vector2 cell)
    {
        faceId = newFaceId;
        layerIndex = layer;
        cellPosition = cell;
        boardParent = transform.parent;
        boardScale = transform.localScale;
        restPosition = transform.localPosition;
        restRotation = transform.localRotation;

        if (faceImage != null)
        {
            faceImage.sprite = faceSprite;
            faceImage.enabled = faceSprite != null;
        }
        OnTileInitialized();
    }

    // ---------------- Board and tray state ----------------

    public void SetBlocked(bool blocked)
    {
        isBlocked = blocked;
        if (faceImage != null) faceImage.color = blocked ? blockedColor : Color.white;
        look.SetBlocked(blocked);
        OnBlockedChanged(blocked);
    }

    public void SetSlot(TraySlot newSlot)
    {
        bool entering = slot == null && newSlot != null;
        slot = newSlot;
        if (!entering) return;

        ApplyTrayLook();
        OnEnteredTray();
    }

    /// <summary>Called when the tile is picked up: no longer covered, not clickable, no shadow.</summary>
    public void PrepareForTray()
    {
        SetBlocked(false);
        DisableInteraction();
        look.SetShadows(false);
    }

    /// <summary>Undo: leaves the tray slot and goes back under the board parent, keeping its world position.</summary>
    public void PrepareReturnToBoard()
    {
        slot = null;
        transform.SetParent(boardParent, true);
        if (col != null) col.enabled = true;
        look.SetShadows(true);

        if (HasTrayLook) ApplyBoardLook();
        OnReturnedToBoard();
    }

    public void SetSpot(TileSpot spot)
    {
        layerIndex = spot.layer;
        cellPosition = spot.cell;
        restPosition = spot.position;
        restRotation = spot.rotation;
    }

    public void RestoreBoardTransform()
    {
        transform.localPosition = restPosition;
        transform.localRotation = restRotation;
        transform.localScale = boardScale;
    }

    public void RecordTrayScale() { trayScale = transform.localScale; }
    public void RestoreTrayScale() { transform.localScale = trayScale; }

    public void DisableInteraction()
    {
        if (col != null) col.enabled = false;
        Deselect();
    }

    // ---------------- Selection outline ----------------

    public void Select(Material outlineMaterial)
    {
        Deselect();
        if (outlineMaterial == null || bodyRenderer == null || bodyMesh == null || bodyMesh.sharedMesh == null) return;

        outline = new GameObject(bodyRenderer.name + "_Outline");
        outline.transform.SetParent(bodyRenderer.transform, false);
        outline.AddComponent<MeshFilter>().sharedMesh = bodyMesh.sharedMesh;

        MeshRenderer outlineRenderer = outline.AddComponent<MeshRenderer>();
        Material[] materials = new Material[bodyRenderer.sharedMaterials.Length];
        for (int i = 0; i < materials.Length; i++) materials[i] = outlineMaterial;

        outlineRenderer.sharedMaterials = materials;
        outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        outlineRenderer.receiveShadows = false;
    }

    public void Deselect()
    {
        if (outline != null) Destroy(outline);
        outline = null;
    }
}