using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.otm.editor
{
	public static class SpriteEditor
	{
		[MenuItem("Assets/OTM/RenameSprites")]
		public static void RenameSprite()
		{
			string folder = OTMEditorUtility.GetSelectedFolderPath();
			if (string.IsNullOrEmpty(folder))
				return;
			Sprite[] sprites = OTMEditorUtility.GetAssets<Sprite>(folder);
			string[] folders = folder.Split("/");
			RenameWindow.ShowWindow(folders[folders.Length - 1], sprites);
		}

		[MenuItem("Assets/OTM/RenameTexture")]
		public static void RenameTexture()
		{
			string folder = OTMEditorUtility.GetSelectedFolderPath();
			if (string.IsNullOrEmpty(folder))
				return;
			Texture2D[] textures = OTMEditorUtility.GetAssets<Texture2D>(folder);
			string[] folders = folder.Split("/");
			RenameWindow.ShowWindow(folders[folders.Length - 1], textures);
		}

		[MenuItem("Assets/OTM/RenameSelectedSprite")]
		public static void RenameSelectedSprite()
		{
			Object[] objects = Selection.objects;
			List<Sprite> sprites = new List<Sprite>();
			for (int i = 0; i < objects.Length; i++)
			{
				if (objects[i] is Texture2D)
				{
					Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(objects[i]));
					if (s != null)
					{
						sprites.Add(s);
					}
				}
			}
			RenameWindow.ShowWindow("", sprites.ToArray());
		}

		[MenuItem("Assets/OTM/CreateMaterial")]
		public static void CreateMaterial()
		{
			string folder = OTMEditorUtility.GetSelectedFolderPath();
			if (string.IsNullOrEmpty(folder))
				return;
			MaterialWindow.ShowWindow(folder);
		}

		[MenuItem("Assets/OTM/Convert Sprite To Single")]
		public static void ConvertToSingleModeSprite()
		{
			string folder = OTMEditorUtility.GetSelectedFolderPath();
			Sprite[] sprites = OTMEditorUtility.GetAssets<Sprite>(folder);
			for (int i = 0; i < sprites.Length; i++)
			{
				ConvertToSingle(AssetDatabase.GetAssetPath(sprites[i]));
			}
		}

		private static void ConvertToSingle(string path)
		{
			TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
			if (importer == null || importer.spriteImportMode == SpriteImportMode.Single)
				return;
			importer.textureType = TextureImporterType.Sprite;
			importer.spriteImportMode = SpriteImportMode.Single;
			importer.SaveAndReimport();
		}

	}
}
