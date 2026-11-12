using System.Collections.Generic;
using UnityEngine;

public enum MahjongBoardPlane
{
    XZ_Flat,       // Flat on a table (Grid is X/Z, layers stack UP on Y)
    XY_Vertical    // Standing upright (Grid is X/Y, layers stack OUT on Z)
}

public enum GridSizingMode
{
    Manual,              // Use fixed tile sizes and gap
    FitToDimensions,     // Divide a manually typed total area into exact grid pieces
    FitToBoxCollider     // Measure a physical BoxCollider in the scene and divide it up
}

public class MahjongVisualSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Requires a prefab with the MahjongTile component.")]
    [SerializeField] private MahjongTile tilePrefab;
    [Tooltip("Optional. Tiles will be grouped under this object.")]
    [SerializeField] private Transform boardRoot;
    
    [Header("Level Data")]
    [SerializeField] private MahjongLevelData levelAsset;

    [Header("Orientation & Stacking")]
    [SerializeField] private MahjongBoardPlane boardPlane = MahjongBoardPlane.XZ_Flat;
    [SerializeField] private Vector3 tileRotation = Vector3.zero;
    [SerializeField] private bool invertStackingDirection = false;
    [Tooltip("The base thickness of a single tile.")]
    [SerializeField] private float layerThickness = 0.3f;
    [Tooltip("EXTRA empty distance between each layer (useful for shadows/camera spacing).")]
    [SerializeField] private float distanceBetweenLayers = 0f;

    [Header("Grid Sizing")]
    [SerializeField] private GridSizingMode sizingMode = GridSizingMode.Manual;

    [Header("  ↳ If Manual:")]
    [SerializeField] private Vector2 manualTileSize = new Vector2(1f, 1.3f);
    [SerializeField] private Vector2 manualGap = new Vector2(0.05f, 0.05f);

    [Header("  ↳ If Fit To Dimensions:")]
    [Tooltip("The total width (X) and height (Y) area to divide into a grid.")]
    [SerializeField] private Vector2 totalAreaDimensions = new Vector2(100f, 100f);

    [Header("  ↳ If Fit To Box Collider:")]
    [Tooltip("Drop a BoxCollider here. The grid will scale to fit inside its physical size.")]
    [SerializeField] private BoxCollider targetBoundsBox;

    [Header("Positioning")]
    [Tooltip("Centers the fully generated board on the parent spawner object.")]
    [SerializeField] private bool centerBoard = true;

    private readonly List<MahjongTile> spawnedTiles = new List<MahjongTile>();

    private void Start()
    {
        if (levelAsset != null) Generate();
    }

    [ContextMenu("Generate Level")]
    public void Generate()
    {
        Clear();

        if (levelAsset == null || tilePrefab == null)
        {
            Debug.LogError("MahjongVisualSpawner: Missing Level Asset or Tile Prefab setup.");
            return;
        }

        Transform parent = boardRoot != null ? boardRoot : transform;
        Quaternion customRotation = Quaternion.Euler(tileRotation);

        // Find the maximum grid bounds across all layers to divide the space properly
        int maxW = 1;
        int maxH = 1;
        foreach (var layer in levelAsset.layers)
        {
            maxW = Mathf.Max(maxW, layer.width);
            maxH = Mathf.Max(maxH, layer.height);
        }

        // Calculate Cell Step (Distance between the center of one cell and the next)
        float stepX = 1f;
        float stepY = 1f;

        if (sizingMode == GridSizingMode.Manual)
        {
            stepX = manualTileSize.x + manualGap.x;
            stepY = manualTileSize.y + manualGap.y;
        }
        else if (sizingMode == GridSizingMode.FitToDimensions)
        {
            stepX = totalAreaDimensions.x / maxW;
            stepY = totalAreaDimensions.y / maxH;
        }
        else if (sizingMode == GridSizingMode.FitToBoxCollider && targetBoundsBox != null)
        {
            // Calculate true world size of the box collider (ignoring rotation)
            float sizeU = targetBoundsBox.size.x * targetBoundsBox.transform.lossyScale.x;
            float sizeV = boardPlane == MahjongBoardPlane.XZ_Flat ? 
                          targetBoundsBox.size.z * targetBoundsBox.transform.lossyScale.z : 
                          targetBoundsBox.size.y * targetBoundsBox.transform.lossyScale.y;

            stepX = sizeU / maxW;
            stepY = sizeV / maxH;
        }

        // Calculate Centering Offsets
        float startU = 0f;
        float startV = 0f;
        if (centerBoard)
        {
            float boardWidth = maxW * stepX;
            float boardHeight = maxH * stepY;
            
            // Subtract half the board size, then add half a cell to align center-to-center
            startU = -(boardWidth / 2f) + (stepX / 2f);
            startV = -(boardHeight / 2f) + (stepY / 2f);
        }

        // Total stack shift
        float layerTotalShift = layerThickness + distanceBetweenLayers;
        float stackSign = invertStackingDirection ? -1f : 1f;

        // SPAWNING LOOP
        for (int z = 0; z < levelAsset.layers.Count; z++)
        {
            MahjongLayer layer = levelAsset.layers[z];

            for (int y = 0; y < layer.height; y++)
            {
                for (int x = 0; x < layer.width; x++)
                {
                    if (layer.GetCell(x, y))
                    {
                        // 1. Calculate Grid Base Coordinates
                        float posU = startU + ((x + layer.offset.x) * stepX);
                        float posV = startV + ((y + layer.offset.y) * stepY); 
                        
                        // 2. Calculate Stacking Depth
                        float posStack = z * layerTotalShift * stackSign; 

                        // 3. Map to 3D Space depending on Table or Wall orientation
                        Vector3 spawnPos;
                        if (boardPlane == MahjongBoardPlane.XZ_Flat)
                        {
                            spawnPos = new Vector3(posU, posStack, posV); // Depth is on Y
                        }
                        else
                        {
                            // We use -posV so Row 0 (top of the grid in UI) stays at the top physically
                            spawnPos = new Vector3(posU, -posV, posStack); // Depth is on Z
                        }

                        MahjongTile tile = Instantiate(tilePrefab, parent);
                        tile.transform.localPosition = spawnPos;
                        tile.transform.localRotation = customRotation;
                        tile.name = $"Tile_{x}_{y}_L{z}";
                        
                        spawnedTiles.Add(tile);
                    }
                }
            }
        }
    }

    [ContextMenu("Clear Board")]
    public void Clear()
    {
        for (int i = spawnedTiles.Count - 1; i >= 0; i--)
        {
            if (spawnedTiles[i] != null)
            {
                if (Application.isPlaying) Destroy(spawnedTiles[i].gameObject);
                else DestroyImmediate(spawnedTiles[i].gameObject);
            }
        }
        spawnedTiles.Clear();
    }
}