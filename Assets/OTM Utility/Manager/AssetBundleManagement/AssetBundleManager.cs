using com.otm.utility;
using System;
using UnityEngine;

namespace com.otm.manager
{
	public class AssetBundleManager : Manager<AssetBundleManager>
	{
		#region private methods
		[SerializeField] private BundleManifest manifest;
		[SerializeField] private int maxRetry = 1;
		[SerializeField] private string androidURL;
		[SerializeField] private string isoURL;
		[SerializeField] private bool useExistBundle;
		private BaseBundleHandler handler;
		#endregion

		#region propertices

		public static bool Initlized => Instance != null && Instance.handler != null && Instance.handler.Initlized;

		#endregion

		#region overrided methods

		public override void Awake()
		{
			base.Awake();
			if (handler != null)
				Destroy(handler);
#if UNITY_EDITOR
			if (useExistBundle)
			{
				InitHandler();
				return;
			}
			handler = gameObject.AddComponent<EditorBundleHandler>();
			handler.Init(manifest, "", maxRetry);
#else
			InitHandler();
#endif

		}

		#endregion

		#region public methods

		public void LoadAsset<T>(string bundleName, string assetName, Action<T> onComplete) where T : UnityEngine.Object
		{
			handler.LoadAsset<T>(bundleName, assetName, onComplete);
		}

		public void DownloadBundle(string bundleName, Action<bool> onComplete)
		{
			handler.DownloadBundle(bundleName, onComplete);
		}

		public void UnloasAllBundle()
		{
			handler.UnloasAllBundle();
		}

		public void UnloadBundle(string bundleName)
		{
			handler.UnloadBundle(bundleName);
		}

		public bool IsBundleAvailable(string bundleName)
		{
			return handler.IsBundleAvailable(bundleName);
		}

		public void GetBundleDownloadSize(string bundleName, Action<bool, long> onComplete)
		{
			handler.GetBundleDownloadSize(bundleName, onComplete);
		}

		#endregion

		#region private methods

		private void InitHandler()
		{
			handler = gameObject.AddComponent<DeviceBundleHandler>();
			if (Application.platform == RuntimePlatform.Android)
				handler.Init(manifest, androidURL, maxRetry);
			else if (Application.platform == RuntimePlatform.IPhonePlayer)
				handler.Init(manifest, isoURL, maxRetry);
		}

		#endregion
	}
}
