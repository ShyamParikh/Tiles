#if OTM_ADS
using com.otm.manager;
using com.otm.utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.otm.ui
{
	public class RewardedAdsButton : SerializedMonoBehaviour
	{
        #region private veriables

		[SerializeField] private Button button;
		[SerializeField] private string location;
		[SerializeField] private bool isManualEvent;
		[SerializeField] private RewardAdValidation adValidation = new NoneAdValidation();
		[SerializeField] private UnityEvent onRewardAdWatchSuccess = new UnityEvent();
		[SerializeField] private UnityEvent onRewardAdWatchFailed = new UnityEvent();
		[SerializeField] private UnityEvent onAdNotAvailable = new UnityEvent();
		[SerializeField] private UnityEvent onAdAvailable = new UnityEvent();

        #endregion

        #region unity callback

		private void OnEnable()
		{
			adValidation.Init();
#if OTM_ADS
			AdsManager.onRewardedAdStateChange.RegisterEvent(SetVideoAdButton);
#endif
			SetVideoAdButton();
		}

		private void Start()
		{
			if (button != null)
				button.onClick.AddListener(OnAdButtonClick);
		}

		private void OnDisable()
		{
			AdsManager.onRewardedAdStateChange.DeRegisterEvent(SetVideoAdButton);
		}

		private void OnValidate()
		{
			if (button == null)
				button = GetComponent<Button>();
		}

#endregion

        #region private methods

		private void SetVideoAdButton()
		{
			if (adValidation.IsValid() && AdsManager.Instance.IsRewardVideoAvailable())
				onAdAvailable?.Invoke();
			else
				onAdNotAvailable?.Invoke();
		}

		private void AddWatchDone(bool isSuccess)
		{
			if (isSuccess)
				onRewardAdWatchSuccess?.Invoke();
			else
				onRewardAdWatchFailed?.Invoke();
			SetVideoAdButton();

		}

		private void OnAdButtonClick()
		{
			if (!adValidation.IsValid() || !AdsManager.Instance.IsRewardVideoAvailable())
			{
				Debug.Log("Ads Not Available");
				return;
			}
			adValidation.OnWatchAd();
			AdsManager.Instance.ShowRewardAd(AddWatchDone, location, !isManualEvent);
		}

        #endregion
	}
}
#endif