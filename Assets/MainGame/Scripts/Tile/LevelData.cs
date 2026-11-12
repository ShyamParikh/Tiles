using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A hand-made level stored as an asset (Create > Mahjong > Level Data).
/// Each element of "Layers" is one layer, index 0 = bottom layer.
/// Inside a layer, write one line per row, TOP row first.
/// 'X' (or 'x' or '#') = a tile, any other character = empty.
///
/// Half Step Grid ON (default): every character is HALF a tile.
/// 'X' marks the top-left corner of a tile. The tile covers that character, the one to its right,
/// and the two below them. Tiles side by side are written "X.X.X.", and moving an X one
/// character moves that tile half a tile.
///
/// Half Step Grid OFF: old format, every character is one whole tile and the Stack Style
/// offset of the spawner is applied to odd layers.
/// </summary>
[CreateAssetMenu(fileName = "MahjongLevel", menuName = "Mahjong/Level Data")]
public class MahjongLevelData : ScriptableObject
{
    [Tooltip("ON: every character is HALF a tile ('X' = top-left of a tile). OFF: every character is one whole tile.")]
    [SerializeField] private bool halfStepGrid = true;

    [SerializeField, TextArea(5, 16)] private string[] layers;

    public int LayerCount
    {
        get
        {
            if (layers == null)
            {
                return 0;
            }
            return layers.Length;
        }
    }

    // Converts the text into grid positions: x, y = half-tile units (y up), z = layer.
    public List<Vector3Int> ToPositions(MahjongStackSettings stack)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        if (layers == null)
        {
            return result;
        }

        // Half-step mode aligns every layer to the tallest one, so layers line up on top of each other.
        int tallestLayerRows = 0;
        for (int z = 0; z < layers.Length; z++)
        {
            if (string.IsNullOrEmpty(layers[z]))
            {
                continue;
            }
            int lineCount = layers[z].Replace("\r", "").Split('\n').Length;
            tallestLayerRows = Mathf.Max(tallestLayerRows, lineCount);
        }

        for (int z = 0; z < layers.Length; z++)
        {
            if (string.IsNullOrEmpty(layers[z]))
            {
                continue;
            }

            string[] lines = layers[z].Replace("\r", "").Split('\n');
            Vector2Int offset = stack.GetLayerOffset(z);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                for (int x = 0; x < line.Length; x++)
                {
                    char c = line[x];
                    if (c != 'X' && c != 'x' && c != '#')
                    {
                        continue;
                    }

                    if (halfStepGrid)
                    {
                        // First line of text = top row. Same constant shift for every layer.
                        result.Add(new Vector3Int(x, tallestLayerRows - 1 - i, z));
                    }
                    else
                    {
                        int row = lines.Length - 1 - i;
                        result.Add(new Vector3Int(x * 2 + offset.x, row * 2 + offset.y, z));
                    }
                }
            }
        }
        return result;
    }
}