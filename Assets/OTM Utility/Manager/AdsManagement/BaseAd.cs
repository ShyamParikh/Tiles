#if OTM_ADS
using com.otm.games;
using com.otm.manager;
using System;
using UnityEngine;

namespace com.otm.utility
{
	public class BaseAd
	{
        #region private veriable

		private Action<bool> rewardedAdAction;

        #endregion

        #region propertice
        #endregion

        #region protected variables

		protected Action<bool> onInitDone;
		protected Action<bool> onInterstitialAdHide;

        #endregion

        #region virtual methods

		public virtual void Initialize(Action<bool> onInitDone, bool ATTStatus)
		{
			this.onInitDone = onInitDone;
		}

		public virtual bool IsInitialized()
		{
			return true;
		}

		public virtual void LoadInterstitialAds()
		{
		}

		public virtual bool ShowInterstitialAds(Action<bool> onInterstitialAdHide)
		{
			this.onInterstitialAdHide = onInterstitialAdHide;
			return true;
		}

		public virtual void OnInterstitialAdsLoadSuccess()
		{
		}

		public virtual void OnInterstitialAdsLoadFail()
		{
		}

		public virtual bool IsInterstitialAdsAvailable()
		{
			return false;
		}

		public virtual void ShowRewardedAds(Action<bool> rewardedAdSuccess)
		{
			rewardedAdAction = rewardedAdSuccess;
		}

		public virtual void LoadRewardedAds()
		{
		}

		public virtual void OnRewardedAdsLoadSuccess()
		{
		}

		public virtual void OnRewardedAdsLoadFail()
		{
		}

		public virtual bool IsRewardedAdsAvailable()
		{
			return false;
		}

		public virtual void OnRewardAdsShowSuccess()
		{
			UnScaleWaitForSecondsTask waitForSecondsTask = new UnScaleWaitForSecondsTask();
			waitForSecondsTask.OnTaskComplete.RegisterEvent(() => { rewardedAdAction?.Invoke(true); });
			waitForSecondsTask.Start(0.2f, 0.1f);
		}

		public virtual void OnRewardAdsShowFail()
		{
			rewardedAdAction?.Invoke(false);
		}

		public virtual void LoadBannerAds()
		{
		}

		public virtual void OnBannerAdsLoadSuccess()
		{

		}

		public virtual void OnBannerAdsLoadFail()
		{
		}

		public virtual bool IsBannerAdsAvailable()
		{
			return false;
		}

		public virtual void ShowBannerAds()
		{
		}

		public virtual void HideBannerAds()
		{

		}

		public virtual Vector2 GetBannerSize()
		{
			return Vector2.zero;
		}

		public virtual void OnInterstitalAdHide(bool show)
		{
			//if (show)
			//	AnalyticsManager.Instance.InterstitialShowEvent();
			if (!AdsManager.Instance.AdsShownForFirstTime && show)
				AdsManager.Instance.AdsShownForFirstTime = true;
			onInterstitialAdHide?.Invoke(show);
			onInterstitialAdHide = null;
		}

        #endregion

        #region App Open Ads

		public virtual void LoadAppOpenAd()
		{
		}

		public virtual bool IsAppOpenAdAvailable()
		{
			return false;
		}

		public virtual void ShowAppOpenAd(Action<bool> onAppOpenAdClosed)
		{
		}

        #endregion

        #region public methods
        #endregion
	}

	public enum AdType
	{
		RewardedAd,
		InterstitialAd,
		BannerAd
	}
}
#endif