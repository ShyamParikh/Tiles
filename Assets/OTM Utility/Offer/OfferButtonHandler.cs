#if OTM_Offer
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.otm.offer
{
	public class OfferButtonHandler : MonoBehaviour
	{
#region private variables
		[SerializeField] private int placementId;
		[SerializeField] private Transform buttonParent;
		[SerializeField] private ScrollRect scrollRect;
		private Dictionary<string, OfferButton> buttons = new Dictionary<string, OfferButton>();
#endregion

#region unity callback

		private void OnEnable()
		{
			OfferManager.OnOfferHide.RegisterEvent(OnOfferHide);
		}

		private void OnDisable()
		{
			OfferManager.OnOfferHide.DeRegisterEvent(OnOfferHide);
		}

#endregion

#region public methods

		[Button]
		public void UpdateButtons()
		{
			CreateButtons();
			UpdateScrollRect();
			foreach (var item in buttons)
				item.Value.ShowButton();
		}

#endregion

#region private methods

		private void OnOfferHide(string offerId)
		{
			UpdateButtons();
		}

		private void UpdateScrollRect()
		{
			if (scrollRect == null)
				return;
			RectTransform rect = scrollRect.GetComponent<RectTransform>();
			if (rect.rect.height >= scrollRect.content.rect.height)
				scrollRect.enabled = false;
			else
				scrollRect.enabled = true;
		}

		private void CreateButtons()
		{
			List<BaseOfferView> offers = OfferManager.Instance.GetOfferForPlacement(placementId);
			for (int i = 0; i < offers.Count; i++)
			{
				if (!buttons.ContainsKey(offers[i].OfferConfig.offerId) && offers[i].OfferButton != null)
				{
					OfferButton btn = Instantiate(offers[i].OfferButton, buttonParent);
					btn.Init(offers[i]);
					buttons.Add(offers[i].OfferConfig.offerId, btn);
				}
			}
		}

#endregion
	}
}
#endif