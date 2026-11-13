#if OTM_Offer
using com.otm.manager;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.otm.offer
{
	public abstract class BaseOfferRewardCardSetup : SerializedMonoBehaviour
	{
#region protected variables
		[SerializeField] protected int bundleIndex;
		[SerializeField] protected TMP_Text priceText;
		[SerializeField] protected Button purchaseButton;
		protected OfferBundle? bundle;
		protected BaseOfferView offerView;
#endregion

#region virtual methods

		public virtual void Init(BaseOfferView offerView)
		{
			this.offerView = offerView;
			bundle = offerView.GetOfferBundle(bundleIndex);
			if (purchaseButton != null && bundle != null)
				purchaseButton.onClick.AddListener(OnPurchaseButtonClick);
		}


		public virtual void SetDetails()
		{
			if (priceText != null && bundle != null)
			{
				LocalPrice localPrice = IAPManager.Instance.GetLocalPrice(bundle.Value.productId);
				priceText.text = localPrice.currencyCode + " " + localPrice.price;
			}
		}

#endregion

#region private methods

		private void OnPurchaseButtonClick()
		{
			offerView.PurchaseOffer(bundle.Value.productId, bundleIndex);
		}

#endregion
	}
}
#endif