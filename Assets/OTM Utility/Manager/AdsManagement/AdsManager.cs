#if OTM_ADS
#if OTM_Firebase
using com.otm.analytics;
using com.otm.firebase;
#endif
using com.otm.utility;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

namespace com.otm.manager
{
	public class AdsManager : SerializedManager<AdsManager>
	{
		[SerializeField] private BaseAd ads;
		[SerializeField] private bool canInitialize = false;

		private string location;
		[ShowInInspector] private bool IsAllAdEnable;
		[ShowInInspector] private int interstitialAdStartLevel = 9;
		[ShowInInspector] private int interstitialAdIntervalInSeconds = 60;
		[ShowInInspector] private int interstitialDailyCapping = 9;
		private Coroutine waitForInterstitialTimeCoroutine;
		private DateTime resetInterstitialDate;

		// Level-based frequency
		[ShowInInspector] private int interstitialLevelGap = 1;
		[ShowInInspector] private int levelsSinceLastInterstitial = 0;

		#region propertice

		public string Location => location;

		[ShowInInspector]
		public bool IsAdPurchased
		{
			get => PlayerPrefsHelper.GetBool("Remove Ad", false);
			set => PlayerPrefsHelper.SetBool("Remove Ad", value);
		}

		[ShowInInspector]
		public int RemainInterstitialAds
		{
			get => PlayerPrefsHelper.GetInt("Remain_Interstitial_Ads", 0);
			set => PlayerPrefsHelper.SetInt("Remain_Interstitial_Ads", value);
		}
		public bool AdsShownForFirstTime
		{
			get => PlayerPrefsHelper.GetBool("AdsShownForFirstTime", false);
			set => PlayerPrefsHelper.SetBool("AdsShownForFirstTime", value);
		}

		public bool IsAdEnable { get; set; }

		public bool IsInterstitialAdShown { get; set; }

		#endregion

		#region static variables

		public static ActionSystem onRewardedAdStateChange = new ActionSystem();
		public static ActionSystem onInterstitialAdStateChange = new ActionSystem();

		#endregion

		#region UNITY_CALLBACKS
		public override void Awake()
		{
			IsAdEnable = true;
			base.Awake();
			CoroutineRunner.Instance.ValidationTask(IsFirebaseLoaded, LoadRemoteData);
			if (canInitialize)
				Initialize();
			else
				OnLoadingDone();
		}
		#endregion


		#region PUBLIC_METHODS

		public void ShowRewardAd(Action<bool> successAction, string location, bool canAdEvent = true)
		{
			if (!IsAllAdEnable)
			{
				successAction?.Invoke(false);
				return;
			}
			this.location = location;

			if (ads != null && ads.IsRewardedAdsAvailable())
			{
				ads.ShowRewardedAds(b =>
				{
					successAction.Invoke(b);
					if (b)
					{
#if OTM_Analytics
						AnalyticsManager.Instance.RewardedShowEvent(this.location);
#endif
					}
				});
				return;
			}
			successAction?.Invoke(false);
		}

		public void ShowInterstitialAd(Action<bool> onInterstitialAdHide = null)
		{
			if (IsAdPurchased || !IsAdEnable || !IsAllAdEnable)
			{
				onInterstitialAdHide?.Invoke(false);
				return;
			}
			if (ads != null && ads.IsInterstitialAdsAvailable() && ads.ShowInterstitialAds(onInterstitialAdHide))
			{
				RemainInterstitialAds = RemainInterstitialAds - 1;
				ResetLevelCounter();
				if (interstitialAdIntervalInSeconds > 0)
					waitForInterstitialTimeCoroutine = StartCoroutine(WaitForInterstitialTime(interstitialAdIntervalInSeconds));
				else
					waitForInterstitialTimeCoroutine = null;
				IsInterstitialAdShown = true;
				return;
			}
			onInterstitialAdHide?.Invoke(false);
		}

		public bool IsRewardVideoAvailable()
		{
			if (!IsAllAdEnable)
				return false;
			return ads != null && ads.IsRewardedAdsAvailable();
		}

