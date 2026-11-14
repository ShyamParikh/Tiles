using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Put this on the ROOT of your tile prefab (TilePrefab3D).
/// Assign all visual and collider references directly in the Inspector.
/// </summary>
public class Tile : MonoBehaviour
{
    private static readonly Dictionary<Collider, Tile> tilesByCollider = new Dictionary<Collider, Tile>();

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
    [Tooltip("The UI Image on the Canvas child (ObjectImage). The face sprite is drawn here.")]
    [SerializeField] private Image faceImage;

    [Header("Interaction (Assign in Inspector)")]
    [Tooltip("REQUIRED: Drag the BoxCollider on this tile prefab here.")]
    [SerializeField] private Collider col;

    [Header("Tile Body Color")]
    [Tooltip("Optional: Force a specific body color on this Tile Prefab (e.g. Gold) even if it shares the Classic material.")]
    [SerializeField] private bool overrideBodyColor = false;
    [SerializeField] private Color customBodyColor = Color.white;

    [Header("Blocked Look (tile covered by another tile)")]
    [SerializeField] private Color blockedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    [Tooltip("Colour property of the body material. URP Lit = _BaseColor, Built-in Standard = _Color.")]
    [SerializeField] private string bodyColorProperty = "_BaseColor";

    private int faceId;
    private int layerIndex;
    private Vector2 cellPosition;
    private bool isBlocked;
    private TraySlot slot;

    private Vector3 restLocalPosition;
    private Quaternion restLocalRotation;
    private bool hasRecordedRestTransform;

    private Vector3 trayRestLocalScale;
    private bool hasRecordedTrayScale;

    private MaterialPropertyBlock propertyBlock;
    private Color baseBodyColor = Color.white;
    private bool preservePropertyColor;
    private GameObject outlineObject;

    public int FaceId => faceId;
    public Sprite FaceSprite => faceImage != null ? faceImage.sprite : null;
    public int LayerIndex => layerIndex;
    public Vector2 CellPosition => cellPosition;
    public bool IsBlocked => isBlocked;
    public TraySlot Slot => slot;
    public Collider TileCollider => col;
    public Vector3 RestLocalPosition => restLocalPosition;
    public Quaternion RestLocalRotation => restLocalRotation;
    public Vector3 TrayRestLocalScale => hasRecordedTrayScale ? trayRestLocalScale : transform.localScale;

    public bool HasBodyMesh => bodyMesh != null && bodyMesh.sharedMesh != null;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();

        if (bodyRenderer != null)
        {
            bodyRenderer.GetPropertyBlock(propertyBlock);
            Color initialPropColor = propertyBlock.GetColor(bodyColorProperty);

            if (overrideBodyColor)
            {
                baseBodyColor = customBodyColor;
                preservePropertyColor = true;
                propertyBlock.SetColor(bodyColorProperty, baseBodyColor);
                bodyRenderer.SetPropertyBlock(propertyBlock);
            }
            else if (initialPropColor.a > 0f)
            {
                baseBodyColor = initialPropColor;
                preservePropertyColor = true;
            }
            else if (bodyRenderer.sharedMaterial != null && bodyRenderer.sharedMaterial.HasProperty(bodyColorProperty))
            {
                baseBodyColor = bodyRenderer.sharedMaterial.GetColor(bodyColorProperty);
                preservePropertyColor = false;
            }
        }

        if (col != null)
        {
            tilesByCollider[col] = this;
        }
        else
        {
            Debug.LogError($"[TILE] '{name}' has NO Collider assigned in the 'Col' Inspector slot! It cannot be clicked.", this);
        }
    }

    private void OnDestroy()
    {
        if (col != null) tilesByCollider.Remove(col);
        Deselect();
    }

    public void Init(int faceId, Sprite faceSprite, int layerIndex, Vector2 cellPosition)
    {
        this.layerIndex = layerIndex;
        this.cellPosition = cellPosition;
        hasRecordedTrayScale = false;
        SetFace(faceId, faceSprite);
        RecordBoardRestTransform();
    }

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

    public void SetFace(int newFaceId, Sprite faceSprite)
    {
        faceId = newFaceId;

        if (faceImage != null)
        {
            faceImage.sprite = faceSprite;
            faceImage.enabled = faceSprite != null;
        }
    }

    public void SetSlot(TraySlot newSlot)
    {
        slot = newSlot;
    }

    public void SetBlocked(bool blocked)
    {
        isBlocked = blocked;

        if (faceImage != null)
            faceImage.color = blocked ? blockedColor : Color.white;

        if (bodyRenderer != null)
        {
            if (blocked)
            {
                bodyRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(bodyColorProperty, baseBodyColor * blockedColor);
                bodyRenderer.SetPropertyBlock(propertyBlock);
            }
            else if (preservePropertyColor)
            {
                bodyRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(bodyColorProperty, baseBodyColor);
                bodyRenderer.SetPropertyBlock(propertyBlock);
            }
            else
            {
                bodyRenderer.SetPropertyBlock(null);
            }
        }
    }

    public void DisableShadows()
    {
        if (bodyRenderer != null)
            bodyRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    public void DisableInteraction()
    {
        if (col != null) col.enabled = false;
        Deselect();
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