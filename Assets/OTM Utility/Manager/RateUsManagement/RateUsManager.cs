#if OTM_RATE_US
using com.otm.manager;
using com.otm.utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.rate_us
{
	public class RateUsManager : SerializedManager<RateUsManager>
	{
		#region private variables

		[SerializeField] private string email;
		[SerializeField] private string storeLinkAndroid = "https://play.google.com/store/apps/details?id=";
		[SerializeField] private string storeLinkIOS;
		[SerializeField] private string privacyPolicyLink;
		[SerializeField] private string termsAndConditionLink;
		[SerializeField] private BaseRateShowCondition[] rateShowConditions = new BaseRateShowCondition[0];

		private BaseRateUs rateUs;
		private string storeLink;

		private Dictionary<Type, BaseRateShowCondition> conditionMapping = new Dictionary<Type, BaseRateShowCondition>();

		#endregion

		#region properties

		public bool IsReviewGiven
		{
			get => PlayerPrefsHelper.GetBool("ReviewGiven");
			set => PlayerPrefsHelper.SetBool("ReviewGiven", value);
		}

		public string Email => email;

		#endregion

		#region unity callback

		private void Start()
		{
			InitConditions();
#if UNITY_IOS
			rateUs = new IOSRateUs(email, storeLinkIOS);
#endif
#if UNITY_ANDROID && OTM_RATE_US
			rateUs = new AndroidRateUs(email, storeLinkAndroid);
#endif

#if UNITY_ANDROID
			storeLink = storeLinkAndroid;
#elif UNITY_IOS
            storeLink = storeLinkIOS;
#else
            storeLink = "";
#endif
		}

		#endregion

		#region public methods

		public bool CanShowRateUs()
		{
			if (IsReviewGiven)
				return false;

			for (int i = 0; i < rateShowConditions.Length; i++)
			{
				if (rateShowConditions[i].IsValid())
					return true;
			}
			return false;
		}

		public void OpenEmail()
		{
			if (rateUs == null)
				return;
			rateUs.OpenEmail();
		}

		public void ShowRateUs()
		{
			if (rateUs == null)
				return;
			if (IsReviewGiven)
				rateUs.ShowRateUs();
			else
				rateUs.OpenInAppReview();
			IsReviewGiven = true;
		}

		public void ShowPrivacyPolicy()
		{
			Application.OpenURL(privacyPolicyLink);
		}

		public void ShowTermsAndCondition()
		{
			Application.OpenURL(termsAndConditionLink);
		}

		public U GetRateUsCondition<U>() where U : BaseRateShowCondition
		{
			if (conditionMapping.ContainsKey(typeof(U)))
				return conditionMapping[typeof(U)] as U;
			return null;
		}

		#endregion

		#region private methods

		private void InitConditions()
		{
			for (int i = 0; i < rateShowConditions.Length; i++)
			{
				rateShowConditions[i].Init();
				conditionMapping[rateShowConditions[i].GetType()] = rateShowConditions[i];
			}
		}

		#endregion
	}
}
#endif