		public bool IsInterstialAdAvailable()
		{
			if (ads == null || IsAdPurchased || !IsAllAdEnable)
				return false;
			if (waitForInterstitialTimeCoroutine != null)
				return false;
			if (interstitialAdStartLevel > DataManager.LevelNumber.Value)
				return false;
			if (IsInterstitialCappingExit())
				return false;
			// Level gap check
			if (interstitialLevelGap > 0 && levelsSinceLastInterstitial < interstitialLevelGap)
				return false;
			return ads.IsInterstitialAdsAvailable();
		}

		public void ShowBanner()
		{
			if (ads == null || IsAdPurchased || !IsAdEnable || !IsAllAdEnable)
				return;
			ads.ShowBannerAds();
		}

		public void HideBanner()
		{
			if (ads != null)
				ads.HideBannerAds();
		}

		public Vector2 GetBannerSize()
		{
			if (ads == null)
				return Vector2.zero;
			return ads.GetBannerSize();
		}

		#endregion

		#region Level Tracking

		public void OnLevelStart()
		{
			levelsSinceLastInterstitial++;
		}

		public void ResetLevelCounter()
		{
			levelsSinceLastInterstitial = 0;
		}

		#endregion

		#region PRIVATE_FUNCTIONS

		private void SetIniterstitialAdsData()
		{
			string lastDateJson = PlayerPrefsHelper.GetString("InterstitialAdsDate", "");
			if (string.IsNullOrEmpty(lastDateJson))
			{
				RemainInterstitialAds = interstitialDailyCapping;
				resetInterstitialDate = DateTime.UtcNow.Date;
				DateResetIntertitialDate();
				return;
			}
			resetInterstitialDate = JsonConvert.DeserializeObject<DateTime>(lastDateJson);
			if (resetInterstitialDate < DateTime.UtcNow.Date)
			{
				RemainInterstitialAds = interstitialDailyCapping;
				resetInterstitialDate = DateTime.UtcNow.Date;
				DateResetIntertitialDate();
			}
		}

		private bool IsInterstitialCappingExit()
		{
			if (resetInterstitialDate < DateTime.UtcNow)
			{
				RemainInterstitialAds = interstitialDailyCapping;
				resetInterstitialDate = DateTime.UtcNow.Date;
				DateResetIntertitialDate();
				return false;
			}
			return RemainInterstitialAds <= 0;
		}

		private void DateResetIntertitialDate()
		{
			PlayerPrefsHelper.SetString("InterstitialAdsDate", JsonConvert.SerializeObject(resetInterstitialDate));
		}

		private void Initialize()
		{
			if (ads != null)
				ads.Initialize(OnInitializeSuccess, true);
			else
				OnLoadingDone();
		}

		private void OnInitializeSuccess(bool isSuccess)
		{
			ads?.LoadInterstitialAds();
			ads?.LoadRewardedAds();
			ads?.LoadBannerAds();
			OnLoadingDone();
		}

		private void LoadRemoteData()
		{
#if OTM_Firebase
			IsAllAdEnable = FirebaseManager.FirebaseRemoteConfig.GetValue<bool>(FirebaseRemoteConfigConstant.AD_ENABLE);
			interstitialAdStartLevel = FirebaseManager.FirebaseRemoteConfig.GetValue<int>(FirebaseRemoteConfigConstant.INTER_START_LEVEL);
			interstitialAdIntervalInSeconds = FirebaseManager.FirebaseRemoteConfig.GetValue<int>(FirebaseRemoteConfigConstant.INTER_CAPPING);
			interstitialDailyCapping = FirebaseManager.FirebaseRemoteConfig.GetValue<int>(FirebaseRemoteConfigConstant.DAILY_INTER_CAPPING);

			interstitialLevelGap = FirebaseManager.FirebaseRemoteConfig.GetValue<int>(FirebaseRemoteConfigConstant.INTER_LEVEL_GAP);
#endif
			if (interstitialLevelGap <= 0)
				interstitialLevelGap = 3;

			SetIniterstitialAdsData();
		}

		private bool IsFirebaseLoaded()
		{
#if OTM_Firebase
			if (FirebaseManager.Instance == null)
				return true;
			return !FirebaseManager.FirebaseRemoteConfig.isInit;
#else
			return true;
#endif
		}

		#endregion

		#region coroutine

		IEnumerator WaitForInterstitialTime(float time)
		{
			yield return new WaitForSecondsRealtime(time);
			waitForInterstitialTimeCoroutine = null;
		}

		#endregion

	}
}
#endif