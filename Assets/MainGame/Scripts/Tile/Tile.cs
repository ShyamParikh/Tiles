using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The base tile: it knows its face, where it sits, whether it's covered, and how it looks.
// Special tiles (cloudy, question mark...) inherit from this and override the hooks below.
// Don't write Awake or OnDestroy in a subclass, the hooks are enough.
public class Tile : MonoBehaviour
{
    // A raycast hits a collider, so this is how a click finds its tile.
    private static readonly Dictionary<Collider, Tile> byCollider = new Dictionary<Collider, Tile>();

    public static Tile FromCollider(Collider collider)
    {
        return byCollider.ContainsKey(collider) ? byCollider[collider] : null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        byCollider.Clear();
    }

    [Header("References")]
    [Tooltip("The mesh child that draws the tile body.")]
    [SerializeField] private MeshFilter bodyMesh;
    [SerializeField] private MeshRenderer bodyRenderer;
    [Tooltip("Optional: other parts of the prefab that should get the color tint (they keep their own materials).")]
    [SerializeField] private List<MeshRenderer> extraBodyRenderers = new List<MeshRenderer>();
    [Tooltip("The UI image that shows the face.")]
    [SerializeField] private Image faceImage;
    [Tooltip("The collider that clicks hit.")]
    [SerializeField] private Collider col;

    [Header("Look on the board (a level can override these)")]
    [Tooltip("Replaces the body material. Empty = keep the prefab's own.")]
    [SerializeField] private Material bodyMaterialOverride;
    [Tooltip("-1 = every material slot, otherwise just that slot.")]
    [SerializeField] private int bodyMaterialSlot = -1;
    [SerializeField] private bool overrideBodyColor;
    [SerializeField] private Color bodyColor = Color.white;
    [SerializeField] private int bodyColorSlot = -1;

    [Header("Covered look")]
    [SerializeField] private bool tintBodyWhenBlocked = true;
    [SerializeField] private Color blockedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    [Tooltip("Color property of the body material. URP Lit = _BaseColor, built-in = _Color. Shader Graph: its Reference name (Exposed).")]
    [SerializeField] private string bodyColorProperty = "_BaseColor";

    // The tiles lying under this one, and how many tiles cover it. TileBoardManager keeps these up to date.
    public readonly List<Tile> Covered = new List<Tile>();
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
    private Vector3 boardScale = Vector3.one;
    private Transform boardParent;
    private GameObject outline;
    private TileLookSpec boardLook;   // what the level wants on the board
    private TileLookSpec trayLook;    // what the level wants once the tile is in the tray

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

    // ---------------- Hooks for special tiles ----------------

    protected virtual void OnTileInitialized() { }
    protected virtual void OnBlockedChanged(bool blocked) { }
    protected virtual void OnEnteredTray() { }
    protected virtual void OnReturnedToBoard() { }
    public virtual void OnMergeStarted(List<Tile> group) { }

    // ---------------- Unity ----------------

    protected virtual void Awake()
    {
        look = new TileBodyLook(bodyRenderer, extraBodyRenderers, bodyColorProperty, tintBodyWhenBlocked ? blockedColor : Color.white);
        ApplyBoardLook();

        if (col == null) Debug.LogError($"[TILE] '{name}' has no Collider assigned, so it can't be clicked.", this);
        else byCollider[col] = this;
    }

    private void OnDestroy()
    {
        if (col != null) byCollider.Remove(col);
        if (look != null) look.Release();
        Deselect();
    }

    // ---------------- Setup and look ----------------

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

        if (faceImage != null)
        {
            faceImage.sprite = faceSprite;
            faceImage.enabled = faceSprite != null;
        }
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
        if (material != null) look.SetMaterial(material, fromLevel ? boardLook.materialSlot : bodyMaterialSlot);

        if (boardLook.hasColor) look.SetColor(boardLook.color, boardLook.colorSlot);
        else if (overrideBodyColor) look.SetColor(bodyColor, bodyColorSlot);
        else look.ClearColor();
    }

    private void ApplyTrayLook()
    {
        if (trayLook.material != null) look.SetMaterial(trayLook.material, trayLook.materialSlot);
        if (trayLook.hasColor) look.SetColor(trayLook.color, trayLook.colorSlot);
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

    // A copy of the body mesh with the outline material, shown while the pointer is over the tile.
    public void Select(Material outlineMaterial)
    {
        Deselect();
        if (outlineMaterial == null || bodyRenderer == null || bodyMesh == null || bodyMesh.sharedMesh == null) return;

        outline = new GameObject(bodyRenderer.name + "_Outline");
        outline.transform.SetParent(bodyRenderer.transform, false);
        outline.AddComponent<MeshFilter>().sharedMesh = bodyMesh.sharedMesh;

        Material[] materials = new Material[bodyRenderer.sharedMaterials.Length];
        for (int i = 0; i < materials.Length; i++) materials[i] = outlineMaterial;

        MeshRenderer outlineRenderer = outline.AddComponent<MeshRenderer>();
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