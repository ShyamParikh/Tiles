#if OTM_Offer
using com.otm.jigsaw;
using com.otm.manager;
using com.otm.ui;
using com.otm.utility;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace com.otm.offer
{
	public abstract class BaseOfferView : BaseView
	{
#region protected methods
		[SerializeField] protected BaseOfferRewardCardSetup[] rewardCards = new BaseOfferRewardCardSetup[0];
		[SerializeField] protected bool selfCardInit;
		[SerializeField] protected OfferButton offerButton;
		protected string purchaseProductId;
		protected OfferBundle? purchaseBundle;
		protected OfferConfig offerConfig;
		protected Action onHide;

#endregion

#region propertices
		public int Priority => offerConfig.priority;
		public OfferConfig OfferConfig => offerConfig;
		public OfferButton OfferButton => offerButton;

		public string JsonData { get; private set; }

#endregion

#region overrided methods

		public override void ShowView()
		{
			if (selfCardInit)
				SetCardDetailes();
			base.ShowView();
			if (offerConfig != null)
				OfferManager.OnOfferShow.Invoke(offerConfig.offerId);
		}

		public override void HideView()
		{
			base.HideView();
			if (offerConfig != null)
				OfferManager.OnOfferHide.Invoke(offerConfig.offerId);
			onHide?.Invoke();
			onHide = null;
		}

#endregion

#region virtual methods

		public virtual void ShowView(Action onHide)
		{
			this.onHide = onHide;
			ShowView();
		}

		public virtual void Init(string data)
		{
			JsonData = data;
			offerConfig = JsonConvert.DeserializeObject<OfferConfig>(data);
			Init();
			if (selfCardInit)
				InitRewardCards();
		}

		public virtual bool IsValid()
		{
			return offerConfig != null && offerConfig.isActive;
		}

		public virtual bool CanShowOffer()
		{
			return true;
		}

		public virtual void PurchaseOffer(string productId, int bundleIndex = 0)
		{
			purchaseBundle = GetOfferBundle(bundleIndex);
			if (purchaseBundle == null)
			{
				OnPurchaseFailed(productId);
				return;
			}
			purchaseProductId = productId;
			IAPManager.Instance.DoPurchase(productId, OnPurchaseDone);
		}

		public virtual void OnPurchaseSuccess(string productId, OfferBundle offerBundle)
		{
		}

		public virtual void OnPurchaseFailed(string productId)
		{
		}

		public virtual bool CanShowButton()
		{
			return true;
		}

		public virtual void UpdateOffer()
		{

		}

#endregion

#region protected methods

		protected void OnPurchaseDone(string productId, PurchaseStatus purchaseStatus)
		{
			if (purchaseStatus == PurchaseStatus.Fail || !purchaseProductId.Equals(productId))
			{
				OnPurchaseFailed(productId);
				return;
			}
			OnPurchaseSuccess(productId, purchaseBundle.Value);
		}

		protected void InitRewardCards()
		{
			for (int i = 0; i < rewardCards.Length; i++)
				rewardCards[i].Init(this);
		}

		protected void SetCardDetailes()
		{
			for (int i = 0; i < rewardCards.Length; i++)
				rewardCards[i].SetDetails();
		}

#endregion

#region public methods
		public OfferBundle? GetOfferBundle(int bundleIndex = 0)
		{
			if (offerConfig == null || offerConfig.bundles == null || offerConfig.bundles.Length <= bundleIndex)
				return null;
			return offerConfig.bundles[bundleIndex];
		}

#endregion
	}

	public class OfferConfig
	{
		public string bundleName;
		public string prefabName;
		public string offerId;
		public OfferTag offerTag;
		public bool isActive;
		public int priority;
		public int placementId;
		public OfferBundle[] bundles = new OfferBundle[0];
	}

	public enum OfferTag
	{
		Meta,
		LevelComplete,
		Interstital,
	}

	public struct OfferBundle
	{
		public int discountValue;
		[StringConstantDropDown(typeof(PurchaseProductIds))]
		public string productId;
		public Rewards rewards;
	}
}
#endif