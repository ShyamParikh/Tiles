#if UNITY_IOS
using com.otm.manager;
using UnityEngine;
using UnityEngine.Networking;

namespace com.otm.rate_us
{
	public class IOSRateUs : BaseRateUs
	{
		#region constructor
		public IOSRateUs(string emailAddress, string storeUrl) : base(emailAddress, storeUrl)
		{
		}
		#endregion

		#region overrided methods

		public override void OpenEmail()
		{
			string subject = UnityWebRequest.EscapeURL("Query").Replace("+", "%20");
			Application.OpenURL("mailto:" + emailAddress + "?subject=" + subject);
		}

		public override void ShowRateUs()
		{
			UnityEngine.iOS.Device.RequestStoreReview();
		}
		#endregion
	}
}
#endif
