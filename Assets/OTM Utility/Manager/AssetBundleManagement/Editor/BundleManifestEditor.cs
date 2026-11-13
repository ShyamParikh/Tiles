using com.otm.utility;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace com.otm.editor
{
	[CustomEditor(typeof(BundleManifest))]
	public class BundleManifestEditor : OdinEditor
	{
		#region private variables

		private BundleManifest bundleManifest;

		#endregion

		#region overrided methods

		protected override void OnEnable()
		{
			base.OnEnable();
			bundleManifest = target as BundleManifest;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			if (GUILayout.Button("LoadMenifest"))
				LoadMenifest();
			if (GUILayout.Button("CopyBundlesToStreamingAssets"))
				CopyBundlesToStreamingAssets();
		}

		#endregion

		#region private methods
		private void LoadMenifest()
		{
			string path = Application.dataPath;
			path = path.Replace("Assets", "AssetBundles/" + UnityEditor.EditorUserBuildSettings.activeBuildTarget + "/");
			AssetBundle assetBundle = AssetBundle.LoadFromFile(path + bundleManifest.manifetName);
			if (assetBundle == null)
				return;
			AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
			if (manifest == null)
				return;
			string[] bundles = manifest.GetAllAssetBundles();
			bundleManifest.bundleMapping.Clear();
			for (int i = 0; i < bundles.Length; i++)
			{
				if (!bundleManifest.bundleMapping.ContainsKey(bundles[i]))
				{
					AssetBundleConfig config = new AssetBundleConfig();
					config.bundleName = bundles[i];
					config.hash = manifest.GetAssetBundleHash(bundles[i]).ToString();
					config.dependencies = manifest.GetAllDependencies(bundles[i]);
					AssetBundle b = AssetBundle.LoadFromFile(path + bundles[i]);
					config.assetPath = new Dictionary<string, string>();
					string[] assetPaths = b.GetAllAssetNames();
					for (int j = 0; j < assetPaths.Length; j++)
					{
						string fileName = GetFileName(assetPaths[i]);
						if (!config.assetPath.ContainsKey(fileName))
							config.assetPath.Add(fileName, assetPaths[j]);
					}
					b.Unload(true);
					bundleManifest.bundleMapping.Add(bundles[i], config);
				}
			}
			assetBundle.Unload(true);
			UnityEditor.EditorUtility.SetDirty(this);
		}
		private void CopyBundlesToStreamingAssets()
		{
			string path = Application.dataPath;
			path = path.Replace("Assets", "AssetBundles/" + UnityEditor.EditorUserBuildSettings.activeBuildTarget + "/");
			string streamingAssetsPath = Application.dataPath + "/StreamingAssets/Bundles";
			string[] pathSplit = streamingAssetsPath.Split("/");
			string pathForCheck = pathSplit[0];
			for (int i = 1; i < pathSplit.Length; i++)
			{
				pathForCheck = pathForCheck + "/" + pathSplit[i];
				DirectoryInfo directoryInfo = new DirectoryInfo(pathForCheck);
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
					AssetDatabase.Refresh();
				}
			}
			string[] files = Directory.GetFiles(streamingAssetsPath);
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo file = new FileInfo(files[i]);
				file.Delete();
				AssetDatabase.Refresh();
			}
			streamingAssetsPath += "/";
			foreach (var item in bundleManifest.bundleMapping)
			{
				if (!item.Value.isLocalBundle)
					continue;
				CopyBundle(path + item.Key, streamingAssetsPath + item.Key);
			}
		}

		private void CopyBundle(string mainPath, string streamingAssetsPath)
		{
			FileInfo origaleBundle = new FileInfo(mainPath);
			FileInfo origaleBundleManifest = new FileInfo(mainPath + ".manifest");
			if (!origaleBundle.Exists)
			{
				Debug.LogError("Bundel Not Found " + origaleBundle.FullName);
				return;
			}
			if (!origaleBundleManifest.Exists)
			{
				Debug.LogError("Bundel Manifest Not Found " + origaleBundleManifest.FullName);
				return;
			}
			File.Copy(origaleBundle.FullName, streamingAssetsPath);
			File.Copy(origaleBundleManifest.FullName, streamingAssetsPath + ".manifest");
			AssetDatabase.Refresh();
			return;
		}

		private string GetFileName(string path)
		{
			string[] folders = path.Split("/");
			string[] fName = folders[folders.Length - 1].Split(".");
			return fName[0];
		}
		#endregion
	}
}