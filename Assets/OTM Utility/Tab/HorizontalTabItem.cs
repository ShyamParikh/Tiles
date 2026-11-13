using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.otm.ui
{
	public class HorizontalTabItem : MonoBehaviour
	{
		#region private variables
		[SerializeField] private int tabId;

		[SerializeField] private UnityEvent onTabSelect = new UnityEvent();
		[SerializeField] private UnityEvent onTabDeSelect = new UnityEvent();
		private Button button;
		private HorizontalTabGroup tabGroup;
		private RectTransform rectTransform;
		[ShowInInspector] private Vector2 size;
		[ShowInInspector] private Vector2 selectionSize;

		private Coroutine coroutine;

		#endregion

		#region propertices
		public int ID => tabId;

		public RectTransform RectTransform => rectTransform;

		#endregion

		#region unity callback

		private void OnValidate()
		{
			if (button == null)
				button = GetComponent<Button>();
		}

		#endregion

		#region public methods
		public void Init(HorizontalTabGroup tabGroup, Vector2 normalSize, Vector2 selectionSize)
		{
			size = normalSize;
			this.selectionSize = normalSize + selectionSize;
			if (rectTransform == null)
				rectTransform = GetComponent<RectTransform>();
			if (rectTransform != null)
				rectTransform.sizeDelta = normalSize;
			this.tabGroup = tabGroup;
			if (tabGroup.SelectedTab == this)
				OnTabSelect();
			if (button != null)
			{
				button.onClick.RemoveListener(OnTabSelect);
				button.onClick.AddListener(OnTabSelect);
			}
		}

		public void OnTabSelect()
		{
			if (tabGroup.SelectedTab == this)
				return;
			tabGroup.SetAsSelectedTab(this);
			onTabSelect?.Invoke();
		}


		public void SetAsSelectedTab()
		{
			PlayWidthAnimation(selectionSize);
			onTabSelect?.Invoke();
		}

		public void OnTabDeselect()
		{
			PlayWidthAnimation(size);
			onTabDeSelect?.Invoke();
		}

		#endregion

		#region  private methods

		private void PlayWidthAnimation(Vector2 size)
		{
			if (coroutine != null)
				StopCoroutine(coroutine);
			if (gameObject.activeInHierarchy)
				coroutine = StartCoroutine(DoAnimation(size));
		}

		#endregion

		#region  Coroutine
		IEnumerator DoAnimation(Vector2 size)
		{
			float i = 0;
			float rate = 1f / tabGroup.AnimationDuration;
			AnimationCurve animationCurve = tabGroup.AnimtionCurve;
			Vector2 startSize = rectTransform.rect.size;
			while (i < 1f)
			{
				i += Time.deltaTime * rate;
				rectTransform.sizeDelta = Vector2.Lerp(startSize, size, animationCurve.Evaluate(i));
				yield return null;
			}
		}
		#endregion
	}
}
