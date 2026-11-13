using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.otm.editor
{
	public static class OTMEditorUtility
	{
		public static string GetSelectedFolderPath()
		{
			MethodInfo getActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
			return (string)getActiveFolderPath.Invoke(null, null);
		}

		public static T[] GetAssets<T>(string path) where T : UnityEngine.Object
		{
			if (string.IsNullOrEmpty(path))
				return new T[0];
			Type type = typeof(T);
			string[] guids = AssetDatabase.FindAssets("t:" + type.Name, new string[] { path });
			T[] objects = new T[guids.Length];
			for (int i = 0; i < guids.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				objects[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
			}
			return objects;
		}

		public static List<T> GetPrefab<T>(string path) where T : Component
		{
			if (string.IsNullOrEmpty(path))
				return new List<T>();
			Type type = typeof(T);
			string[] guids = AssetDatabase.FindAssets("t:prefab", new string[] { path });
			List<T> objects = new List<T>();
			for (int i = 0; i < guids.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
				T component = obj.GetComponent<T>();
				if (component != null)
					objects.Add(component);
			}
			return objects;
		}

		public static T FindAsset<T>(string path) where T : UnityEngine.Object
		{
			if (string.IsNullOrEmpty(path))
				return null;
			Type type = typeof(T);
			string[] pathSplit = path.Split('/');
			for (int k = pathSplit.Length - 1; k >= 0; k--)
			{
				string[] guids = AssetDatabase.FindAssets("t:" + type.Name, new string[] { CreatePath(pathSplit, 0, k) });
				for (int i = 0; i < guids.Length; i++)
				{
					string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
					T obj = AssetDatabase.LoadAssetAtPath<T>(assetPath);
					if (obj != null)
						return obj;
				}
			}

			return null;
		}

		private static string CreatePath(string[] pathArray, int startIndex, int endIndex)
		{
			if (pathArray == null || pathArray.Length <= 0)
				return "";
			string path = "";
			for (int i = startIndex; i <= endIndex; i++)
			{
				if (i != endIndex)
					path += pathArray[i] + "/";
				else
					path += pathArray[i];
			}
			return path;
		}

		public static string GetFolderName(UnityEngine.Object obj)
		{
			string assetPath = AssetDatabase.GetAssetPath(obj);
			string[] folders = assetPath.Split("/");
			return folders[folders.Length - 2];
		}

		public static string GetFolderPath(UnityEngine.Object obj)
		{
			string assetPath = AssetDatabase.GetAssetPath(obj);
			if (string.IsNullOrEmpty(assetPath))
				return "";
			assetPath = assetPath.Replace(assetPath.Split("/").Last(), "");
			return assetPath;
		}

		public static string GetFolderName(string path)
		{
			string[] folders = path.Split("/");
			for (int i = folders.Length - 1; i >= 0; i--)
			{
				if (!string.IsNullOrEmpty(folders[i]))
					return folders[i];
			}
			return "";
		}

		public static string[] GetSelectedFolders()
		{
			Object[] selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

			List<string> folderPaths = new List<string>();

			foreach (Object obj in selectedAssets)
			{
				string path = AssetDatabase.GetAssetPath(obj);

				if (AssetDatabase.IsValidFolder(path))
				{
					folderPaths.Add(path);
				}
			}
			return folderPaths.ToArray();
		}

		public static T[] GetSelectedObject<T>() where T : Object
		{
			Object[] selectedAssets = Selection.GetFiltered(typeof(T), SelectionMode.Assets);

			List<T> objs = new List<T>();

			foreach (Object obj in selectedAssets)
			{
				objs.Add((T)obj);
			}
			return objs.ToArray();
		}
	}
}