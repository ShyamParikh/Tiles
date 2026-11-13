using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.rate_us
{
	public abstract class BaseRateUs
	{
		#region protected variables

		protected string emailAddress;
		protected string storeUrl;

		#endregion

		#region constructor

		public BaseRateUs(string emailAddress, string storeUrl)
		{
			this.emailAddress = emailAddress;
			this.storeUrl = storeUrl;
		}

		#endregion

		#region abstract methods

		public abstract void OpenEmail();
		public abstract void ShowRateUs();

		#endregion

		#region virtual methods

		public virtual void OpenInAppReview()
		{

		}

		#endregion
	}
}
