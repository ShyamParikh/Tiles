#if OTM_Analytics
using ByteBrewSDK;
using com.otm.firebase;
using com.otm.games;
using com.otm.jigsaw;
using com.otm.manager;
using System.Collections.Generic;
using UnityEngine;
using FirebaseManager = com.otm.firebase.FirebaseManager;

namespace com.otm.analytics
{
	public class AnalyticsManager : Manager<AnalyticsManager>
	{
		#region private variables

        [SerializeField] private bool logEvent;

        #endregion

        #region propetices
        public bool LogEvent => logEvent;

        #endregion

		#region Overrided Methods

        public override void Awake()
        {
            base.Awake();
            ByteBrew.InitializeByteBrew();
        }

        #endregion

#region public methods

		public void LevelStartEvent(int levelIndex)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add(FirebaseParameterConstant.LEVEL, "" + levelIndex);
			FirebaseManager.FirebaseAnalytics.LogEvent(FirebaseEventConstant.LEVEL_START, data);
			GameAnalyticsManager.Instance.LogDesignEvent(GameAnalyticsParameterConstant.LEVEL_START + ":" + levelIndex);
		}
		public void ChallengeLevelStartEvent(int levelIndex)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add(FirebaseParameterConstant.LEVEL, "" + levelIndex);
			FirebaseManager.FirebaseAnalytics.LogEvent(FirebaseEventConstant.LEVEL_START, data);
			GameAnalyticsManager.Instance.LogDesignEvent(GameAnalyticsParameterConstant.LEVEL_START + ":" + levelIndex);
		}

		public void LevelFailEvent(int levelIndex)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add(FirebaseParameterConstant.LEVEL, "" + levelIndex);
			FirebaseManager.FirebaseAnalytics.LogEvent(FirebaseEventConstant.LEVEL_FAIL, data);
			GameAnalyticsManager.Instance.LogDesignEvent(GameAnalyticsParameterConstant.LEVEL_FAIL + ":" + levelIndex);
		}

		public void LevelWinEvent(int levelIndex)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add(FirebaseParameterConstant.LEVEL, "" + levelIndex);

			FirebaseManager.FirebaseAnalytics.LogEvent(FirebaseEventConstant.LEVEL_WIN, data);
			GameAnalyticsManager.Instance.LogDesignEvent(GameAnalyticsParameterConstant.LEVEL_WIN + ":" + levelIndex);
		}
		public void ChallengeLevelWinEvent(int levelIndex)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add(FirebaseParameterConstant.LEVEL, "" + levelIndex);

			FirebaseManager.FirebaseAnalytics.LogEvent(FirebaseEventConstant.LEVEL_WIN, data);
			GameAnalyticsManager.Instance.LogDesignEvent(GameAnalyticsParameterConstant.LEVEL_WIN + ":" + levelIndex);
		}

		public void BoosterEvent(BoosterType boosterType)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add(FirebaseParameterConstant.BOOSTER_TYPE, "" + boosterType);
			FirebaseManager.FirebaseAnalytics.LogEvent(FirebaseEventConstant.BOOSTER_USE, data);
			GameAnalyticsManager.Instance.LogDesignEvent(GameAnalyticsParameterConstant.BOOSTER_USE + ":" + boosterType);
		}

		public void BoosterEvent(int levelIndex, BoosterType boosterType)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add(FirebaseParameterConstant.LEVEL, "" + levelIndex);
			data.Add(FirebaseParameterConstant.BOOSTER_TYPE, "" + boosterType);
			FirebaseManager.FirebaseAnalytics.LogEvent(FirebaseEventConstant.BOOSTER_USE, data);
			GameAnalyticsManager.Instance.LogDesignEvent(GameAnalyticsParameterConstant.BOOSTER_USE + ":" + levelIndex + ":" + boosterType);
		}
		public void BoosterChallengeEvent(int levelIndex, BoosterType boosterType)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add(FirebaseParameterConstant.LEVEL, "" + levelIndex);
			data.Add(FirebaseParameterConstant.BOOSTER_TYPE, "" + boosterType);
			FirebaseManager.FirebaseAnalytics.LogEvent(FirebaseEventConstant.BOOSTER_USE, data);
			GameAnalyticsManager.Instance.LogDesignEvent(GameAnalyticsParameterConstant.BOOSTER_USE + ":" + levelIndex + ":" + boosterType);
		}

		public void InterstitialAttemptEvent()
		{
			FirebaseManager.FirebaseAnalytics.LogEvent(FirebaseEventConstant.AD_INTERSTITIAL_ATTEMPT);
			GameAnalyticsManager.Instance.LogDesignEvent(GameAnalyticsParameterConstant.AD_INTERSTITIAL_ATTEMPT);
		}

		public void InterstitialShowEvent()
		{
			FirebaseManager.FirebaseAnalytics.LogEvent(FirebaseEventConstant.AD_INTERSTITIAL_SHOW);
			GameAnalyticsManager.Instance.LogDesignEvent(GameAnalyticsParameterConstant.AD_INTERSTITIAL_SHOW);
		}

		public void RewardedAttemptEvent()
		{
			FirebaseManager.FirebaseAnalytics.LogEvent(FirebaseEventConstant.REWARD_ATTEMPT);
			GameAnalyticsManager.Instance.LogDesignEvent(GameAnalyticsParameterConstant.REWARD_ATTEMPT);
		}

		public void RewardedShowEvent(string placement)
		{
			int level = DataManager.LevelNumber.Value;
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add(FirebaseParameterConstant.LEVEL, "" + level);
			data.Add(FirebaseParameterConstant.REWARD_PLACEMENT, placement);

			FirebaseManager.FirebaseAnalytics.LogEvent(FirebaseEventConstant.AD_REWARDED_SHOWN, data);
			GameAnalyticsManager.Instance.LogDesignEvent(GameAnalyticsParameterConstant.AD_REWARDED_SHOWN + ":" + level);
			GameAnalyticsManager.Instance.LogDesignEvent(GameAnalyticsParameterConstant.AD_REWARDED_PLACMENT + ":" + placement);
		}

		public void AdImpressionEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
		{
			double revenue = impressionData.Revenue;
			var impressionParameters = new[] {
				new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
				new Firebase.Analytics.Parameter("ad_source", impressionData.NetworkName),
				new Firebase.Analytics.Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
				new Firebase.Analytics.Parameter("ad_format", impressionData.AdFormat),
				new Firebase.Analytics.Parameter("value", revenue),
				new Firebase.Analytics.Parameter("currency", "USD"),
			};
			Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
		}

		public void PurchaseEvent(string productId, string placement)
		{
			int level = DataManager.LevelNumber.Value;
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add(FirebaseParameterConstant.LEVEL, "" + level);
			data.Add(FirebaseParameterConstant.PLACEMENT, placement);
			data.Add(FirebaseParameterConstant.PRODUCT_ID, productId);

			FirebaseManager.FirebaseAnalytics.LogEvent(FirebaseEventConstant.PURCHASE_SUCCESS, data);
			GameAnalyticsManager.Instance.LogDesignEvent(GameAnalyticsParameterConstant.PURCHASE_PRODUCT_ID + ":" + placement);
			GameAnalyticsManager.Instance.LogDesignEvent(GameAnalyticsParameterConstant.PURCHASE_LEVEL_NUMBER + ":" + level);
		}

