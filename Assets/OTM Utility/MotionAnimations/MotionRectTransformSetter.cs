using UnityEngine;

namespace com.otm.animation
{
	public class MotionRectTransformSetter : BaseMotionAnimation
	{
		#region private variables

		[SerializeField] private RectTransform rectTransform;

		#endregion

		#region overrided methods
		public override void OnUpdate(float evolution) { }

		public override void SetValue<V>(V startValue, V endValue)
		{
			RectTransform rect = startValue as RectTransform;
			if (rect != null)
			{
				rectTransform.position = rect.position;
				rectTransform.rotation = rect.rotation;
				rectTransform.pivot = rect.pivot;
				rectTransform.sizeDelta = new Vector2(rect.rect.width, rect.rect.height);
				return;
			}
			rect = endValue as RectTransform;
			if (rect != null)
			{
				rectTransform.position = rect.position;
				rectTransform.rotation = rect.rotation;
				rectTransform.pivot = rect.pivot;
				rectTransform.sizeDelta = new Vector2(rect.rect.width, rect.rect.height);
			}
		}
		#endregion
	}
}
