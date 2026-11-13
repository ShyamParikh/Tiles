#if OTM_Offer
using com.otm.firebase;
using com.otm.manager;
using com.otm.utility;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.otm.offer
{
	public class OfferManager : Manager<OfferManager>
	{
#region private variables
		[SerializeField] private Transform offerParent;
		[SerializeField] private BaseOfferView[] localOffers = new BaseOfferView[0];

		private string[] offerIds = new string[0];
		private List<BaseOfferView> offers = new List<BaseOfferView>();
		private List<BaseOfferView> shownOffers = new List<BaseOfferView>();
		private Dictionary<string, Coroutine> offerTimer = new Dictionary<string, Coroutine>();
		private WaitForSecondsRealtime waitForSeconds;

#endregion

#region public static variables

		public static ActionSystem<string, TimeSpan> OnOfferTimeUpdate = new ActionSystem<string, TimeSpan>();
		public static ActionSystem<string> OnOfferTimeOver = new ActionSystem<string>();
		public static ActionSystem<string> OnOfferHide = new ActionSystem<string>();
		public static ActionSystem<string> OnOfferShow = new ActionSystem<string>();

#endregion

#region overrided methods

		public override void Awake()
		{
			waitForSeconds = new WaitForSecondsRealtime(1);
			base.Awake();
			StartCoroutine(WaitForFirebaseRemoteConfigInitialization());
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			OnOfferTimeUpdate.ClearEvents();
			OnOfferTimeOver.ClearEvents();
		}

#endregion

#region public methods

		public bool ShowOffer(OfferTag offerTag, Action onOfferHide = null)
		{
			for (int i = 0; i < offers.Count; i++)
			{
				if (offers[i].IsValid() && offers[i].OfferConfig.offerTag == offerTag && offers[i].CanShowOffer())
				{
					offers[i].ShowView(onOfferHide);
					return true;
				}
			}
			return false;
		}

		public void ShowOffer(BaseOfferView baseOfferView, Action onOfferHide = null)
		{
			for (int i = 0; i < offers.Count; i++)
			{
				if (offers[i] == baseOfferView)
				{
					offers[i].ShowView(onOfferHide);
				}
			}
		}

		public void AddToShownOffer(BaseOfferView baseOfferView)
		{
			if (!shownOffers.Contains(baseOfferView))
				shownOffers.Add(baseOfferView);
		}

		public bool IsOfferShown(BaseOfferView baseOfferView)
		{
			return shownOffers.Contains(baseOfferView);
		}

		public void StartOfferTimer(string offerId, DateTime endDate)
		{
			if (!offerTimer.ContainsKey(offerId))
				offerTimer.Add(offerId, StartCoroutine(DoTimer(offerId, endDate)));
		}

		public List<BaseOfferView> GetOfferForPlacement(int placementId)
		{
			List<BaseOfferView> placementOffers = new List<BaseOfferView>();
			for (int i = 0; i < offers.Count; i++)
			{
				if (offers[i].IsValid() && offers[i].OfferConfig.placementId == placementId)
					placementOffers.Add(offers[i]);
			}
			return placementOffers;
		}

#endregion

#region private methods

		private void CreateOffers()
		{
			offerIds = FirebaseManager.FirebaseRemoteConfig.GetValue<string[]>(FirebaseRemoteConfigConstant.OFFERS_ID);
			for (int i = 0; i < offerIds.Length; i++)
				LoadOffer(offerIds[i]);
		}

		private void LoadOffer(string offerId)
		{
			string data = FirebaseManager.FirebaseRemoteConfig.GetValue(offerId);
			if (string.IsNullOrEmpty(data))
				return;
			OfferConfig offerConfig = JsonConvert.DeserializeObject<OfferConfig>(data);
			BaseOfferView baseOfferView = GetLocalOffer(offerConfig.prefabName);
			if (baseOfferView != null)
			{
				BaseOfferView b = Instantiate(baseOfferView, offerParent);
				b.Init(data);
				offers.Add(b);
				offers = offers.OrderBy(o => o.IsValid()).ThenBy(o => o.Priority).ToList();
				return;
			}

			AssetBundleManager.Instance.LoadAsset<GameObject>(offerConfig.bundleName, offerConfig.prefabName, g =>
			{
				if (g == null)
					return;
				BaseOfferView baseOfferView = g.GetComponent<BaseOfferView>();
				if (baseOfferView == null)
					return;
				BaseOfferView b = Instantiate(baseOfferView, offerParent);
				b.Init(data);
				offers.Add(b);
				offers = offers.OrderBy(o => o.IsValid()).ThenBy(o => o.Priority).ToList();
			});
		}

		private BaseOfferView GetLocalOffer(string offerName)
		{
			for (int i = 0; i < localOffers.Length; i++)
			{
				Debug.Log(offerName);
				Debug.Log(localOffers[i].name);
				if (localOffers[i].name.Equals(offerName))
					return localOffers[i];
			}
			return null;
		}

#endregion

#region coroutine

		IEnumerator DoTimer(string offerId, DateTime endDate)
		{
			while (endDate > DateTime.UtcNow)
			{
				yield return waitForSeconds;
				OnOfferTimeUpdate.Invoke(offerId, endDate - DateTime.UtcNow);
			}
			offerTimer.Remove(offerId);
			OnOfferTimeOver.Invoke(offerId);
			for (int i = 0; i < offers.Count; i++)
			{
				if (offers[i].OfferConfig.offerId.Equals(offerId))
					offers[i].UpdateOffer();
			}
		}

		IEnumerator WaitForFirebaseRemoteConfigInitialization()
		{
			while (!FirebaseManager.FirebaseRemoteConfig.isInit)
                yield return null;
			CreateOffers();
		}

#endregion
	}
}
#endif