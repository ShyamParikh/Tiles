#if OTM_Addressable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace com.otm.manager
{
	public abstract class BundleLoader
	{
		#region abstract methods
		public abstract void Init();
		public abstract void LoadAssetAsync<T>(string assetName, Action<T> onSuccess, Action onFaild, AssetLoadMode assetLoadMode);
		public abstract void LoadAssetAsyncWithHandle<T>(string assetName, Action<T, BundleHandle> onSuccess, Action onFaild, AssetLoadMode assetLoadMode);
		public abstract void LoadAssets<T>(IList<string> assetNames, Action<Dictionary<string, T>, Dictionary<string, BundleHandle>> loadSuccess, AssetLoadMode assetLoadMode, Action loadFailed = null);
		public abstract void UnloadAsset(string assetName);
		public abstract void UnLoadAsset(BundleHandle bundleHandle);
		public abstract void UnLoadAllLodedAssets();
		public abstract void UnloadStrictBundles();
		public abstract void GetDownloadSize(string assetName, Action<long> onComplete, Action onFailed);
		public abstract void DownloadBundle(string assetName, Action onSuccess, Action onFailed, Action<long, float> onUpdate);
		public abstract bool IsBundleDownloading(string assetName);

		#endregion

		#region virtual methods

		public virtual bool IsValid(string assetName)
		{
			return true;
		}

		public virtual void LoadWithGroup<T>(string groupName, Action<List<T>> onComplete, AssetLoadMode assetLoadMode, Action onFailed = null)
		{
			onFailed?.Invoke();
		}

		public virtual float GetDownloadProgress(string bundleName)
		{
			return 1;
		}

		public virtual void Reset()
		{
		}

		#endregion
	}

	public enum AssetLoadMode
	{
		Loose,
		Strict
	}

	public class BundleHandle
	{
		public string assetName;
		public AsyncOperationHandle asyncOperationHandle;
		public AssetBundle assetBundle;

		public BundleHandle(string assetName, AsyncOperationHandle asyncOperationHandle)
		{
			this.assetName = assetName;
			this.asyncOperationHandle = asyncOperationHandle;
		}

		public BundleHandle(string assetName, AssetBundle assetBundle)
		{
			this.assetName = assetName;
			this.assetBundle = assetBundle;
		}
	}
}
#endif
