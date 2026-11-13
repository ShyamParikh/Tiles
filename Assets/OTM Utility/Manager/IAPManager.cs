#if OTM_IAP
using com.otm.games;
using com.otm.jigsaw;
using com.otm.manager;
using com.otm.utility;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace com.otm.manager
{
	public class IAPManager : Manager<IAPManager>, IDetailedStoreListener
	{
		#region private variables

		[SerializeField, StringConstantDropDown(typeof(PurchaseProductIds))] private string[] productIds = new string[0];

		private IStoreController m_StoreController;
		private Action<string, PurchaseStatus> purchseCallBack;
		private Action<bool> onIAPIninDone;

		#endregion

		#region propertices

		public bool IsIAPInitialized => m_StoreController != null;
		[ShowInInspector] public string Location { get; set; }

		#endregion

		#region unity callback

		private void Start()
		{
			InitializeStore();
		}

		#endregion

		#region private methods

		private void InitializeStore(Action<bool> onInitializingDone = null)
		{
			var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
			onIAPIninDone = onInitializingDone;
			for (int i = 0; i < productIds.Length; i++)
			{
				builder.AddProduct(productIds[i], ProductType.Consumable);
			}
			UnityPurchasing.Initialize(this, builder);
		}

		#endregion

		#region public methods

		public void DoPurchase(string productId, Action<string, PurchaseStatus> onPurchaseComplete)
		{
			if (!productIds.Contains(productId))
			{
				Debug.LogError("ProductId not Found : " + productId);
				onPurchaseComplete?.Invoke(productId, PurchaseStatus.Fail);
				return;
			}
			StartCoroutine(DoInAppPurchase(productId, onPurchaseComplete));
		}

		public LocalPrice GetLocalPrice(string productID)
		{
			LocalPrice localPrice = new LocalPrice() { currencyCode = "None", price = 0 };
			if (m_StoreController != null)
			{
				Product product = m_StoreController.products.WithID(productID);
				if (product != null && product.metadata != null)
				{
					localPrice.currencyCode = product.metadata.isoCurrencyCode;
					localPrice.price = product.metadata.localizedPrice;
				}
			}

			return localPrice;
		}

		public bool CheckProductIdAvaialble(string id)
		{
			if (productIds.Contains(id))
				return true;
			return false;
		}

		#endregion

		#region IDetailedStoreListener methods
		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			m_StoreController = controller;
			onIAPIninDone?.Invoke(true);
		}

		public void OnInitializeFailed(InitializationFailureReason error)
		{
			Debug.LogError("OnInitializeFailed : " + error.ToString());
			onIAPIninDone?.Invoke(false);
		}

		public void OnInitializeFailed(InitializationFailureReason error, string message)
		{
			Debug.LogError("OnInitializeFailed : " + message);
			onIAPIninDone?.Invoke(false);
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
		{
			purchseCallBack?.Invoke(product.definition.id, PurchaseStatus.Fail);
			Debug.LogError("OnPurchaseFailed : " + failureDescription.message);
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
		{
			Debug.LogError("OnPurchaseFailed : " + failureReason);
		}

		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
		{
			var product = purchaseEvent.purchasedProduct;
			if (productIds.Contains(product.definition.id))
				purchseCallBack?.Invoke(product.definition.id, PurchaseStatus.Sucess);
			else
				purchseCallBack?.Invoke("None", PurchaseStatus.Fail);
			Debug.Log($"Purchase Complete - Product: {product.definition.id}");
			return PurchaseProcessingResult.Complete;
		}
		#endregion
		#region coroutine

		IEnumerator DoInAppPurchase(string productId, Action<string, PurchaseStatus> onPurchaseComplete)
		{
			SoftLoadingView softLoadingView = UIManager.Instance.GetView<SoftLoadingView>();
			softLoadingView.ShowView("Checking Internet connection...");
			bool isInternetAvailable = false;
			bool isIAPAvailable = false;
			yield return StartCoroutine(CheckInternetConnection(b => isInternetAvailable = b));
			if (!isInternetAvailable)
			{
				softLoadingView.HideView();
				Debug.LogError("No Internet");
				//UIManager.Instance.ShowView<NoInternetView>();
				yield break;
			}
			softLoadingView.UpdateMessage("Initilizing Purchase...");
			yield return StartCoroutine(WaitForIAPInitlization(b => isIAPAvailable = b));
			softLoadingView.HideView();
			if (!isIAPAvailable)
			{
				onPurchaseComplete?.Invoke(productId, PurchaseStatus.Fail);
				yield break;
			}
			purchseCallBack = onPurchaseComplete;
			m_StoreController.InitiatePurchase(productId);
		}

		IEnumerator CheckInternetConnection(Action<bool> onCheckingDone)
		{
			bool chekingInetrnet = true;
			bool isInternetAvailable = false;
			InternetManager.Instance.CheckForConnection(b =>
			{
				isInternetAvailable = b;
				chekingInetrnet = false;
			});
			while (chekingInetrnet)
				yield return null;
			onCheckingDone?.Invoke(isInternetAvailable);
		}

		IEnumerator WaitForIAPInitlization(Action<bool> onInitCheckingDone)
		{
			if (IsIAPInitialized)
			{
				onInitCheckingDone?.Invoke(true);
				yield break;
			}
			bool waitForInitlization = true;
			InitializeStore((b) => { waitForInitlization = false; });
			while (waitForInitlization)
				yield return null;
			onInitCheckingDone?.Invoke(IsIAPInitialized);
		}

		#endregion

	}

	public class PurchaseProductIds
	{
		public const string NONE = "None";
		public const string COM_OTM_TICKET_1 = "com.otm.ticket_1";
		public const string COM_OTM_TICKET_2 = "com.otm.ticket_2";
		public const string COM_OTM_TICKET_3 = "com.otm.ticket_3";
		public const string COM_OTM_TICKET_4 = "com.otm.ticket_4";
		public const string COM_OTM_TICKET_5 = "com.otm.ticket_5";
		public const string COM_OTM_TICKET_6 = "com.otm.ticket_6";

		public const string COM_OTM_REMOVE_ADS_1 = "com.otm.jigsawsolitaire.remove_ads_1";
		public const string COM_OTM_REMOVE_ADS_2 = "com.otm.jigsawsolitaire.remove_ads_2";

	}

	public enum PurchaseStatus
	{
		Sucess,
		Fail
	}

	public struct LocalPrice
	{
		public string currencyCode;
		public decimal price;
	}
}
#endif