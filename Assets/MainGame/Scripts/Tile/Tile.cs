using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TileColorMode
{
    MaterialInstance,   // each tinted tile gets its own material copy (always independent colors)
    PropertyBlock       // lighter, but some shaders / pipelines ignore it
}

/// <summary>
/// BASE tile. Put this on the ROOT of your tile prefab (TilePrefab3D), or use one of the subclasses
/// (NormalTile, CloudyTile, QuestionTile, MergeBurstTile) which add behaviour through the virtual hooks below.
/// Assign all visual and collider references directly in the Inspector.
/// </summary>
public class Tile : MonoBehaviour
{
    private static readonly Dictionary<Collider, Tile> tilesByCollider = new Dictionary<Collider, Tile>();

    private static readonly string[] FallbackColorProperties =
    {
        "_BaseColor",
        "_Color",
        "_TintColor",
        "_MainColor"
    };

    public static Tile FromCollider(Collider collider)
    {
        if (collider == null) return null;
        tilesByCollider.TryGetValue(collider, out Tile tile);
        return tile;
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
    [Tooltip("Optional: Any additional MeshRenderers on child parts of the tile prefab. They get the color tint, but keep their own materials.")]
    [SerializeField] private List<MeshRenderer> extraBodyRenderers = new List<MeshRenderer>();
    [Tooltip("The UI Image on the Canvas child (ObjectImage). The face sprite is drawn here.")]
    [SerializeField] private Image faceImage;

    [Header("Interaction (Assign in Inspector)")]
    [Tooltip("REQUIRED: Drag the BoxCollider on this tile prefab here.")]
    [SerializeField] private Collider col;

    [Header("Body Look (Prefab Defaults - levels can override these)")]
    [Tooltip("Optional: replaces the body material on the Body Renderer. Leave empty to keep the prefab's own material.")]
    [SerializeField] private Material bodyMaterialOverride;
    [Tooltip("-1 = replace ALL material slots on the body mesh. 0 = Element 0 only, 1 = Element 1 only, etc.")]
    [SerializeField] private int bodyMaterialSlot = -1;
    [Tooltip("On = tints the tile body with Body Color.")]
    [SerializeField] private bool overrideBodyColor = false;
    [SerializeField] private Color bodyColor = Color.white;
    [Tooltip("-1 = tint ALL material slots. 0 = Element 0 only, 1 = Element 1 only, etc.")]
    [SerializeField] private int bodyColorSlot = -1;

    [Header("Color Mode")]
    [Tooltip("MaterialInstance (recommended): every tinted tile gets its own copy of the material, so tiles can never share a color. " +
             "PropertyBlock: lighter on memory/batching, but some shaders / render pipelines ignore it.")]
    [SerializeField] private TileColorMode colorMode = TileColorMode.MaterialInstance;

    [Header("Blocked Look (tile covered by another tile)")]
    [Tooltip("On = dims each material slot using its active color when covered. Off = keeps active color un-dimmed.")]
    [SerializeField] private bool tintBodyWhenBlocked = true;
    [SerializeField] private Color blockedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    [Tooltip("Primary color property of the body material. URP Lit = _BaseColor, Built-in Standard = _Color. For Shader Graph use the property's Reference name (and tick Exposed).")]
    [SerializeField] private string bodyColorProperty = "_BaseColor";

    private class RendererSlotCache
    {
        public MeshRenderer renderer;
        public Material[] baseMaterials;     // the shared (original) materials of this renderer
        public Material[] instances;         // per-tile copies, only created in MaterialInstance mode when needed
        public string[] colorProperties;
        public Color[] originalColors;
        public Color[] activeColors;
        public bool[] hasColor;
    }

    private readonly List<RendererSlotCache> rendererCaches = new List<RendererSlotCache>();

    private int faceId;
    private int layerIndex;
    private Vector2 cellPosition;
    private bool isBlocked;
    private TraySlot slot;

    // The real face sprite, and the optional cover (cloud / question mark) drawn instead of it.
    private Sprite realFaceSprite;
    private Sprite coverSprite;
    private bool faceCovered;

    private bool hasCustomColor;
    private Color customColor = Color.white;
    private int customColorSlot = -1;
    private bool warnedNoColorProperty;

    private Material[] originalBodyMaterials;
    private bool inspectorMaterialApplied;
    private bool inspectorColorApplied;

    private Vector3 restLocalPosition;
    private Quaternion restLocalRotation;
    private bool hasRecordedRestTransform;

    private Vector3 trayRestLocalScale;
    private bool hasRecordedTrayScale;

    // Remembered so a tile can go back to the board (Undo booster).
    private Transform boardParent;
    private Vector3 boardRestLocalScale = Vector3.one;

    private UnityEngine.Rendering.ShadowCastingMode originalShadowMode = UnityEngine.Rendering.ShadowCastingMode.On;

    private MaterialPropertyBlock propertyBlock;
    private GameObject outlineObject;

    public int FaceId => faceId;
    /// <summary>The tile's real face sprite (even while a cover is shown on top of it).</summary>
    public Sprite FaceSprite => realFaceSprite;
    public bool IsFaceCovered => faceCovered;
    public int LayerIndex => layerIndex;
    public Vector2 CellPosition => cellPosition;
    public bool IsBlocked => isBlocked;
    public TraySlot Slot => slot;
    public Collider TileCollider => col;
    public Vector3 RestLocalPosition => restLocalPosition;
    public Quaternion RestLocalRotation => restLocalRotation;
    public Vector3 TrayRestLocalScale => hasRecordedTrayScale ? trayRestLocalScale : transform.localScale;
    public Vector3 BoardRestLocalScale => boardRestLocalScale;
    public Transform BoardParent => boardParent;

    public bool HasBodyMesh => bodyMesh != null && bodyMesh.sharedMesh != null;

    // ---------------- Hooks for special tiles (override these in subclasses) ----------------

    /// <summary>Called at the end of Init (tile spawned on the board, face assigned).</summary>
    protected virtual void OnTileInitialized() { }

    /// <summary>Called every time the board manager updates whether this tile is covered.</summary>
    protected virtual void OnBlockedChanged(bool blocked) { }

    /// <summary>Called when the tile starts moving into a tray slot (first time it gets a slot).</summary>
    protected virtual void OnEnteredTray() { }

    /// <summary>Called when the Undo booster sends the tile back from the tray to the board.</summary>
    protected virtual void OnReturnedToBoard() { }

    // ---------------- Unity ----------------

    protected virtual void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();

        if (bodyRenderer != null)
        {
            originalShadowMode = bodyRenderer.shadowCastingMode;
            originalBodyMaterials = bodyRenderer.sharedMaterials;
        }

        BuildRendererCaches();
        ApplyInspectorLook();

        if (col != null)
        {
            tilesByCollider[col] = this;
        }
        else
        {
            Debug.LogError($"[TILE] '{name}' has NO Collider assigned in the 'Col' Inspector slot! It cannot be clicked.", this);
        }
    }

