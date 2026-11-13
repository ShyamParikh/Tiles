using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace com.otm.ui
{
	public class CustomToggleButton : MonoBehaviour, IPointerClickHandler
	{
		#region  private variables

		[SerializeField] private RectTransform onRectTransform;
		[SerializeField] private RectTransform offRectTransform;
		[SerializeField] private RectTransform handle;
		[SerializeField] private float handleOffset;
		[SerializeField] private bool isOn;
		[SerializeField] UnityEvent<bool> onValueChange = new UnityEvent<bool>();

		private RectTransform mainRectTransform;
		private float size;
		private float handleSize;
		private float position;
		private Coroutine animationCoroutine;

		#endregion

		#region propertices

		public bool IsOn => isOn;

		public UnityEvent<bool> OnValueChange => onValueChange;

		#endregion


		#region  unity callback

		private void Start()
		{
			mainRectTransform = GetComponent<RectTransform>();
			size = mainRectTransform.rect.size.x;
			if (handle != null)
				handleSize = handle.rect.size.x;
			handleSize += handleOffset;
			position = (size - handleSize) / 2f;
			SetToggleLayout();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			isOn = !isOn;
			OnToogleChange();
		}

		public void SetToggle(bool isOn, bool doAnimation = false)
		{
			this.isOn = isOn;
			OnToogleChange(doAnimation);
		}

		#endregion

		#region private methods

		private void SetToggleLayout()
		{
			if (handle != null)
				handle.anchoredPosition = isOn ? new Vector2(position * -1, handle.anchoredPosition.y) : new Vector2(position, handle.anchoredPosition.y);
			if (onRectTransform)
				onRectTransform.sizeDelta = new Vector2(isOn ? size : handleSize, onRectTransform.sizeDelta.y);
			if (offRectTransform)
				offRectTransform.sizeDelta = new Vector2(!isOn ? size : handleSize, offRectTransform.sizeDelta.y);
		}

		private void OnToogleChange(bool doAnimation = true)
		{
			onValueChange?.Invoke(isOn);
			if (animationCoroutine != null)
				StopCoroutine(animationCoroutine);
			if (doAnimation)
			{
				animationCoroutine = StartCoroutine(DoAnchorAnimation());
				return;
			}
			SetToggleLayout();
		}


		#endregion
		#region coroutine

		IEnumerator DoAnchorAnimation()
		{
			float i = 0;
			float rate = 1f / 0.1f;
			Vector2 handPosition = Vector2.zero;
			if (handle != null)
				handPosition = handle.anchoredPosition;
			Vector2 handTargetPosition = isOn ? new Vector2(position * -1, handle.anchoredPosition.y) : new Vector2(position, handle.anchoredPosition.y);

			Vector2 onRestSize = Vector2.zero;
			if (onRectTransform != null)
				onRestSize = onRectTransform.sizeDelta;

			Vector2 onRectTargetSize = new Vector2(isOn ? size : handleSize, onRectTransform.sizeDelta.y);
			Vector2 offRectTargetSize = new Vector2(!isOn ? size : handleSize, onRectTransform.sizeDelta.y);

			Vector2 offRestSize = Vector2.zero;
			if (offRectTransform != null)
				offRestSize = offRectTransform.sizeDelta;

			while (i < 1)
			{
				i += Time.deltaTime * rate;
				if (handle != null)
					handle.anchoredPosition = Vector2.Lerp(handPosition, handTargetPosition, i);
				if (onRectTransform != null)
					onRectTransform.sizeDelta = Vector2.Lerp(onRestSize, onRectTargetSize, i);
				if (offRectTransform != null)
					offRectTransform.sizeDelta = Vector2.Lerp(offRestSize, offRectTargetSize, i);
				yield return null;
			}
		}

		#endregion
	}
}
