using Sirenix.OdinInspector;
using System;
using UnityEngine;


namespace com.otm.ui
{
	public class RectFillBar : MonoBehaviour
	{
		#region private veriables

		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private RectTransform.Axis axis;

		[SerializeField] private float width;
		[SerializeField] private float height;

		[SerializeField, Range(0, 1)] private float fillAmount = 1;

		#endregion

		#region propertices

		public float Width => width;
		public float Height => height;

		#endregion

		#region unity callBack

		private void OnValidate()
		{
			if (rectTransform == null)
				return;
			if (width <= 0)
				width = rectTransform.sizeDelta.x;
			if (height <= 0)
				height = rectTransform.sizeDelta.y;
			Fill(fillAmount);
		}

		#endregion


		#region public methods

		public void Fill(float fillAmount)
		{
			fillAmount = Mathf.Clamp(fillAmount, 0, 1);
			this.fillAmount = fillAmount;
			if (axis == RectTransform.Axis.Horizontal)
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * fillAmount);
			if (axis == RectTransform.Axis.Vertical)
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * fillAmount);
		}

		public float GetFillAmount()
		{
			return fillAmount;
		}

		#endregion

		#region private methods

		[Button]
		private void SetHeightAndWidth()
		{
			var rect = rectTransform.rect;
			width = rect.width;
			height = rect.height;
		}

		#endregion
	}
}