    /// <summary>Lets you tweak the Body Look fields live in Play Mode and see the result on spawned tiles.</summary>
    private void OnValidate()
    {
        if (!Application.isPlaying || propertyBlock == null) return;
        ApplyInspectorLook();
    }

    private void OnDestroy()
    {
        if (col != null) tilesByCollider.Remove(col);
        Deselect();

        // Per-tile material copies are not freed automatically.
        for (int r = 0; r < rendererCaches.Count; r++)
            DestroyInstanceArray(rendererCaches[r].instances);
    }

    // ---------------- Renderer caches & material instances ----------------

    private void BuildRendererCaches()
    {
        rendererCaches.Clear();

        CacheRendererMaterials(bodyRenderer);
        for (int i = 0; i < extraBodyRenderers.Count; i++)
        {
            if (extraBodyRenderers[i] != null && extraBodyRenderers[i] != bodyRenderer)
                CacheRendererMaterials(extraBodyRenderers[i]);
        }
    }

    private void CacheRendererMaterials(MeshRenderer targetRenderer)
    {
        if (targetRenderer == null) return;

        Material[] mats = targetRenderer.sharedMaterials;
        RendererSlotCache cache = new RendererSlotCache
        {
            renderer = targetRenderer,
            baseMaterials = mats,
            instances = null,
            colorProperties = new string[mats.Length],
            originalColors = new Color[mats.Length],
            activeColors = new Color[mats.Length],
            hasColor = new bool[mats.Length]
        };

        for (int i = 0; i < mats.Length; i++)
        {
            Material mat = mats[i];
            if (mat == null) continue;

            string resolvedProp = ResolveColorProperty(mat);
            if (!string.IsNullOrEmpty(resolvedProp))
            {
                Color c = mat.GetColor(resolvedProp);
                cache.colorProperties[i] = resolvedProp;
                cache.originalColors[i] = c;
                cache.activeColors[i] = c;
                cache.hasColor[i] = true;
            }
        }

        rendererCaches.Add(cache);
    }

