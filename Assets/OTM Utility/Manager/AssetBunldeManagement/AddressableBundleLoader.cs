#if OTM_Addressable
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace com.otm.manager
{
	public class AddressableBundleLoader : BundleLoader
	{
		#region private variables

		private Dictionary<string, BundleHandle> loadedBundles = new Dictionary<string, BundleHandle>();
		private Dictionary<string, BundleHandle> strictBundles = new Dictionary<string, BundleHandle>();
		private Dictionary<string, List<DownloadCallbacks>> downloadCallbacks = new Dictionary<string, List<DownloadCallbacks>>();

		private Dictionary<string, float> downloadProgress = new Dictionary<string, float>();

		#endregion

		#region override methods

		public override void Init()
		{

		}

		public override void LoadAssetAsync<T>(string assetName, Action<T> onSuccess, Action loadFailed, AssetLoadMode assetLoadMode)
		{
			AsyncOperationHandle<T> operationHandle = Addressables.LoadAssetAsync<T>(assetName);
			AddToLoadedBundle(assetName, operationHandle, assetLoadMode);
			operationHandle.Completed += handle =>
			{
				string e = GetDownloadError(operationHandle);
				if (!string.IsNullOrEmpty(e))
				{
					Debug.LogError(assetName + " Bundle loading Failed : " + e);
					loadFailed?.Invoke();
					return;
				}
				AddressableManager.Instance.StartCoroutine(WaitForFrame(() =>
				{
					onSuccess?.Invoke(operationHandle.Result);
				}));
			};
		}

		public override void LoadAssetAsyncWithHandle<T>(string assetName, Action<T, BundleHandle> onSuccess, Action loadFailed, AssetLoadMode assetLoadMode)
		{
			AsyncOperationHandle<T> operationHandle = Addressables.LoadAssetAsync<T>(assetName);
			BundleHandle bundleHandle = AddToLoadedBundle(assetName, operationHandle, assetLoadMode);
			operationHandle.Completed += handle =>
			{
				string e = GetDownloadError(operationHandle);
				if (!string.IsNullOrEmpty(e))
				{
					Debug.LogError(assetName + " Bundle loading Failed : " + e);
					loadFailed?.Invoke();
					return;
				}
				AddressableManager.Instance.StartCoroutine(WaitForFrame(() =>
				{
					onSuccess?.Invoke(operationHandle.Result, bundleHandle);
				}));
			};
		}

		public override void LoadAssets<T>(IList<string> assetNames, Action<Dictionary<string, T>, Dictionary<string, BundleHandle>> loadSuccess, AssetLoadMode assetLoadMode, Action loadFailed = null)
		{
			AddressableManager.Instance.StartCoroutine(LoadMultipleAssets<T>(assetNames, loadSuccess, assetLoadMode, loadFailed));
		}

		public override void UnloadAsset(string assetName)
		{
			if (!loadedBundles.ContainsKey(assetName) || strictBundles.ContainsKey(assetName))
				return;
			ReleaseBundleHandle(loadedBundles[assetName]);
			loadedBundles.Remove(assetName);
		}

		public override void UnLoadAsset(BundleHandle bundleHandle)
		{
			if (string.IsNullOrEmpty(bundleHandle.assetName))
				return;
			if (strictBundles.ContainsKey(bundleHandle.assetName))
				return;
			if (!loadedBundles.ContainsKey(bundleHandle.assetName))
				return;
			ReleaseBundleHandle(bundleHandle);
			loadedBundles.Remove(bundleHandle.assetName);
		}

		public override void UnLoadAllLodedAssets()
		{
			List<string> needToRemove = new List<string>();
			foreach (var assetName in loadedBundles)
			{
				if (!strictBundles.ContainsKey(assetName.Key))
					needToRemove.Add(assetName.Key);
			}
			for (int i = 0; i < needToRemove.Count; i++)
			{
				UnloadAsset(needToRemove[i]);
			}
		}

		public override void UnloadStrictBundles()
		{
			List<string> needToRemove = new List<string>();
			foreach (var assetName in strictBundles)
			{
				ReleaseBundleHandle(assetName.Value);
				needToRemove.Add(assetName.Key);
			}
			for (int i = 0; i < needToRemove.Count; i++)
			{
				loadedBundles.Remove(needToRemove[i]);
			}
			strictBundles.Clear();
		}

		public override void Reset()
		{
			strictBundles.Clear();
			loadedBundles.Clear();
			downloadCallbacks.Clear();
			downloadProgress.Clear();
			base.Reset();
		}

		public override void GetDownloadSize(string assetName, Action<long> onComplete, Action onFailed)
		{
			AddressableManager.Instance.StartCoroutine(GetDownloadSizeAsync(assetName, onComplete, onFailed));
		}

		public override void DownloadBundle(string assetName, Action onSuccess, Action onFailed, Action<long, float> onUpdate)
		{
			AddressableManager.Instance.StartCoroutine(DownloadBundleAsync(assetName, onSuccess, onFailed, onUpdate));
		}

		public override bool IsBundleDownloading(string assetName)
		{
			return downloadCallbacks.ContainsKey(assetName);
		}

		public override float GetDownloadProgress(string bundleName)
		{
			if (downloadProgress.ContainsKey(bundleName))
				return downloadProgress[bundleName];
			return base.GetDownloadProgress(bundleName);
		}

		public override void LoadWithGroup<T>(string groupName, Action<List<T>> onComplete, AssetLoadMode assetLoadMode, Action onFailed = null)
		{
			AddressableManager.Instance.StartCoroutine(WaitForAllBundleLoad(groupName, onComplete, assetLoadMode, onFailed));
		}

		#endregion

		#region private methods

		private BundleHandle AddToLoadedBundle(string key, AsyncOperationHandle asyncOperationHandle, AssetLoadMode assetLoadMode)
		{
			if (loadedBundles.ContainsKey(key))
				loadedBundles[key].asyncOperationHandle = asyncOperationHandle;
			else
				loadedBundles.Add(key, new BundleHandle(key, asyncOperationHandle));

			if (assetLoadMode == AssetLoadMode.Strict && !strictBundles.ContainsKey(key))
				strictBundles.Add(key, loadedBundles[key]);
			return loadedBundles[key];
		}

		private string GetDownloadError(AsyncOperationHandle fromHandle)
		{
			try
			{
				if (fromHandle.Equals(default))
					return null;
				if (fromHandle.Status != AsyncOperationStatus.Failed)
					return null;
				RemoteProviderException remoteException;
				Exception e = fromHandle.OperationException;
				while (e != null)
				{
					remoteException = e as RemoteProviderException;
					if (remoteException != null)
						return remoteException.WebRequestResult.Error;
					e = e.InnerException;
				}
			}
			catch
			{
				return null;
			}

			return null;
		}

		private void ReleaseBundleHandle(BundleHandle bundleHandle)
		{
			if (bundleHandle == null)
				return;
			if (bundleHandle.asyncOperationHandle.IsValid())
				Addressables.Release(bundleHandle.asyncOperationHandle);
		}

		#endregion

		#region Coroutine

		IEnumerator WaitForAllBundleLoad<T>(string groupName, Action<List<T>> onComplete, AssetLoadMode assetLoadMode, Action onFailed)
		{
			AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync(groupName, (T obj) => { });
			AddToLoadedBundle(groupName, handle, assetLoadMode);
			while (!handle.IsDone)
				yield return null;
			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				List<T> objs = new List<T>(handle.Result);
				onComplete?.Invoke(objs);
			}
			else
				onFailed?.Invoke();
		}

		IEnumerator GetDownloadSizeAsync(string assetName, Action<long> onComplete, Action onFailed)
		{
			AsyncOperationHandle<long> asyncOperationHandle = Addressables.GetDownloadSizeAsync(assetName);

			yield return asyncOperationHandle;
			if (asyncOperationHandle.Status == AsyncOperationStatus.Failed)
			{
				Debug.LogError(" Failed to Get Bundle size : " + assetName);
				onFailed?.Invoke();
			}
			else
			{
				Debug.Log("DownloadSize : " + asyncOperationHandle.Result);
				onComplete?.Invoke(asyncOperationHandle.Result);
			}
			Addressables.Release(asyncOperationHandle);

		}

		IEnumerator WaitForFrame(Action action)
		{
			yield return new WaitForEndOfFrame();
			action?.Invoke();
		}

		IEnumerator DownloadBundleAsync(string key, Action onComplete, Action onFailed, Action<long, float> onUpdate)
		{
			bool canCoutinue = false;
			if (!downloadCallbacks.ContainsKey(key))
			{
				downloadCallbacks.Add(key, new List<DownloadCallbacks>());
				canCoutinue = true;
			}
			if (!downloadProgress.ContainsKey(key))
				downloadProgress.Add(key, 0);
			downloadCallbacks[key].Add(new DownloadCallbacks(onComplete, onFailed, onUpdate));
			if (!canCoutinue)
				yield break;
			float progress = 0;
			long downloadedSize = 0;
			AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync(key);
			DownloadStatus status = handle.GetDownloadStatus();
			while (!handle.IsDone)
			{
				status = handle.GetDownloadStatus();
				progress = status.Percent;
				downloadProgress[key] = progress;
				downloadedSize = status.DownloadedBytes;
				for (int i = 0; i < downloadCallbacks[key].Count; i++)
					downloadCallbacks[key][i].onUpdate?.Invoke(downloadedSize, progress);
				yield return null;
			}
			if (handle.Status == AsyncOperationStatus.Failed)
			{
				Debug.LogError(key + " : " + handle.OperationException.Message);
				for (int i = 0; i < downloadCallbacks[key].Count; i++)
					downloadCallbacks[key][i].onFailed?.Invoke();
			}
			else
				for (int i = 0; i < downloadCallbacks[key].Count; i++)
					downloadCallbacks[key][i].onComplete?.Invoke();
			downloadCallbacks.Remove(key);
			downloadProgress.Remove(key);
			Addressables.Release(handle);
		}

		IEnumerator LoadMultipleAssets<T>(IList<string> keys, Action<Dictionary<string, T>, Dictionary<string, BundleHandle>> loadSuccess, AssetLoadMode assetLoadMode, Action loadFailed)
		{
			Dictionary<string, T> loadedObject = new Dictionary<string, T>();
			Dictionary<string, BundleHandle> loadedHandles = new Dictionary<string, BundleHandle>();
			AsyncOperationHandle<IList<IResourceLocation>> locations = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, typeof(T));
			yield return locations;
			List<AsyncOperationHandle> loadOps = new List<AsyncOperationHandle>(locations.Result.Count);
			foreach (IResourceLocation location in locations.Result)
			{
				AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(location);
				BundleHandle bundleHandle = AddToLoadedBundle(location.PrimaryKey, handle, assetLoadMode);
				handle.Completed += obj =>
				{
					loadedObject.Add(location.PrimaryKey, obj.Result);
					loadedHandles.Add(location.PrimaryKey, bundleHandle);
				};
				loadOps.Add(handle);
			}

			yield return Addressables.ResourceManager.CreateGenericGroupOperation(loadOps, true);

			yield return new WaitForEndOfFrame();
			if (loadedObject.Count > 0)
				loadSuccess?.Invoke(loadedObject, loadedHandles);
			else
				loadFailed?.Invoke();
		}

		#endregion

		#region class

		public class DownloadCallbacks
		{
			public Action onComplete;
			public Action onFailed;
			public Action<long, float> onUpdate;

			public DownloadCallbacks(Action onComplete, Action onFailed, Action<long, float> onUpdate)
			{
				this.onComplete = onComplete;
				this.onFailed = onFailed;
				this.onUpdate = onUpdate;
			}
		}
		#endregion
	}
}
#endif
