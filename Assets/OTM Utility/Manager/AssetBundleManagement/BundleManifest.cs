using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.utility
{
	[CreateAssetMenu(menuName = "OTM/AssetBundleManifest")]
	public class BundleManifest : SerializedScriptableObject
	{
		public string manifetName;
		public Dictionary<string, AssetBundleConfig> bundleMapping = new Dictionary<string, AssetBundleConfig>();
	}

	[HideReferenceObjectPicker]
	public class AssetBundleConfig
	{
		public string bundleName;
		public string hash;
		public bool isLocalBundle;
		public string[] dependencies = new string[0];
#if UNITY_EDITOR
		public Dictionary<string, string> assetPath = new Dictionary<string, string>();
#endif

		#region public methods

		public bool CanLoadBundle()
		{
			if (isLocalBundle)
				return true;
			return !string.IsNullOrEmpty(hash);
		}

		#endregion
	}
}
