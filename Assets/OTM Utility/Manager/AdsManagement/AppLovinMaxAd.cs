#if OTM_APPLOVIN
using com.otm.analytics;
using com.otm.manager;
using com.otm.utility;
using System;
using UnityEngine;

namespace com.otm.games
{
	public class AppLovinMaxAd : BaseAd
	{
		#region private veriabels
		[SerializeField] private float reloadTimeForInterstitialAds = 30;
		[SerializeField] private float reloadTimeForRewardedAds = 30;
		[SerializeField] private float reloadTimeForBannerAds = 30;

#if UNITY_ANDROID
		private const string rewardAdUnitId = "ceedb6e112f93b37";
		private const string interstitialAdUnitId = "1ee608153aa09a3a";
		private const string bannerAdUnitId = "3fee7bc41f9f9a16";
#endif

#if UNITY_IOS
        private const string rewardAdUnitId = "";
        private const string interstitialAdUnitId = "";
        private const string bannerAdUnitId = ""; 
#endif


		private Coroutine rewardAdCoroutine;
		private Coroutine bannerAdCoroutine;
		private Coroutine interstitialAdCoroutine;

		#endregion

		#region overrided methods

		public override void Initialize(Action<bool> onInitDone, bool ATTStatus)
		{
			Debug.Log("Ads Initialize");
			base.Initialize(onInitDone, ATTStatus);
			MaxSdkCallbacks.OnSdkInitializedEvent += OnInitDone;
			MaxSdk.SetHasUserConsent(true);
			MaxSdk.SetExtraParameter("pisw", "true");
			MaxSdk.SetExtraParameter("disable_all_logs", "true");
			MaxSdk.InitializeSdk();
			RegisterEvents();
		}

		public override bool IsInitialized()
		{
			return MaxSdk.IsInitialized();
		}

		public override void LoadRewardedAds()
		{
			if (!MaxSdk.IsInitialized() || string.IsNullOrEmpty(rewardAdUnitId))
				return;
			base.LoadRewardedAds();
			MaxSdk.LoadRewardedAd(rewardAdUnitId);
#if OTM_Analytics
			if (AnalyticsManager.Instance == null)
				CoroutineRunner.Instance.ValidationTask(IsAnalyticsManagerLoading, () => AnalyticsManager.Instance.RewardedAttemptEvent());
			else
				AnalyticsManager.Instance.RewardedAttemptEvent();
#endif
		}

		public override bool IsRewardedAdsAvailable()
		{
			return MaxSdk.IsInitialized() && MaxSdk.IsRewardedAdReady(rewardAdUnitId);
		}

		public override void ShowRewardedAds(Action<bool> rewardedAdSuccess)
		{
			if (!IsRewardedAdsAvailable())
			{
				LoadRewardedAds();
				rewardedAdSuccess?.Invoke(false);
				return;
			}
			base.ShowRewardedAds(rewardedAdSuccess);
			if (MaxSdk.IsRewardedAdReady(rewardAdUnitId))
			{
				MaxSdk.ShowRewardedAd(rewardAdUnitId);
			}
		}

		public override void LoadInterstitialAds()
		{
			if (!MaxSdk.IsInitialized())
				return;
			if (AdsManager.Instance.IsAdPurchased || string.IsNullOrEmpty(interstitialAdUnitId))
				return;
			base.LoadInterstitialAds();
			MaxSdk.LoadInterstitial(interstitialAdUnitId);
#if OTM_Analytics
			if (AnalyticsManager.Instance == null)
				CoroutineRunner.Instance.ValidationTask(IsAnalyticsManagerLoading, () => AnalyticsManager.Instance.InterstitialAttemptEvent());
			else
				AnalyticsManager.Instance.InterstitialAttemptEvent();
#endif
		}

		public override bool IsInterstitialAdsAvailable()
		{
			if (AdsManager.Instance.IsAdPurchased)
				return false;
			return MaxSdk.IsInitialized() && MaxSdk.IsInterstitialReady(interstitialAdUnitId);
		}

		public override bool ShowInterstitialAds(Action<bool> onInterstitialAdHide = null)
		{
			if (AdsManager.Instance.IsAdPurchased)
			{
				OnInterstitalAdHide(false);
				return false;
			}

			if (!IsInterstitialAdsAvailable())
			{
				LoadInterstitialAds();
				OnInterstitalAdHide(false);
				return false;
			}
			base.ShowInterstitialAds(onInterstitialAdHide);
			MaxSdk.ShowInterstitial(interstitialAdUnitId);
			return true;
		}

