using com.otm.utility;
using Newtonsoft.Json;
using SimpleDiskUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace com.otm.manager
{
	public class DeviceBundleHandler : BaseBundleHandler
	{
		#region private variables
		private AssetBundleManifest remoteManifest;
		private string bundlePath;

		#endregion

		#region overrided methods

		public override void Init(BundleManifest manifest, string url, int maxTry = 3)
		{
			base.Init(manifest, url, maxTry);
			bundlePath = Application.persistentDataPath + "/bundles";
			CheckMenifest();
		}

		public override void LoadAsset<T>(string bundleName, string assetName, Action<T> onComplete)
		{
			base.LoadAsset(bundleName, assetName, onComplete);
			if (!localManifestData.ContainsKey(bundleName))
			{
				onComplete?.Invoke(null);
				return;
			}
			if (IsBundleAvailable(bundleName))
			{
				StartCoroutine(LoadAssetFromBundle(bundleName, assetName, onComplete));
				return;
			}
			StartCoroutine(DownloadBundleWithDependencies(bundleName, b =>
			{
				if (b && IsBundleAvailable(bundleName))
					StartCoroutine(LoadAssetFromBundle(bundleName, assetName, onComplete));
				else
					onComplete?.Invoke(null);
			}));
		}

		public override void DownloadBundle(string bundleName, Action<bool> onComplete)
		{
			base.DownloadBundle(bundleName, onComplete);
			if (IsBundleAvailable(bundleName))
			{
				onComplete?.Invoke(true);
				return;
			}
			StartCoroutine(DownloadBundleWithDependencies(bundleName, onComplete));
		}

		public override bool IsBundleAvailable(string bundleName)
		{
			return IsAllDependencisLoaded(bundleName);
		}

		public override void GetBundleDownloadSize(string bundleName, Action<bool, long> onComplete)
		{
			if (!localManifestData.ContainsKey(bundleName) || !IsBundleAvailable(bundleName))
			{
				onComplete?.Invoke(false, 0);
				return;
			}
			if (IsBundleAvailable(bundleName))
			{
				onComplete?.Invoke(true, 0);
				return;
			}
			StartCoroutine(GetBundleSizeWithDependencies(bundleName, onComplete));
		}

		#endregion

		#region protected virtual methods
		protected virtual void CheckMenifest()
		{
			StartCoroutine(DownloadBundleManifest(VerifyManifest));
		}

		protected virtual void VerifyManifest(AssetBundle bundle)
		{
			if (bundle == null)
			{
				Initlized = true;
				return;
			}
			remoteManifest = bundle.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
			UpdateManifest(remoteManifest);
			DeleteExtraBundle();
			CheckForAvailableBundle();
			SaveManifestData();
			Initlized = true;
		}

		protected virtual void UpdateManifest(AssetBundleManifest manifest)
		{
			if (manifest == null)
				return;
			string[] bundles = manifest.GetAllAssetBundles();
			for (int i = 0; i < bundles.Length; i++)
			{
				if (localManifestData.ContainsKey(bundles[i]))
				{
					if (!localManifestData[bundles[i]].hash.Equals(manifest.GetAssetBundleHash(bundles[i]).ToString()))
						localManifestData[bundles[i]].hash = "";
					continue;
				}
				AssetBundleConfig config = new AssetBundleConfig();
				config.bundleName = bundles[i];
				localManifestData.Add(bundles[i], config);
			}
		}

		protected virtual void DeleteExtraBundle()
		{
			if (!Directory.Exists(bundlePath) || remoteManifest == null)
				return;
			string[] bundles = remoteManifest.GetAllAssetBundles();
			string[] files = Directory.GetFiles(bundlePath);
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo file = new FileInfo(files[i]);
				if (!bundles.Contains(file.Name))
					file.Delete();
			}
		}

		protected virtual void SaveManifestData()
		{
			PlayerPrefsHelper.SetString(MANIFEST_KEY, JsonConvert.SerializeObject(localManifestData));
		}

		protected virtual void LoadManifestData()
		{
			string jsonData = PlayerPrefsHelper.GetString(MANIFEST_KEY, "");
			if (string.IsNullOrEmpty(jsonData))
				localManifestData = new Dictionary<string, AssetBundleConfig>(manifest.bundleMapping);
			else
				localManifestData = JsonConvert.DeserializeObject<Dictionary<string, AssetBundleConfig>>(jsonData);
		}

		protected virtual void CheckForAvailableBundle()
		{
			if (!Directory.Exists(bundlePath))
				Directory.CreateDirectory(bundlePath);
			foreach (var item in localManifestData)
			{
				FileInfo file = new FileInfo(bundlePath + "/" + item.Key);
				if (!file.Exists || string.IsNullOrEmpty(item.Value.hash))
					item.Value.hash = "";
			}
		}

		protected virtual bool IsAllDependencisLoaded(string bundleName)
		{
			if (!localManifestData.ContainsKey(bundleName))
				return false;
			AssetBundleConfig config = localManifestData[bundleName];
			if (config.isLocalBundle)
				return true;
			for (int i = 0; i < config.dependencies.Length; i++)
			{
				if (!localManifestData.ContainsKey(config.dependencies[i]) || !localManifestData[config.dependencies[i]].CanLoadBundle())
					return false;
			}
			return localManifestData[bundleName].CanLoadBundle();
		}

		#endregion

		#region coroutine

		IEnumerator LoadAssetFromBundle<T>(string bundleName, string assetName, Action<T> onComplete) where T : UnityEngine.Object
		{
			for (int i = 0; i < localManifestData[bundleName].dependencies.Length; i++)
				yield return StartCoroutine(LoadBundle(localManifestData[bundleName].dependencies[i]));
			yield return StartCoroutine(LoadBundle(bundleName));
			for (int i = 0; i < localManifestData[bundleName].dependencies.Length; i++)
			{
				if (!loadedBundles.ContainsKey(localManifestData[bundleName].dependencies[i]))
				{
					onComplete?.Invoke(null);
					yield break;
				}
			}
			if (!loadedBundles.ContainsKey(bundleName))
			{
				onComplete?.Invoke(null);
				yield break;
			}
			AssetBundleRequest assetBundleRequest = loadedBundles[bundleName].LoadAssetAsync<T>(assetName);
			while (!assetBundleRequest.isDone)
				yield return null;
			onComplete?.Invoke((T)assetBundleRequest.asset);
		}

		IEnumerator LoadBundle(string bundleName)
		{
			if (!loadedBundles.ContainsKey(bundleName))
			{
				if (!localManifestData.ContainsKey(bundleName) || !localManifestData[bundleName].CanLoadBundle())
					yield break;
				if (localManifestData[bundleName].isLocalBundle)
					yield return LoadFromStreamingAssets(bundleName);
				else
					yield return LoadFromFile(bundleName);
			}
		}

		IEnumerator LoadFromStreamingAssets(string bundleName)
		{
			AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/Bundles/" + bundleName);
			while (!assetBundleCreateRequest.isDone)
				yield return null;
			if (assetBundleCreateRequest.assetBundle == null)
				yield break;
			loadedBundles[bundleName] = assetBundleCreateRequest.assetBundle;
		}

		IEnumerator LoadFromFile(string bundleName)
		{
			AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(bundlePath + "/" + bundleName);
			while (!assetBundleCreateRequest.isDone)
				yield return null;
			if (assetBundleCreateRequest.assetBundle == null)
				yield break;
			loadedBundles[bundleName] = assetBundleCreateRequest.assetBundle;
		}

		IEnumerator DownloadBundleWithDependencies(string bundleName, Action<bool> onComplete)
		{
			bool isSizeAvailable = false;
			long bundleSize = 0;
			yield return StartCoroutine(GetBundleSizeWithDependencies(bundleName, (b, l) => { isSizeAvailable = b; bundleSize = l; }));
			bundleSize /= 1048576;//(1024*1024) convert byte in to mb
			if (!isSizeAvailable || DiskUtils.CheckAvailableSpace() <= bundleSize)
			{
				onComplete?.Invoke(false);
				yield break;
			}
			bool isSuccess = false;
			yield return StartCoroutine(DownloadBundle(bundleName, false, b => isSuccess = b));
			if (!isSuccess)
			{
				onComplete?.Invoke(false);
				yield break;
			}
			for (int i = 0; i < localManifestData[bundleName].dependencies.Length; i++)
			{
				yield return StartCoroutine(DownloadBundle(localManifestData[bundleName].dependencies[i], false, b => isSuccess = b));
				if (!isSuccess)
				{
					onComplete?.Invoke(false);
					yield break;
				}
			}
			onComplete?.Invoke(true);
		}

		IEnumerator DownloadBundle(string bundleName, bool checkForDisk, Action<bool> onComplete)
		{
			int downloadTry = maxTry;
			if (checkForDisk)
			{
				bool isSizeAvailable = false;
				long bundleSize = 0;
				yield return StartCoroutine(GetBundleSize(bundleName, (b, l) => { isSizeAvailable = b; bundleSize = l; }));
				bundleSize /= 1048576;//(1024*1024) convert byte in to mb
				if (!isSizeAvailable || DiskUtils.CheckAvailableSpace() <= bundleSize)
				{
					onComplete?.Invoke(false);
					yield break;
				}
			}
			while (maxTry > 0)
			{
				UnityWebRequest request = new UnityWebRequest(url + bundleName);
				request.downloadHandler = new DownloadHandlerFile(bundlePath + "/" + bundleName);
				yield return request.SendWebRequest();
				if (request.result != UnityWebRequest.Result.Success)
				{
					downloadTry--;
					continue;
				}
				if (localManifestData.ContainsKey(bundleName))
				{
					localManifestData[bundleName].hash = remoteManifest.GetAssetBundleHash(bundleName).ToString();
					localManifestData[bundleName].dependencies = remoteManifest.GetAllDependencies(bundleName);
					SaveManifestData();
					onComplete?.Invoke(true);
					yield break;
				}
			}
			onComplete?.Invoke(false);
		}

		IEnumerator DownloadBundleManifest(Action<AssetBundle> onComplete)
		{
			UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url + manifest.manifetName);
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success)
			{
				onComplete?.Invoke(null);
				yield break;
			}
			AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(request);
			onComplete?.Invoke(assetBundle);
		}

		IEnumerator GetBundleSizeWithDependencies(string bundleName, Action<bool, long> onComplete)
		{
			long size = 0;
			bool isSuccess = false;
			yield return StartCoroutine(GetBundleSize(bundleName, (b, l) => { size += l; isSuccess = b; }));
			if (!isSuccess)
			{
				onComplete?.Invoke(false, 0);
				yield break;
			}
			for (int i = 0; i < localManifestData[bundleName].dependencies.Length; i++)
			{
				yield return StartCoroutine(GetBundleSize(localManifestData[bundleName].dependencies[i], (b, l) => { size += l; isSuccess = b; }));
				if (!isSuccess)
				{
					onComplete?.Invoke(false, 0);
					yield break;
				}
			}
			onComplete?.Invoke(true, size);
		}

		IEnumerator GetBundleSize(string bundleName, Action<bool, long> onComplete)
		{
			if (localManifestData.ContainsKey(bundleName) && localManifestData[bundleName].CanLoadBundle())
				onComplete?.Invoke(true, 0);
			int downloadTry = maxTry;
			while (maxTry > 0)
			{
				UnityWebRequest request = UnityWebRequest.Head(url + bundleName);
				yield return request.SendWebRequest();
				if (request.result != UnityWebRequest.Result.Success)
				{
					downloadTry--;
					continue;
				}
				string contentLength = request.GetResponseHeader("Content-Length");
				if (!string.IsNullOrEmpty(contentLength))
					onComplete?.Invoke(true, long.Parse(contentLength));
				else
					onComplete?.Invoke(true, 0);
				yield break;
			}
			onComplete?.Invoke(false, 0);
		}

		#endregion
	}
}
