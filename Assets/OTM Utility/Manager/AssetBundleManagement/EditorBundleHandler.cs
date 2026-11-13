using com.otm.utility;
using System;
using System.Collections.Generic;

namespace com.otm.manager
{
#if UNITY_EDITOR
	public class EditorBundleHandler : BaseBundleHandler
	{
		#region overrided methods

		public override void Init(BundleManifest manifest, string url, int maxTry = 3)
		{
			base.Init(manifest, url, maxTry);
			localManifestData = new Dictionary<string, AssetBundleConfig>(manifest.bundleMapping);
			Initlized = true;
		}

		public override void LoadAsset<T>(string bundleName, string assetName, Action<T> onComplete)
		{
			bundleName = bundleName.ToLower();
			assetName = assetName.ToLower();
			if (localManifestData.ContainsKey(bundleName) && localManifestData[bundleName].assetPath.ContainsKey(assetName))
			{
				T obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(localManifestData[bundleName].assetPath[assetName]);
				onComplete?.Invoke(obj);
				return;
			}
			onComplete?.Invoke(null);

		}
		#endregion

	}
#endif
}