    private string ResolveColorProperty(Material mat)
    {
        if (!string.IsNullOrEmpty(bodyColorProperty) && mat.HasProperty(bodyColorProperty))
            return bodyColorProperty;

        for (int i = 0; i < FallbackColorProperties.Length; i++)
        {
            if (mat.HasProperty(FallbackColorProperties[i]))
                return FallbackColorProperties[i];
        }

        return null;
    }

    private static void DestroyInstanceArray(Material[] instances)
    {
        if (instances == null) return;

        for (int i = 0; i < instances.Length; i++)
        {
            if (instances[i] != null) Destroy(instances[i]);
        }
    }

    /// <summary>Gives every renderer its shared materials back and frees the per-tile copies.</summary>
    private void ReleaseAllInstances()
    {
        for (int r = 0; r < rendererCaches.Count; r++)
        {
            RendererSlotCache cache = rendererCaches[r];
            if (cache.instances == null) continue;

            if (cache.renderer != null) cache.renderer.sharedMaterials = cache.baseMaterials;
            DestroyInstanceArray(cache.instances);
            cache.instances = null;
        }
    }

    private static void EnsureInstances(RendererSlotCache cache)
    {
        if (cache.instances != null || cache.renderer == null) return;

        // Renderer.materials clones every material and assigns the clones to this renderer.
        cache.instances = cache.renderer.materials;
    }

    private static Material GetInstance(RendererSlotCache cache, int slotIndex)
    {
        if (cache.instances == null || slotIndex >= cache.instances.Length) return null;
        return cache.instances[slotIndex];
    }

    private void ApplyInspectorLook()
    {
        if (bodyMaterialOverride != null)
        {
            SetBodyMaterial(bodyMaterialOverride, bodyMaterialSlot);
            inspectorMaterialApplied = true;
        }
        else if (inspectorMaterialApplied)
        {
            ResetBodyMaterial();
            inspectorMaterialApplied = false;
        }

        if (overrideBodyColor)
        {
            SetCustomColor(bodyColor, bodyColorSlot);
            inspectorColorApplied = true;
        }
        else if (inspectorColorApplied)
        {
            ClearCustomColor();
            inspectorColorApplied = false;
        }
    }

    // ---------------- Init & Face ----------------

    public void Init(int faceId, Sprite faceSprite, int layerIndex, Vector2 cellPosition)
    {
        this.layerIndex = layerIndex;
        this.cellPosition = cellPosition;
        hasRecordedTrayScale = false;

        boardParent = transform.parent;
        boardRestLocalScale = transform.localScale;

        SetFace(faceId, faceSprite);
        RecordBoardRestTransform();

        OnTileInitialized();
    }

    public void SetFace(int newFaceId, Sprite faceSprite)
    {
        faceId = newFaceId;
        realFaceSprite = faceSprite;
        RefreshFaceImage();
    }

    /// <summary>Draws a cover sprite (cloud, question mark...) instead of the real face. A null cover hides the image.</summary>
    protected void SetFaceCover(Sprite cover)
    {
        faceCovered = true;
        coverSprite = cover;
        RefreshFaceImage();
    }

    /// <summary>Shows the real face again.</summary>
    protected void ClearFaceCover()
    {
        faceCovered = false;
        coverSprite = null;
        RefreshFaceImage();
    }

    private void RefreshFaceImage()
    {
        if (faceImage == null) return;

        Sprite shown = faceCovered ? coverSprite : realFaceSprite;
        faceImage.sprite = shown;
        faceImage.enabled = shown != null;
    }

    // ---------------- Material & Color ----------------

    /// <summary>
    /// Replaces the body material. materialSlot = -1 replaces ALL slots on the body renderer;
    /// 0, 1, etc. replaces just that Material Element. Any custom color stays applied.
    /// </summary>
    public void SetBodyMaterial(Material material, int materialSlot = -1)
    {
        if (material == null || bodyRenderer == null) return;

        ReleaseAllInstances();

        Material[] mats = bodyRenderer.sharedMaterials;

        if (materialSlot < 0)
        {
            for (int i = 0; i < mats.Length; i++) mats[i] = material;
        }
        else if (materialSlot < mats.Length)
        {
            mats[materialSlot] = material;
        }
        else
        {
            Debug.LogWarning($"[TILE] '{name}': material slot {materialSlot} does not exist (body has {mats.Length} slot(s)).", this);
            RefreshAfterMaterialChange();
            return;
        }

        bodyRenderer.sharedMaterials = mats;
        RefreshAfterMaterialChange();
    }

