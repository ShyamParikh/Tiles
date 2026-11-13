using com.otm.ui;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.otm.tutorial
{
	public class TutorialButtonView : BaseView
	{
		#region private variables

		private Button tutorialButton;
		[ShowInInspector] private List<RectTransform> parents = new List<RectTransform>();
		private List<RectTransform> updateRect = new List<RectTransform>();
		private bool canUpadte;

		#endregion

		#region unity callback

		private void Update()
		{
			if (!canUpadte)
				return;
			for (int i = 0; i < parents.Count; i++)
			{
				SetRect(updateRect[i], parents[i]);
			}
		}

		#endregion

		#region overrided methods
		public void ShowView(Button button, Action onButtonClick)
		{
			canUpadte = false;
			base.ShowView();
			Transform buttonParent = SetParents(button.transform);
			canUpadte = true;
			tutorialButton = Instantiate(button, buttonParent);
			tutorialButton.onClick = new Button.ButtonClickedEvent();
			tutorialButton.onClick.AddListener(() => { onButtonClick?.Invoke(); });
			ChangeLayerOnButton(tutorialButton.gameObject);
			tutorialButton.gameObject.SetActive(true);
			button.gameObject.SetActive(false);
		}

		protected override void OnViewHideDone()
		{
			base.OnViewHideDone();
			Destroy(gameObject);
		}

		#endregion
		#region private methods

		private Transform SetParents(Transform buttonTransform)
		{
			Transform t = buttonTransform.parent;
			while (t != null)
			{
				RectTransform r = t.GetComponent<RectTransform>();
				t = t.parent;
				parents.Insert(0, r);
			}
			t = transform;
			for (int i = 1; i < parents.Count; i++)
			{
				GameObject g = new GameObject(parents[i].name);
				g.transform.SetParent(t);
				RectTransform r = g.AddComponent<RectTransform>();
				SetRect(r, parents[i]);
				updateRect.Add(r);
				t = g.transform;
			}
			parents.RemoveAt(0);
			return t;
		}


		private void SetRect(RectTransform targetRect, RectTransform sourceRect)
		{
			targetRect.position = sourceRect.position;
			targetRect.rotation = sourceRect.rotation;
			targetRect.sizeDelta = sourceRect.sizeDelta;
			targetRect.anchorMin = sourceRect.anchorMin;
			targetRect.anchorMax = sourceRect.anchorMax;
			targetRect.anchoredPosition = sourceRect.anchoredPosition;
			targetRect.pivot = sourceRect.pivot;
			targetRect.localScale = sourceRect.localScale;
		}

		private void ChangeLayerOnButton(GameObject gameObject)
		{
			Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
			for (int i = 0; i < transforms.Length; i++)
			{
				transforms[i].gameObject.layer = LayerMask.NameToLayer("Tutorial");
			}
		}
		#endregion

	}
}
