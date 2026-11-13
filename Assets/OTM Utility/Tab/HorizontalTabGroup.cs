using com.otm.utility;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.otm.ui
{
	public class HorizontalTabGroup : MonoBehaviour
	{
		#region private variables
		[SerializeField] private int defaultSelection;
		[SerializeField] private float normalTabHeight;
		[SerializeField] private Vector2 selectionExpandSize;
		[SerializeField] private RectTransform selectionRect;
		[SerializeField] private float animationDuration;
		[SerializeField] private AnimationCurve animtionCurve;
		[SerializeField] private HorizontalTabItem[] tabs = new HorizontalTabItem[0];
		[SerializeField] private UnityEvent<HorizontalTabItem> onTabSelectionChange = new UnityEvent<HorizontalTabItem>();

		private HorizontalTabItem selectedTab;
		private Coroutine coroutine;

		#endregion

		#region propertices
		public HorizontalTabItem SelectedTab => selectedTab;

		public UnityEvent<HorizontalTabItem> OnTabSelectionChange => onTabSelectionChange;

		public int SelectedTabId => selectedTab.ID;
		public float AnimationDuration => animationDuration;
		public AnimationCurve AnimtionCurve => animtionCurve;

		#endregion

		#region unity callBack

		private void Start()
		{
			Init();
			if (tabs.Length <= defaultSelection)
				return;
			DeselectAllTabItems();
			SetAsSelectedTab(tabs[defaultSelection]);
		}

		#endregion

		#region public methods

		public void SetAsSelectedTab(HorizontalTabItem tabItem)
		{
			if (selectedTab == tabItem)
				return;
			if (selectedTab != null)
				selectedTab.OnTabDeselect();
			selectedTab = tabItem;
			selectedTab.SetAsSelectedTab();
			onTabSelectionChange?.Invoke(selectedTab);
			if (coroutine != null)
				StopCoroutine(coroutine);
			if (gameObject.activeInHierarchy && selectionRect != null)
				StartCoroutine(DoAnimation(selectedTab.RectTransform));
		}
		public void Init()
		{
			InitTabs();
		}

		public void SetTab(int tabId)
		{
			if (selectedTab != null && selectedTab.ID == tabId)
				return;
			for (int i = 0; i < tabs.Length; i++)
			{
				if (tabs[i].ID == tabId)
				{
					SetAsSelectedTab(tabs[i]);
					break;
				}
			}
		}

		public void DeselectAllTabItems()
		{
			for (int i = 0; i < tabs.Length; i++)
			{
				if (tabs[i] != null)
					tabs[i].OnTabDeselect();
			}
		}

		#endregion

		#region private Methods

		private void InitTabs()
		{
			HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
			float padding = 0;
			if (horizontalOrVerticalLayoutGroup != null)
			{
				padding = horizontalOrVerticalLayoutGroup.spacing * (tabs.Length - 1);
				padding += horizontalOrVerticalLayoutGroup.padding.left;
				padding += horizontalOrVerticalLayoutGroup.padding.right;
			}
			float screenWidth = (Screen.width - (selectionExpandSize.x + padding)) / tabs.Length;
			Vector2 size = new Vector2(screenWidth, normalTabHeight);
			if (selectionRect != null)
				selectionRect.sizeDelta = size + selectionExpandSize;
			for (int i = 0; i < tabs.Length; i++)
			{
				tabs[i].Init(this, size, selectionExpandSize);
			}
		}

		#endregion

		#region  Coroutine
		IEnumerator DoAnimation(RectTransform rectTransform)
		{
			float i = 0;
			float rate = 1f / animationDuration;
			Vector2 pos = selectionRect.anchoredPosition;
			while (i <= 1f)
			{
				i += Time.deltaTime * rate;
				selectionRect.anchoredPosition = Vector2.LerpUnclamped(pos, rectTransform.anchoredPosition, animtionCurve.Evaluate(i));
				yield return null;
			}
		}
		#endregion
	}
}