    /// <summary>Puts back the material(s) the body renderer had when the tile was created.</summary>
    public void ResetBodyMaterial()
    {
        if (bodyRenderer == null || originalBodyMaterials == null) return;

        ReleaseAllInstances();
        bodyRenderer.sharedMaterials = originalBodyMaterials;
        RefreshAfterMaterialChange();
    }

    private void RefreshAfterMaterialChange()
    {
        BuildRendererCaches();

        if (hasCustomColor)
            SetCustomColor(customColor, customColorSlot);
        else
            ApplyVisualState();
    }

    /// <summary>
    /// Overrides the tile's body color.
    /// materialSlot = -1 applies to all material slots; 0, 1, etc. targets a specific Material Element.
    /// </summary>
    public void SetCustomColor(Color color, int materialSlot = -1)
    {
        hasCustomColor = true;
        customColor = color;
        customColorSlot = materialSlot;

        bool targetedAnyColorSlot = false;

        for (int r = 0; r < rendererCaches.Count; r++)
        {
            RendererSlotCache cache = rendererCaches[r];
            for (int i = 0; i < cache.hasColor.Length; i++)
            {
                if (!cache.hasColor[i]) continue;

                if (materialSlot < 0 || i == materialSlot)
                {
                    cache.activeColors[i] = color;
                    targetedAnyColorSlot = true;
                }
                else
                {
                    cache.activeColors[i] = cache.originalColors[i];
                }
            }
        }

        if (!targetedAnyColorSlot && !warnedNoColorProperty)
        {
            warnedNoColorProperty = true;
            Debug.LogWarning(
                $"[TILE] '{name}': a body color was requested (material slot {materialSlot}) but that slot has no color property, so nothing will change. " +
                $"Tried '{bodyColorProperty}' and the fallbacks _BaseColor / _Color / _TintColor / _MainColor. " +
                "Set 'Body Color Property' to your shader's exact color property name (Shader Graph: the Reference name, with Exposed ticked).", this);
        }

        ApplyVisualState();
    }

    public void ClearCustomColor()
    {
        hasCustomColor = false;
        customColorSlot = -1;

        for (int r = 0; r < rendererCaches.Count; r++)
        {
            RendererSlotCache cache = rendererCaches[r];
            for (int i = 0; i < cache.hasColor.Length; i++)
            {
                cache.activeColors[i] = cache.originalColors[i];
            }
        }

        ApplyVisualState();
    }

    // ---------------- Board / Tray transforms ----------------

    public void RecordBoardRestTransform()
    {
        restLocalPosition = transform.localPosition;
        restLocalRotation = transform.localRotation;
        hasRecordedRestTransform = true;
    }

    public void RestoreBoardRestTransform()
    {
        if (!hasRecordedRestTransform) return;
        transform.localPosition = restLocalPosition;
        transform.localRotation = restLocalRotation;
    }

    /// <summary>
    /// Gives this tile a new spot on the board (Shuffle booster). Only updates the data;
    /// the arrangement manager animates the tile to the new rest position.
    /// </summary>
    public void SetBoardPlacement(int newLayerIndex, Vector2 newCellPosition, Vector3 newLocalPosition, Quaternion newLocalRotation)
    {
        layerIndex = newLayerIndex;
        cellPosition = newCellPosition;
        restLocalPosition = newLocalPosition;
        restLocalRotation = newLocalRotation;
        hasRecordedRestTransform = true;
    }

    /// <summary>
    /// Undo booster: frees the tray slot, puts the tile back under its board parent and makes it clickable again.
    /// Keeps the current world position so the arrangement manager can fly it back.
    /// </summary>
    public void PrepareReturnToBoard()
    {
        Deselect();
        slot = null;
        hasRecordedTrayScale = false;

        if (boardParent != null) transform.SetParent(boardParent, true);

        EnableInteraction();
        EnableShadows();

        OnReturnedToBoard();
    }

    public void RecordTrayRestScale()
    {
        trayRestLocalScale = transform.localScale;
        hasRecordedTrayScale = true;
    }