#endregion
	}

	public class FirebaseEventConstant
	{
		public const string LEVEL_START = "level_start";
		public const string LEVEL_FAIL = "level_fail";
		public const string LEVEL_WIN = "level_win";
		public const string BOOSTER_USE = "booster_use";
		public const string AD_INTERSTITIAL_ATTEMPT = "ad_interstitial_attempt";
		public const string AD_INTERSTITIAL_SHOW = "ad_interstitial_show";
		public const string REWARD_ATTEMPT = "reward_attempt";
		public const string AD_REWARDED_SHOWN = "ad_rewarded_shown";
		public const string AD_IMPRESSION = "ad_impression";
		public const string PURCHASE_SUCCESS = "purchase_success";
	}
	public class FirebaseParameterConstant
	{
		public const string LEVEL = "level";
		public const string ATTEMPTS = "attempts";
		public const string BOOSTER_TYPE = "booster_type";
		public const string REWARD_PLACEMENT = "reward_placement";
		public const string PLACEMENT = "placement";
		public const string PRODUCT_ID = "product_ID";
	}

	public class GameAnalyticsParameterConstant
	{
		public const string LEVEL_START = "level_start";
		public const string LEVEL_FAIL = "level_fail";
		public const string LEVEL_WIN = "level_win";
		public const string LEVEL_ATTEMPTS = "level_attempts";
		public const string BOOSTER_USE = "booster_use";
		public const string AD_INTERSTITIAL_ATTEMPT = "ad_interstitial_attempt";
		public const string AD_INTERSTITIAL_SHOW = "ad_interstitial_show";
		public const string REWARD_ATTEMPT = "reward_attempt";
		public const string AD_REWARDED_SHOWN = "ad_rewarded_shown";
		public const string AD_REWARDED_PLACMENT = "ad_rewarded_placment";
		public const string AD_IMPRESSION = "ad_impression";
		public const string PURCHASE_PRODUCT_ID = "purchase_product_Id";
		public const string PURCHASE_LEVEL_NUMBER = "purchase_level_number";
	}
}
#endif