using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace com.otm.editor
{
	public class MaterialWindow : OdinEditorWindow
	{
		public Shader shader;
		public string textureFiledName;
		[FolderPath(ParentFolder = "")]
		public string textureFolderPath;
		public string folderPath;


		public static MaterialWindow instance;

		public static void ShowWindow(string folderPath)
		{
			MaterialWindow window = GetWindow<MaterialWindow>("Material Window");
			window.folderPath = folderPath;
			instance = window;
		}

		protected override void OnGUI()
		{
			base.OnGUI();
			Event e = Event.current;
			if (e == null)
				return;
			if (e.keyCode == KeyCode.Escape)
			{
				instance.Close();
				instance = null;
			}
			if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
			{
				CreateMaterials();
			}

		}

		[Button]
		private void CreateMaterials()
		{
			Debug.Log(textureFolderPath);
			if (string.IsNullOrEmpty(textureFolderPath))
				return;
			Texture2D[] textures = OTMEditorUtility.GetAssets<Texture2D>(textureFolderPath);
			for (int i = 0; i < textures.Length; i++)
			{
				Material material = new Material(shader);
				material.SetTexture(textureFiledName, textures[i]);
				AssetDatabase.CreateAsset(material, folderPath + "/" + textures[i].name + ".mat");
			}
			AssetDatabase.Refresh();
		}
	}
}
