#if UNITY_ANDROID && OTM_RATE_US
using com.otm.utility;
#if ANDROID_IN_APP_RATE_US
using Google.Play.Review;
#endif
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace com.otm.rate_us
{
	public class AndroidRateUs : BaseRateUs
	{
		#region private variables
#if ANDROID_IN_APP_RATE_US
		private ReviewManager reviewManager;
		private PlayReviewInfo playReviewInfo;
#endif

		public bool IsInAppReviewOpened
		{
			get => PlayerPrefsHelper.GetBool("IsInAppReviewOpened");
			set => PlayerPrefsHelper.SetBool("IsInAppReviewOpened", value);
		}

		#endregion

		#region constructor
		public AndroidRateUs(string emailAddress, string storeUrl) : base(emailAddress, storeUrl)
		{
#if ANDROID_IN_APP_RATE_US
			reviewManager = new ReviewManager();

			RateUsManager.Instance.StartCoroutine(GetReviewInfo());
#endif
		}
		#endregion

		#region overrided methods

		public override void OpenEmail()
		{
			string subject = UnityWebRequest.EscapeURL("Query " + Application.version).Replace("+", "%20");
			Application.OpenURL("mailto:" + emailAddress + "?subject=" + subject);
		}

		public override void ShowRateUs()
		{
			ShowStore();
		}

		public override void OpenInAppReview()
		{
			base.OpenInAppReview();
#if ANDROID_IN_APP_RATE_US
			if (playReviewInfo != null)
			{
				RateUsManager.Instance.StartCoroutine(ShowInAppReview());
				return;
			}
			RateUsManager.Instance.StartCoroutine(GetReviewInfo());
#endif
			ShowRateUs();
		}

		#endregion

		#region private methods

		private void ShowStore()
		{
			Application.OpenURL(storeUrl + Application.identifier);
		}

		#endregion
#if ANDROID_IN_APP_RATE_US
		#region coroutine

		IEnumerator GetReviewInfo()
		{
			var requestFlowOperation = reviewManager.RequestReviewFlow();
			yield return requestFlowOperation;
			if (requestFlowOperation.Error != ReviewErrorCode.NoError)
			{
				yield break;
			}
			playReviewInfo = requestFlowOperation.GetResult();
		}

		IEnumerator ShowInAppReview()
		{
			var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
			yield return launchFlowOperation;
			playReviewInfo = null;
			if (launchFlowOperation.Error != ReviewErrorCode.NoError)
				IsInAppReviewOpened = true;
			else
				ShowRateUs();
		}

		#endregion
#endif
	}
}
#endif

