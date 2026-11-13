#if OTM_Addressable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.otm.manager
{
	public class AddressableManager : Manager<AddressableManager>
	{
		#region private variables

		private BundleLoader[] bundleLoaders = new BundleLoader[] { new AddressableBundleLoader() };

		#endregion

		#region propertices

		#endregion

		#region overrided methods

		public override void Awake()
		{
			for (int i = 0; i < bundleLoaders.Length; i++)
			{
				bundleLoaders[i].Init();
			}
			base.Awake();
		}

        public override void OnDestroy()
        {
			for (int i = 0; i < bundleLoaders.Length; i++)
			{
				bundleLoaders[i].UnLoadAllLodedAssets();
				bundleLoaders[i].UnloadStrictBundles();
				bundleLoaders[i].Reset();
			}
			StopAllCoroutines();
            base.OnDestroy();
        }

		#endregion

		#region private methods

		#endregion

		#region public methods

		public void LoadAssetAsync<T>(string assetName, Action<T> onSuccess, Action onLoadFailed = null, AssetLoadMode assetLoadMode = AssetLoadMode.Loose)
		{
			for (int i = 0; i < bundleLoaders.Length; i++)
			{
				if (bundleLoaders[i].IsValid(assetName))
				{
					bundleLoaders[i].LoadAssetAsync(assetName, onSuccess, onLoadFailed, assetLoadMode);
					break;
				}
			}
		}
		public void LoadAssetAsyncWithHandle<T>(string assetName, Action<T, BundleHandle> onSuccess, Action onLoadFailed = null, AssetLoadMode assetLoadMode = AssetLoadMode.Loose)
		{
			for (int i = 0; i < bundleLoaders.Length; i++)
			{
				if (bundleLoaders[i].IsValid(assetName))
				{
					bundleLoaders[i].LoadAssetAsyncWithHandle(assetName, onSuccess, onLoadFailed, assetLoadMode);
					break;
				}
			}
		}

		public void LoadAssets<T>(IList<string> assetNames, Action<Dictionary<string, T>, Dictionary<string, BundleHandle>> loadSuccess, AssetLoadMode assetLoadMode = AssetLoadMode.Loose, Action loadFailed = null)
		{
			StartCoroutine(LoadBundles(assetNames, loadSuccess, assetLoadMode, loadFailed));
		}

		public void UnloadAsset(string assetName)
		{
			for (int i = 0; i < bundleLoaders.Length; i++)
			{
				if (bundleLoaders[i].IsValid(assetName))
					bundleLoaders[i].UnloadAsset(assetName);
			}
		}

		public void UnLoadAsset(BundleHandle bundleHandle)
		{
			for (int i = 0; i < bundleLoaders.Length; i++)
			{
				if (bundleLoaders[i].IsValid(bundleHandle.assetName))
					bundleLoaders[i].UnLoadAsset(bundleHandle);
			}
		}

		public void UnLoadAllLodedAsset(bool includeScrictBundle)
		{
			for (int i = 0; i < bundleLoaders.Length; i++)
			{
				bundleLoaders[i].UnLoadAllLodedAssets();
				if (includeScrictBundle)
					bundleLoaders[i].UnloadStrictBundles();
			}
		}

		public void GetDownloadSize(string assetName, Action<long> onComplte, Action onFailed)
		{
			for (int i = 0; i < bundleLoaders.Length; i++)
			{
				if (bundleLoaders[i].IsValid(assetName))
				{
					bundleLoaders[i].GetDownloadSize(assetName, onComplte, onFailed);
					break;
				}
			}
		}

		public void GetDownloadSize(string[] assetNames, Action<long> onComplte, Action onFailed)
		{
			StartCoroutine(FindNeedToDownloadBundle(assetNames, (b) =>
			{
				long totalSize = 0;
				foreach (var item in b)
				{
					totalSize += item.Value;
				}
				onComplte?.Invoke(totalSize);
			}, onFailed));
		}

		public void DownloadBundle(string assetName, Action onComplte, Action onFailed, Action<long, float> onUpdate)
		{
			for (int i = 0; i < bundleLoaders.Length; i++)
			{
				if (bundleLoaders[i].IsValid(assetName))
				{
					Debug.Log("Downloading : " + assetName);
					bundleLoaders[i].DownloadBundle(assetName, onComplte, onFailed, onUpdate);
					break;
				}
			}
		}

		public bool IsBundleDownloading(string assetName)
		{
			for (int i = 0; i < bundleLoaders.Length; i++)
			{
				if (bundleLoaders[i].IsValid(assetName))
				{
					return bundleLoaders[i].IsBundleDownloading(assetName);
				}
			}
			return false;
		}

		public void DownloadBundleInBackGround(string[] bundles)
		{
			if (bundles == null || bundles.Length <= 0)
				return;
			StartCoroutine(DownloadInBackgroundBundle(bundles));
		}

		public void DownloadBundles(string[] bundles, Action<long> onBundleDownloadingStart, Action onComplte, Action onFailed, Action<long, float> onUpdate)
		{
			if (bundles == null || bundles.Length <= 0)
				return;
			StartCoroutine(DownloadBundleAsyc(bundles, onBundleDownloadingStart, onComplte, onFailed, onUpdate));
		}

		public void LoadWithGroup<T>(string groupName, Action<List<T>> onComplete, AssetLoadMode assetLoadMode, Action onFailed = null)
		{
			for (int i = 0; i < bundleLoaders.Length; i++)
			{
				if (bundleLoaders[i] is AddressableBundleLoader)
				{
					bundleLoaders[i].LoadWithGroup(groupName, onComplete, assetLoadMode, onFailed);
					return;
				}
			}
			onFailed?.Invoke();
		}

		public float GetDownloadProgress(string assetName)
		{
			for (int i = 0; i < bundleLoaders.Length; i++)
			{
				if (bundleLoaders[i].IsValid(assetName))
				{
					return bundleLoaders[i].GetDownloadProgress(assetName);
				}
			}
			return 0;
		}

		#endregion

		IEnumerator DownloadBundleAsyc(string[] bundles, Action<long> onBundleDownloadingStart, Action onComplte, Action onFailed, Action<long, float> onUpdate)
		{
			Dictionary<string, long> bundleNeedToDownload = new Dictionary<string, long>();
			Dictionary<string, long> downloadedSize = new Dictionary<string, long>();
			List<string> downloadCompleteBundles = new List<string>();
			bool isFailed = false;
			long totalSize = 0;
			yield return StartCoroutine(FindNeedToDownloadBundle(bundles, (b) => { bundleNeedToDownload = b; }, () => { isFailed = true; }));
			if (isFailed)
			{
				onFailed?.Invoke();
				yield break;
			}
			if (bundleNeedToDownload.Count <= 0)
			{
				onComplte?.Invoke();
				yield break;
			}
			foreach (var item in bundleNeedToDownload)
				totalSize += item.Value;
			onBundleDownloadingStart?.Invoke(totalSize);
			long tempSize;
			foreach (var item in bundleNeedToDownload)
			{
				DownloadBundle(item.Key, () => { downloadCompleteBundles.Add(item.Key); }, null, (l, f) =>
				{
					if (!downloadedSize.ContainsKey(item.Key))
						downloadedSize.Add(item.Key, l);
					downloadedSize[item.Key] = l;
					tempSize = 0;
					foreach (var item in downloadedSize)
					{
						tempSize += item.Value;
					}
					onUpdate?.Invoke(tempSize, (tempSize * 100 / totalSize));
				});
				while (IsBundleDownloading(item.Key))
					yield return null;
			}

			if (downloadCompleteBundles.Count < bundleNeedToDownload.Count)
			{
				onFailed?.Invoke();
				yield break;
			}
			onComplte?.Invoke();
		}

		IEnumerator DownloadInBackgroundBundle(string[] bundles)
		{
			while (!InternetManager.Instance.IsReachableToNetwork())
				yield return null;
			List<string> bundleNeedToDownload = new List<string>();
			yield return StartCoroutine(FindNeedToDownloadBundle(bundles, (b) => { bundleNeedToDownload = b.Keys.ToList(); }));
			for (int i = 0; i < bundleNeedToDownload.Count; i++)
			{
				DownloadBundle(bundleNeedToDownload[i], () =>
				{
					Debug.Log("Bundle Download Complete" + bundleNeedToDownload[i]);
				}, null, null);
				while (IsBundleDownloading(bundleNeedToDownload[i]))
					yield return null;
			}
		}

		IEnumerator FindNeedToDownloadBundle(string[] bundles, Action<Dictionary<string, long>> onComplete, Action onFailed = null)
		{
			Dictionary<string, long> needToDownload = new Dictionary<string, long>();
			for (int i = 0; i < bundles.Length; i++)
			{
				if (IsBundleDownloading(bundles[i]))
					continue;
				bool isWaiting = true;
				GetDownloadSize(bundles[i], size =>
				{
					isWaiting = false;
					if (size > 0)
					{
						needToDownload.Add(bundles[i], size);
						Debug.Log("Added To download : " + bundles[i]);
					}
					else
					{
						Debug.Log("Bundle is Uptodate : " + bundles[i]);
					}
				}, () =>
				{
					onFailed?.Invoke();
					isWaiting = false;
				});
				while (isWaiting)
					yield return null;
			}
			onComplete?.Invoke(needToDownload);
		}

		IEnumerator LoadBundles<T>(IList<string> assetNames, Action<Dictionary<string, T>, Dictionary<string, BundleHandle>> loadSuccess, AssetLoadMode assetLoadMode = AssetLoadMode.Loose, Action loadFailed = null)
		{
			bool isLoading = false;
			Dictionary<string, T> assets = new Dictionary<string, T>();
			Dictionary<string, BundleHandle> handles = new Dictionary<string, BundleHandle>();
			for (int i = 0; i < assetNames.Count; i++)
			{
				for (int j = 0; j < bundleLoaders.Length; j++)
				{
					if (bundleLoaders[j].IsValid(assetNames[i]))
					{
						isLoading = true;
						bundleLoaders[j].LoadAssetAsyncWithHandle(assetNames[i], (T t, BundleHandle handle) =>
						{
							if (!assets.ContainsKey(assetNames[i]))
								assets.Add(assetNames[i], t);
							if (!handles.ContainsKey(assetNames[i]))
								handles.Add(assetNames[i], handle);
							isLoading = false;
						}, null, assetLoadMode);
						break;
					}
				}
				while (isLoading)
					yield return null;
			}
			if (assets.Count >= assets.Count)
				loadSuccess?.Invoke(assets, handles);
			else
				loadFailed?.Invoke();
		}
	}
}
#endif