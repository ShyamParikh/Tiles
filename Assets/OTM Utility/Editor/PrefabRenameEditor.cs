using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.WSA;

namespace com.otm.editor
{
	public static class PrefabRenameEditor
	{
		[MenuItem("Assets/OTM/RenamePrefab")]
		public static void RenamePrefab()
		{
			string folder = OTMEditorUtility.GetSelectedFolderPath();
			if (string.IsNullOrEmpty(folder))
				return;
			GameObject[] objects = OTMEditorUtility.GetAssets<GameObject>(folder);
			string[] folders = folder.Split("/");
			RenameWindow.ShowWindow(folders[folders.Length - 1], objects);
		}


		[MenuItem("Assets/OTM/RenameFolder")]
		public static void RenameFolder()
		{
			string folder = OTMEditorUtility.GetSelectedFolderPath();
			if (string.IsNullOrEmpty(folder))
				return;
			string[] folders = AssetDatabase.GetSubFolders(folder);
			string[] f = folder.Split("/");
			RenameWindow.ShowWindow(f[f.Length - 1], folders);
		}

	}
}