    public void RestoreTrayRestScale()
    {
        if (!hasRecordedTrayScale) return;
        transform.localScale = trayRestLocalScale;
    }

    public void SetSlot(TraySlot newSlot)
    {
        bool wasOnBoard = slot == null;
        slot = newSlot;

        if (wasOnBoard && newSlot != null) OnEnteredTray();
    }

    // ---------------- Blocked & Visual State ----------------

    public void SetBlocked(bool blocked)
    {
        isBlocked = blocked;
        ApplyVisualState();
        OnBlockedChanged(blocked);
    }

    private void ApplyVisualState()
    {
        if (faceImage != null)
            faceImage.color = isBlocked ? blockedColor : Color.white;

        bool shouldDim = isBlocked && tintBodyWhenBlocked;

        for (int r = 0; r < rendererCaches.Count; r++)
        {
            RendererSlotCache cache = rendererCaches[r];
            if (cache.renderer == null) continue;

            for (int slotIndex = 0; slotIndex < cache.hasColor.Length; slotIndex++)
            {
                if (!cache.hasColor[slotIndex])
                {
                    if (colorMode == TileColorMode.PropertyBlock)
                        cache.renderer.SetPropertyBlock(null, slotIndex);
                    continue;
                }

                bool slotHasCustomColor = hasCustomColor && (customColorSlot < 0 || slotIndex == customColorSlot);

                Color finalColor = cache.activeColors[slotIndex];
                if (shouldDim) finalColor *= blockedColor;

                bool needsOverride = shouldDim || slotHasCustomColor;

                if (colorMode == TileColorMode.PropertyBlock)
                    ApplyWithPropertyBlock(cache, slotIndex, finalColor, needsOverride);
                else
                    ApplyWithMaterialInstance(cache, slotIndex, finalColor, needsOverride);
            }
        }
    }

    private void ApplyWithPropertyBlock(RendererSlotCache cache, int slotIndex, Color finalColor, bool needsOverride)
    {
        if (!needsOverride)
        {
            cache.renderer.SetPropertyBlock(null, slotIndex);
            return;
        }

        cache.renderer.GetPropertyBlock(propertyBlock, slotIndex);
        propertyBlock.SetColor(cache.colorProperties[slotIndex], finalColor);
        cache.renderer.SetPropertyBlock(propertyBlock, slotIndex);
    }

    private void ApplyWithMaterialInstance(RendererSlotCache cache, int slotIndex, Color finalColor, bool needsOverride)
    {
        if (needsOverride)
        {
            EnsureInstances(cache);
            Material instance = GetInstance(cache, slotIndex);
            if (instance != null) instance.SetColor(cache.colorProperties[slotIndex], finalColor);
        }
        else if (cache.instances != null)
        {
            // Back to the material's own color (the copy stays around, ready for the next change).
            Material instance = GetInstance(cache, slotIndex);
            if (instance != null) instance.SetColor(cache.colorProperties[slotIndex], cache.originalColors[slotIndex]);
        }
    }

    public void DisableShadows()
    {
        if (bodyRenderer != null)
            bodyRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    public void EnableShadows()
    {
        if (bodyRenderer != null)
            bodyRenderer.shadowCastingMode = originalShadowMode;
    }

    public void DisableInteraction()
    {
        if (col != null) col.enabled = false;
        Deselect();
    }

    public void EnableInteraction()
    {
        if (col != null) col.enabled = true;
    }

    public void Select(Material outlineMat)
    {
        Deselect();

        if (outlineMat == null || bodyRenderer == null || !HasBodyMesh) return;

        outlineObject = new GameObject(bodyRenderer.gameObject.name + "_Outline");
        outlineObject.transform.SetParent(bodyRenderer.transform, false);

        MeshFilter outlineFilter = outlineObject.AddComponent<MeshFilter>();
        outlineFilter.sharedMesh = bodyMesh.sharedMesh;

        MeshRenderer outlineRenderer = outlineObject.AddComponent<MeshRenderer>();
        Material[] outlineMats = new Material[bodyRenderer.sharedMaterials.Length];
        for (int i = 0; i < outlineMats.Length; i++) outlineMats[i] = outlineMat;

        outlineRenderer.sharedMaterials = outlineMats;
        outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        outlineRenderer.receiveShadows = false;
    }

    public void Deselect()
    {
        if (outlineObject != null) Destroy(outlineObject);
        outlineObject = null;
    }
}