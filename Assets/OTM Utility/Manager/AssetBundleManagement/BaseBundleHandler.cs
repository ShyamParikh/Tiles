using com.otm.utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.manager
{
	public abstract class BaseBundleHandler : MonoBehaviour
	{
		#region protected variables
		protected BundleManifest manifest;
		protected int maxTry = 1;
		protected Dictionary<string, AssetBundleConfig> localManifestData = new Dictionary<string, AssetBundleConfig>();
		protected Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();
		protected string url;

		#endregion

		#region propertices

		public bool Initlized { get; set; }

		#endregion

		#region protected constant

		protected const string MANIFEST_KEY = "LocalManifestData";

		#endregion

		#region virtual methods
		public virtual void Init(BundleManifest manifest, string url, int maxTry = 3)
		{
			this.manifest = manifest;
			this.maxTry = maxTry;
			this.url = url + "/";
		}

		public virtual void LoadAsset<T>(string bundleName, string assetName, Action<T> onComplete) where T : UnityEngine.Object
		{
		}

		public virtual void DownloadBundle(string bundleName, Action<bool> onComplete)
		{
			onComplete?.Invoke(true);
		}

		public virtual void UnloasAllBundle()
		{
			foreach (var item in loadedBundles)
			{
				item.Value.Unload(true);
			}
			loadedBundles.Clear();
		}

		public virtual void UnloadBundle(string bundleName)
		{
			if (!loadedBundles.ContainsKey(bundleName))
				return;
			loadedBundles[bundleName].Unload(true);
			loadedBundles.Remove(bundleName);
		}

		public virtual bool IsBundleAvailable(string bundleName) { return true; }

		public virtual void GetBundleDownloadSize(string bundleName, Action<bool, long> onComplete)
		{
			onComplete?.Invoke(true, 0);
		}

		#endregion


	}
}
