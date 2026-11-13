using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace com.otm.editor
{

    public class TextturePacker : OdinEditorWindow
    {
        #region  public variables
        [FolderPath]
        public string folderPath;
        [MinValue(0)]
        public int padding = 2;
        public string outputName = "PackedTexture";

        #endregion        

        #region  public methods

        [MenuItem("OTM TOOLS/Texture packer")]
        public static void ShowSpritePackerWindow()
        {
            GetWindow<TextturePacker>("Texture Packer");
        }

        [Button]
        public void PackeTexture()
        {
            if (string.IsNullOrEmpty(folderPath))
                return;

            folderPath = NormalizeFolderPath(folderPath);
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError("Invalid texture packer folder path: " + folderPath);
                return;
            }

            Sprite[] sprites = FilterOutputSprite(OTMEditorUtility.GetAssets<Sprite>(folderPath));
            if (sprites.Length <= 0)
                return;
            PackInToTexture(sprites);
        }

        #endregion

        #region  private methods

        private void PackInToTexture(Sprite[] sprites)
        {
            List<TextureImporter> changedImporters = SetReadable(sprites, true);

            try
            {
                int spriteSize = GetMaxSpriteSize(sprites);
                int gridSize = Mathf.CeilToInt(Mathf.Sqrt(sprites.Length));
                int cellSize = spriteSize + (padding * 2);
                int textureSize = cellSize * gridSize;
                Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

                FillTexture(texture, Color.clear);

                for (int i = 0; i < sprites.Length; i++)
                    CopySpriteToTexture(texture, sprites[i], i, gridSize, cellSize);

                texture.Apply();
                SaveTexture(texture);
                DestroyImmediate(texture);
            }
            finally
            {
                SetReadable(changedImporters, false);
            }
        }

        private int GetMaxSpriteSize(Sprite[] sprites)
        {
            int size = 0;
            for (int i = 0; i < sprites.Length; i++)
            {
                Rect rect = sprites[i].textureRect;
                size = Mathf.Max(size, Mathf.Max(Mathf.CeilToInt(rect.width), Mathf.CeilToInt(rect.height)));
            }
            return size;
        }

        private void FillTexture(Texture2D texture, Color color)
        {
            Color[] colors = new Color[texture.width * texture.height];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = color;
            texture.SetPixels(colors);
        }

        private void CopySpriteToTexture(Texture2D texture, Sprite sprite, int index, int gridSize, int cellSize)
        {
            Rect rect = sprite.textureRect;
            int spriteWidth = Mathf.CeilToInt(rect.width);
            int spriteHeight = Mathf.CeilToInt(rect.height);
            int col = index % gridSize;
            int row = index / gridSize;
            int startX = (col * cellSize) + padding + ((cellSize - (padding * 2) - spriteWidth) / 2);
            int startY = ((gridSize - 1 - row) * cellSize) + padding + ((cellSize - (padding * 2) - spriteHeight) / 2);
            Color[] pixels = sprite.texture.GetPixels(Mathf.FloorToInt(rect.x), Mathf.FloorToInt(rect.y), spriteWidth, spriteHeight);
            texture.SetPixels(startX, startY, spriteWidth, spriteHeight, pixels);
        }

        private void SaveTexture(Texture2D texture)
        {
            string assetPath = GetOutputAssetPath();
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(assetPath);

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            AssetDatabase.Refresh();
            Debug.Log("Texture packed: " + assetPath);
        }

        private string GetOutputAssetPath()
        {
            string fileName = string.IsNullOrEmpty(outputName) ? "PackedTexture" : outputName;
            fileName = fileName.EndsWith(".png") ? fileName : fileName + ".png";
            return folderPath.TrimEnd('/') + "/" + fileName;
        }

        private string NormalizeFolderPath(string path)
        {
            path = path.Replace("\\", "/");
            string dataPath = Application.dataPath.Replace("\\", "/");
            if (path.StartsWith(dataPath))
                return "Assets" + path.Substring(dataPath.Length);
            return path;
        }

        private Sprite[] FilterOutputSprite(Sprite[] sprites)
        {
            string outputAssetPath = GetOutputAssetPath();
            List<Sprite> filteredSprites = new List<Sprite>();

            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] == null || AssetDatabase.GetAssetPath(sprites[i]) == outputAssetPath)
                    continue;
                filteredSprites.Add(sprites[i]);
            }

            return filteredSprites.ToArray();
        }

        private List<TextureImporter> SetReadable(Sprite[] sprites, bool readable)
        {
            List<TextureImporter> changedImporters = new List<TextureImporter>();
            HashSet<string> paths = new HashSet<string>();

            for (int i = 0; i < sprites.Length; i++)
            {
                string path = AssetDatabase.GetAssetPath(sprites[i].texture);
                if (string.IsNullOrEmpty(path) || paths.Contains(path))
                    continue;
                paths.Add(path);

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.isReadable == readable)
                    continue;

                importer.isReadable = readable;
                importer.SaveAndReimport();
                changedImporters.Add(importer);
            }

            return changedImporters;
        }

        private void SetReadable(List<TextureImporter> importers, bool readable)
        {
            for (int i = 0; i < importers.Count; i++)
            {
                if (importers[i] == null)
                    continue;

                importers[i].isReadable = readable;
                importers[i].SaveAndReimport();
            }
        }

        #endregion
    }
}
