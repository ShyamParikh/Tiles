#if OTM_Firebase
using com.otm.manager;
using Firebase;
using Firebase.Extensions;
using System;
using System.Collections;
using UnityEngine;

namespace com.otm.firebase
{
	public class FirebaseManager : Manager<FirebaseManager>
	{
		#region PUBLIC_VARIABLES

		[SerializeField] private FirebaseCrashlytics crashlytics;
		[SerializeField] private FirebaseRemoteConfig remoteConfig;
		[SerializeField] private FirebaseAnalytics firebaseAnalytics;
		[SerializeField] private FirebaseCloudMessaging firebaseCloudMessaging;
		[SerializeField] private FirebaseApp app;
		[SerializeField] private bool debugLog;

		#endregion


		#region Propertices

		public static FirebaseAnalytics FirebaseAnalytics => Instance.firebaseAnalytics;
		public static FirebaseApp App => Instance.app;
		public static FirebaseRemoteConfig FirebaseRemoteConfig => Instance.remoteConfig;
		public static bool DebugLog => Instance.debugLog;
		public bool IsAllComponentInitlized { get; set; }

		#endregion

		#region PRIVATE_VARIABLES

		private bool isInit = false;
		private bool isCheckingDependencies = false;
		private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
		private const float DependencyCheckTimeout = 30f;
		private const float ComponentInitTimeout = 20f;

		#endregion

		#region UNITY_CALLBACKS

		private void Start()
		{
			IsAllComponentInitlized = false;
			isInit = false;
			OnInternetCheckFinish(true);
		}

		#endregion

		#region PRIVATE_METHODS

		private void OnInternetCheckFinish(bool isInternetAvailable)
		{
			if (isCheckingDependencies)
				return;

			if (TryUseExistingFirebaseApp())
			{
				StartCoroutine(InitWaitCo());
				return;
			}

			isCheckingDependencies = true;
			StartCoroutine(InitWaitCo());
			try
			{
				Debug.Log("Firebase" + 1);
				FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
				{
					Debug.Log("Firebase" + 2);
					isCheckingDependencies = false;
					if (task.IsCanceled)
					{
						dependencyStatus = DependencyStatus.UnavailableOther;
						isInit = true;
						Debug.LogError("Firebase dependency check was canceled.");
						return;
					}

					if (task.IsFaulted)
					{
						dependencyStatus = DependencyStatus.UnavailableOther;
						isInit = true;
						Debug.LogError("Firebase dependency check failed: " + task.Exception);
						return;
					}

					dependencyStatus = task.Result;
					if (dependencyStatus == DependencyStatus.Available)
					{
						Debug.Log("Firebase" + 3);
						app = Firebase.FirebaseApp.DefaultInstance;
						isInit = true;
					}
					else
					{
						Debug.Log("Firebase" + 4);
						isInit = true;
						Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
					}
				});
			}
			catch (Exception e)
			{
				Debug.Log("Firebase" + 5);
				isCheckingDependencies = false;
				isInit = true;
				Debug.LogError(e + "Async task failed");
			}
		}

		private bool TryUseExistingFirebaseApp()
		{
			try
			{
				FirebaseApp existingApp = app != null ? app : FirebaseApp.DefaultInstance;
				if (existingApp == null)
					return false;

				app = existingApp;
				dependencyStatus = DependencyStatus.Available;
				isInit = true;
				Debug.Log("Firebase already initialized, skipping dependency check.");
				return true;
			}
			catch (Exception e)
			{
				Debug.LogWarning("Could not reuse existing Firebase app: " + e.Message);
				return false;
			}
		}


		private void Init()
		{
			FirebaseApp.LogLevel = LogLevel.Error;
			StartCoroutine(LoadAllFirebaseComponent());
		}

		#endregion

		#region PUBLIC_METHODS

		#endregion

		#region PRIVATE_METHODS

		#endregion

		#region COROUTINES

		private IEnumerator InitWaitCo()
		{
			float waitTime = 0f;
			while (!isInit)
			{
				waitTime += Time.unscaledDeltaTime;
				if (waitTime >= DependencyCheckTimeout)
				{
					isCheckingDependencies = false;
					Debug.LogError("Firebase dependency check timed out.");
					OnLoadingDone();
					yield break;
				}
				yield return null;
			}

			if (dependencyStatus == DependencyStatus.Available && app != null)
			{
				Init();
			}
			else
			{
				OnLoadingDone();
			}
		}

		private IEnumerator LoadAllFirebaseComponent()
		{
			if (debugLog)
				Debug.Log("--------------------------Firebase Loading Start--------------------------");
			remoteConfig.Init();
			if (debugLog)
				Debug.Log("remoteConfig loading start");
			yield return WaitForComponentInit(() => remoteConfig.isInit, "remoteConfig");
			if (debugLog)
				Debug.Log("remoteConfig loading Complete");
			if (debugLog)
				Debug.Log("crashlytics loading start");
			crashlytics.Init();
			yield return WaitForComponentInit(() => crashlytics.isInit, "crashlytics");
			if (debugLog)
				Debug.Log("crashlytics loading Complete");
			if (debugLog)
				Debug.Log("firebaseAnalytics loading start");
			firebaseAnalytics.Init();
			yield return WaitForComponentInit(() => firebaseAnalytics.isInit, "firebaseAnalytics");
			if (debugLog)
				Debug.Log("firebaseAnalytics loading Complete");
			if (debugLog)
				Debug.Log("firebaseCloudMessaging loading start");
			firebaseCloudMessaging.Init();
			yield return WaitForComponentInit(() => firebaseCloudMessaging.isInit, "firebaseCloudMessaging");
			if (debugLog)
				Debug.Log("firebaseCloudMessaging loading Complete");
			if (debugLog)
				Debug.Log("--------------------------Firebase Loading Done--------------------------");
			OnLoadingDone();
			IsAllComponentInitlized = true;
		}

		private IEnumerator WaitForComponentInit(Func<bool> isComponentInit, string componentName)
		{
			float waitTime = 0f;
			while (!isComponentInit())
			{
				waitTime += Time.unscaledDeltaTime;
				if (waitTime >= ComponentInitTimeout)
				{
					Debug.LogError("Firebase " + componentName + " init timed out.");
					yield break;
				}

				yield return null;
			}
		}

		#endregion
	}
}
#endif