		public override void LoadBannerAds()
		{
			if (AdsManager.Instance.IsAdPurchased || string.IsNullOrEmpty(bannerAdUnitId))
				return;
			base.LoadBannerAds();
			MaxSdk.CreateBanner(bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
			MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, Color.white);
		}

		public override bool IsBannerAdsAvailable()
		{
			return bannerAdCoroutine == null && !AdsManager.Instance.IsAdPurchased;
		}

		public override void ShowBannerAds()
		{
			if (AdsManager.Instance.IsAdPurchased)
				return;
			base.ShowBannerAds();
			MaxSdk.ShowBanner(bannerAdUnitId);
		}

		public override void HideBannerAds()
		{
			base.HideBannerAds();
			MaxSdk.HideBanner(bannerAdUnitId);
			LoadBannerAds();
		}

		public override Vector2 GetBannerSize()
		{
			return new Vector2(0, MaxSdkUtils.GetAdaptiveBannerHeight());
		}

		#endregion

		#region priavte Mathods

		private bool IsAnalyticsManagerLoading()
		{
#if OTM_Analytics
			return AnalyticsManager.Instance == null;
#endif
			return false;
		}

		private void OnInitDone(MaxSdkBase.SdkConfiguration configuration)
		{
			onInitDone?.Invoke(true);
		}

		private void RegisterEvents()
		{
			MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
			MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
			MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
			MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
			MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdPaidEvent;
			MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;

			MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
			MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
			MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
			MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
			MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialAdPaidEvent;


			MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
			MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
			MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
			MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
			MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
			MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
		}



		private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			interstitialAdCoroutine = null;
		}

		private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
		{
			if (interstitialAdCoroutine == null)
				interstitialAdCoroutine = CoroutineRunner.Instance.WaitForRealTime(30, LoadInterstitialAds);
		}

		private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
		{
			LoadInterstitialAds();
			OnInterstitalAdHide(false);
		}

		private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			LoadInterstitialAds();
			OnInterstitalAdHide(true);
		}

		private void OnInterstitialAdPaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			Debug.Log("OnInterstitialAdPaidEvent");
#if OTM_Analytics
			AnalyticsManager.Instance.AdImpressionEvent(adUnitId, adInfo);
#endif
		}


		private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			rewardAdCoroutine = null;
			AdsManager.onRewardedAdStateChange.Invoke();
		}

		private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
		{
			Debug.Log(errorInfo.AdLoadFailureInfo);
			if (rewardAdCoroutine == null)
				rewardAdCoroutine = CoroutineRunner.Instance.WaitForRealTime(30, LoadRewardedAds);
			AdsManager.onRewardedAdStateChange.Invoke();
		}

		private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
		{
			OnRewardAdsShowFail();
			if (rewardAdCoroutine == null)
				rewardAdCoroutine = CoroutineRunner.Instance.WaitForRealTime(30, LoadRewardedAds);
			AdsManager.onRewardedAdStateChange.Invoke();
		}

		private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			OnRewardAdsShowSuccess();
			AdsManager.onRewardedAdStateChange.Invoke();
			LoadRewardedAds();
		}

		private void OnRewardedAdDisplayedEvent(string arg1, MaxSdkBase.AdInfo arg2)
		{

		}

		private void OnRewardedAdPaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			Debug.Log("OnRewardedAdPaidEvent");
#if OTM_Analytics
			AnalyticsManager.Instance.AdImpressionEvent(adUnitId, adInfo);
#endif
		}

		private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			bannerAdCoroutine = null;
		}

		private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
		{
			OnBannerAdsLoadFail();
			if (bannerAdCoroutine == null)
				bannerAdCoroutine = CoroutineRunner.Instance.WaitForRealTime(30, LoadBannerAds);
		}

		private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			Debug.Log("OnBannerAdClickedEvent");
		}

		private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			Debug.Log("OnBannerAdRevenuePaidEvent");
#if OTM_Analytics
			AnalyticsManager.Instance.AdImpressionEvent(adUnitId, adInfo);
#endif
		}

		private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			Debug.Log("OnBannerAdExpandedEvent");
		}

		private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			Debug.Log("OnBannerAdCollapsedEvent");
		}

		#endregion
	}
}
#endif