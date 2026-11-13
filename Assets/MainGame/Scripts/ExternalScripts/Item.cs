/*using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private int itemId;
    public int ItemId => itemId;

    private ItemSpots spot;
    public ItemSpots Spot => spot;

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    [Header("Spot Settings")]
    [SerializeField] private Vector3 rotationOnSpot = Vector3.zero;
    public Vector3 RotationOnSpot => rotationOnSpot;

    [Header("Elements")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider col;

    private Renderer[] allRenderers;
    private List<GameObject> outlineObjects = new List<GameObject>();

    private void Awake()
    {
        allRenderers = GetComponentsInChildren<Renderer>(true);
    }

    public void SetSpot(ItemSpots spot) { this.spot = spot; }

    public void DisableShadows()
    {
        foreach (Renderer r in allRenderers)
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    public void DisablePhysics()
    {
        rb.isKinematic = true;
        col.enabled = false;
    }

    public void Select(Material outlineMat)
    {
        Deselect();

        foreach (Renderer rend in allRenderers)
        {
            // Only works on MeshRenderers
            MeshFilter mf = rend.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            // One child outline per MeshRenderer
            GameObject outlineGO = new GameObject(rend.gameObject.name + "_Outline");
            outlineGO.transform.SetParent(rend.transform);
            outlineGO.transform.localPosition = Vector3.zero;
            outlineGO.transform.localRotation = Quaternion.identity;
            outlineGO.transform.localScale = Vector3.one;

            // Copy the mesh
            MeshFilter outlineMF = outlineGO.AddComponent<MeshFilter>();
            outlineMF.sharedMesh = mf.sharedMesh;

            // One outline material slot per original material slot
            MeshRenderer outlineMR = outlineGO.AddComponent<MeshRenderer>();
            Material[] outlineMats = new Material[rend.sharedMaterials.Length];
            for (int i = 0; i < outlineMats.Length; i++)
                outlineMats[i] = outlineMat;

            outlineMR.materials = outlineMats;
            outlineMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            outlineMR.receiveShadows = false;

            outlineObjects.Add(outlineGO);
        }
    }

    public void Deselect()
    {
        foreach (GameObject outline in outlineObjects)
        {
            if (outline != null)
                Destroy(outline);
        }
        outlineObjects.Clear();
    }

    private void OnDestroy()
    {
        Deselect();
    }
}*/