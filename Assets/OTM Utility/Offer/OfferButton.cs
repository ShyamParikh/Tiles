#if OTM_Offer
using UnityEngine;

namespace com.otm.offer
{
	public class OfferButton : MonoBehaviour
	{
#region protected variables
		protected BaseOfferView offerView;
#endregion

#region virtual methods

		public virtual void Init(BaseOfferView offerView)
		{
			this.offerView = offerView;
		}

		public virtual void ShowButton()
		{
			if (offerView.IsValid() && offerView.CanShowButton())
				gameObject.SetActive(true);
			else
				gameObject.SetActive(false);
		}

		public virtual void OnButtonClick()
		{
			if (offerView != null)
			{
				OfferManager.Instance.ShowOffer(offerView);
			}
		}

#endregion
	}
}
#endif