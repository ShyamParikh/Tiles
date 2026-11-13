#if OTM_Addressable
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace com.otm.editor
{

	public static class AssetBundleEditorMenu
	{
		[UnityEditor.MenuItem("OTM TOOLS/AssetBundle/OpenCacheFolder")]
		public static void OpenCacheFolder()
		{
			Application.OpenURL(Application.persistentDataPath);
		}

		[UnityEditor.MenuItem("OTM TOOLS/AssetBundle/OpenAssetBundleFolder")]
		public static void OpenAssetBundleFolder()
		{
			string path = Application.dataPath.Replace("Assets", "AssetBundles");
			Application.OpenURL(path);
		}

		[UnityEditor.MenuItem("OTM TOOLS/AssetBundle/ClearCache")]
		public static void ClearCache()
		{
			Addressables.CleanBundleCache();
			Caching.ClearCache();
			string bundlePath = Application.persistentDataPath + "/bundles";
			if (!Directory.Exists(bundlePath))
				return;
			string[] files = Directory.GetFiles(bundlePath);
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo file = new FileInfo(files[i]);
				file.Delete();
			}
		}
	}

}
#endif