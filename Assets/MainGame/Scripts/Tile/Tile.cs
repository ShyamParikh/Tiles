using UnityEngine;

/// <summary>
/// Put this on the ROOT of TilePrefab3D.
/// It contains no gameplay. It only tells the spawner which mesh is the tile body,
/// so the spawner can measure the real size of the tile.
/// </summary>
public class MahjongTile : MonoBehaviour
{
    [Tooltip("Drag the RoundedCube child object here (the one with the MeshFilter).")]
    [SerializeField] private MeshFilter bodyMesh;

    public bool HasBodyMesh
    {
        get { return bodyMesh != null && bodyMesh.sharedMesh != null; }
    }

    // Size of the tile body in world units, along the tile's OWN axes.
    // Includes every scale in the prefab (root scale x child scale).
    public Vector3 GetBodySize()
    {
        if (!HasBodyMesh) return Vector3.zero;

        Vector3 meshSize = bodyMesh.sharedMesh.bounds.size;
        Vector3 scale = bodyMesh.transform.lossyScale;

        return new Vector3(
            meshSize.x * Mathf.Abs(scale.x),
            meshSize.y * Mathf.Abs(scale.y),
            meshSize.z * Mathf.Abs(scale.z));
    }
}