#if OTM_Firebase
using com.otm.utility;
using Firebase.Extensions;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.otm.firebase
{
	public class FirebaseRemoteConfig : SerializedMonoBehaviour
	{
		#region private veriabels

		[SerializeField] private RemoteConfigData[] defaultData = new RemoteConfigData[0];
		private Firebase.RemoteConfig.FirebaseRemoteConfig firebaseRemoteConfig;
		[ShowInInspector]private Dictionary<string, string> remoteData = new Dictionary<string, string>();
		private bool isLoading;
		private const float InitTimeout = 15f;

		#endregion

		#region public veriables

		[HideInInspector] public bool isInit = false;

		#endregion

		#region propertice

		#endregion

		#region public methods

		public void Init()
		{
			if (isInit)
				return;

			CompleteWithDefaultData();
			try
			{
				firebaseRemoteConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.GetInstance(FirebaseManager.App);
				isLoading = true;
				StartCoroutine(InitTimeoutCo());
				SetDefaultValue();
			}
			catch (Exception e)
			{
				Debug.LogError("Firebase Remote Config init failed: " + e);
				isLoading = false;
			}
		}

		public T GetValue<T>(string key)
		{
			if (remoteData.ContainsKey(key))
				return JsonConvert.DeserializeObject<T>(remoteData[key]);
			if (firebaseRemoteConfig != null && firebaseRemoteConfig.Keys.Contains(key))
				return JsonConvert.DeserializeObject<T>(firebaseRemoteConfig.GetValue(key).StringValue);
			return default(T);
		}

		public string GetValue(string key)
		{
			if (remoteData.ContainsKey(key))
				return remoteData[key];
			if (firebaseRemoteConfig != null && firebaseRemoteConfig.Keys.Contains(key))
				return firebaseRemoteConfig.GetValue(key).StringValue;
			return "";
		}

		#endregion

		#region private methods

		private void SetDefaultValue()
		{
			Dictionary<string, object> data = new Dictionary<string, object>();
			//Set Defalut Data

			for (int i = 0; i < defaultData.Length; i++)
			{
				if (!data.ContainsKey(defaultData[i].key))
					data.Add(defaultData[i].key, defaultData[i].value);
			}
			try
			{
				firebaseRemoteConfig.SetDefaultsAsync(data).ContinueWithOnMainThread(FetchSetting);
			}
			catch (Exception e)
			{
				Debug.LogError("Firebase Remote Config set defaults failed: " + e);
				isLoading = false;
			}
		}

		private void FetchSetting(System.Threading.Tasks.Task task)
		{
			if (!task.IsFaulted && task.IsCompleted && !task.IsCanceled)
			{
				try
				{
					System.Threading.Tasks.Task fetchTask = firebaseRemoteConfig.FetchAsync(TimeSpan.Zero);
					fetchTask.ContinueWithOnMainThread(ActiveSetting);
				}
				catch (Exception e)
				{
					Debug.LogError("Firebase Remote Config fetch failed: " + e);
					isLoading = false;
				}
				return;
			}

			Debug.LogError("Firebase Remote Config set defaults task failed: " + task.Status);
			isLoading = false;
		}

		private void ActiveSetting(System.Threading.Tasks.Task task)
		{
			if (!task.IsFaulted && task.IsCompleted && !task.IsCanceled)
			{
				try
				{
					firebaseRemoteConfig.ActivateAsync().ContinueWithOnMainThread(ActivateSetting);
				}
				catch (Exception e)
				{
					Debug.LogError("Firebase Remote Config activate failed: " + e);
					FillSetting();
				}
				return;
			}
			Debug.LogError("Firebase Remote Config fetch task failed: " + task.Status);
			isLoading = false;
		}

		private void ActivateSetting(System.Threading.Tasks.Task<bool> task)
		{
			if (task.IsFaulted || task.IsCanceled)
				Debug.LogError("Firebase Remote Config activate task failed: " + task.Status);

			FillSetting();
		}

		private void FillSetting()
		{
			//Set Replace default data to remote data
			for (int i = 0; i < defaultData.Length; i++)
			{
				if (firebaseRemoteConfig != null && firebaseRemoteConfig.Keys.Contains(defaultData[i].key))
					remoteData[defaultData[i].key] = firebaseRemoteConfig.GetValue(defaultData[i].key).StringValue;
				else if (!remoteData.ContainsKey(defaultData[i].key))
					remoteData.Add(defaultData[i].key, defaultData[i].value);
			}
			isLoading = false;
			isInit = true;
		}

		private void CompleteWithDefaultData()
		{
			for (int i = 0; i < defaultData.Length; i++)
			{
				if (!remoteData.ContainsKey(defaultData[i].key))
					remoteData.Add(defaultData[i].key, defaultData[i].value);
			}

			isLoading = false;
			isInit = true;
		}

		private IEnumerator InitTimeoutCo()
		{
			float waitTime = 0f;
			while (isLoading)
			{
				waitTime += Time.unscaledDeltaTime;
				if (waitTime >= InitTimeout)
				{
					Debug.LogWarning("Firebase Remote Config fetch timed out. Keeping default values.");
					isLoading = false;
					yield break;
				}

				yield return null;
			}
		}

		#endregion
	}

	public class FirebaseRemoteConfigConstant
	{
		public const string INTER_START_LEVEL = "inter_start_level";
		public const string INTER_CAPPING = "inter_capping";
		public const string DAILY_INTER_CAPPING = "daily_inter_capping";
		public const string CONNECTION_ENABLE = "connection_enable";
		public const string CONNECTION_ENABLE_START_LEVEL = "connection_enable_start_level";
		public const string FIRST_RATE_US_LEVEL = "first_rate_us_level";
		public const string RATE_US_LEVEL_INTERVAL = "rate_us_level_interval";
		public const string INTER_LEVEL_GAP = "inter_level_gap";
		public const string AD_ENABLE = "ad_enable";
		public const string OFFERS_ID = "offers_id";
		public const string IS_INTERNET_FORCE_CHECK = "is_internet_force_check";
		public const string INTERNET_CHECK_LEVEL_NUMBER = "internet_check_level_number";
	}

	public struct RemoteConfigData
	{
		[StringConstantDropDown(typeof(FirebaseRemoteConfigConstant))] public string key;
		public string value;
	}
}
#endif
