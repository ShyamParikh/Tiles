using UnityEngine;
using UnityEngine.UI;

namespace com.otm.utility
{
	[ExecuteInEditMode]
	public class ParentContentSizeFitter : MonoBehaviour
	{
		#region private variables
		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private RectTransform contentSizeFitterRectTranform;
		[SerializeField] private bool fixHeight;
		[SerializeField] private bool fixWidth;
		[SerializeField] private float leftPadding;
		[SerializeField] private float rightPadding;
		[SerializeField] private float topPadding;
		[SerializeField] private float bottomPadding;

		private Vector2 minOffset;
		private Vector2 maxOffset;

		#endregion

		#region unity callback

		public void Update()
		{
			if (rectTransform == null || contentSizeFitterRectTranform == null)
				return;
			minOffset.x = (fixWidth) ? rectTransform.offsetMin.x : contentSizeFitterRectTranform.offsetMin.x + leftPadding;
			maxOffset.x = (fixWidth) ? rectTransform.offsetMax.x : contentSizeFitterRectTranform.offsetMax.x + rightPadding;

			minOffset.y = (fixHeight) ? rectTransform.offsetMin.y : contentSizeFitterRectTranform.offsetMin.y + bottomPadding;
			maxOffset.y = (fixHeight) ? rectTransform.offsetMax.y : contentSizeFitterRectTranform.offsetMax.y + topPadding;

			rectTransform.offsetMin = minOffset;
			rectTransform.offsetMax = maxOffset;
		}

		private void OnValidate()
		{
			if (rectTransform == null)
				rectTransform = GetComponent<RectTransform>();
			if (contentSizeFitterRectTranform == null)
			{
				ContentSizeFitter contentSizeFitter = GetComponentInChildren<ContentSizeFitter>();
				if (contentSizeFitter != null)
					contentSizeFitterRectTranform = contentSizeFitter.GetComponent<RectTransform>();
				if (contentSizeFitterRectTranform == null)
					contentSizeFitterRectTranform = GetComponentInChildren<RectTransform>();
			}
		}

		#endregion
	}
}
