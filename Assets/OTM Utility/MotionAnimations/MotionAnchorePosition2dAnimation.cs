using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.animation
{
	public class MotionAnchorePosition2dAnimation : BaseMotionAnimation
	{
		#region private variables

		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curve = new AnimationCurve(new Keyframe() { time = 0, value = 1 }, new Keyframe() { time = 1, value = 1 });
		[SerializeField] private CurveOffSet2d offSet2D = new CurveOffSet2d();
		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private Vector2 startAnchorPosition;
		[SerializeField] private Vector2 endAnchorPosition;

		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;

		private Vector2 anchorPosition;

		#endregion

		#region overrided methods
		protected override void OnAnimationStart()
		{
			anchorPosition = startAnchorPosition;
			offSet2D.SetRandomCruveValue();
			if (resetOnStart)
				ResetAnimation();
			base.OnAnimationStart();
		}

		public override void Stop()
		{
			base.Stop();
			if (resetOnStop)
				ResetAnimation();
		}

		public override void OnUpdate(float evolution)
		{
			if (rectTransform == null)
				return;
			anchorPosition = Vector3.LerpUnclamped(startAnchorPosition, endAnchorPosition, curve.Evaluate(evolution));
			anchorPosition += offSet2D.Evaluate(evolution);
			SetAnchorPosition(anchorPosition);
		}

		protected override void OnAnimationOver()
		{
			base.OnAnimationOver();
			if (resetOnOver)
				ResetAnimation();
		}

		public override void ResetAnimation()
		{
			base.ResetAnimation();
			SetAnchorPosition(startAnchorPosition);
		}

		public override void SetValue<V>(V startValue, V endValue)
		{
			if (typeof(V) == typeof(RectTransform))
			{
				RectTransform t = startValue as RectTransform;
				startAnchorPosition = t.anchoredPosition;
				t = endValue as RectTransform;
				endAnchorPosition = t.anchoredPosition;
			}
			else if (typeof(V) == typeof(Vector2))
			{
				Vector2? pos = startValue as Vector2?;
				startAnchorPosition = (pos != null) ? pos.Value : Vector2.zero;
				pos = endValue as Vector2?;
				endAnchorPosition = (pos != null) ? pos.Value : Vector2.zero;
			}
			SetAnchorPosition(startAnchorPosition);
		}

		#endregion

		#region private methods

		private void SetAnchorPosition(Vector2 anchoredPosition)
		{
			rectTransform.anchoredPosition = anchoredPosition;
		}

		[Button]
		private void SetStartValue()
		{
			if (rectTransform == null)
				return;
			startAnchorPosition = rectTransform.anchoredPosition;
		}

		[Button]
		private void SetEndValue()
		{
			if (rectTransform == null)
				return;
			endAnchorPosition = rectTransform.anchoredPosition;
		}

		#endregion
	}
}
