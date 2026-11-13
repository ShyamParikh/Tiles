using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.animation
{
	public class MotionSizeDeltaAnimation : BaseMotionAnimation
	{
		#region private variables

		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curve = new AnimationCurve(new Keyframe() { time = 0, value = 1 }, new Keyframe() { time = 1, value = 1 });
		[SerializeField] private CurveOffSet2d offSet2D = new CurveOffSet2d();
		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private Vector2 startSizeDelta;
		[SerializeField] private Vector2 endSizeDelta;

		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;

		private Vector2 sizeDelta;

		#endregion

		#region overrided methods
		protected override void OnAnimationStart()
		{
			sizeDelta = startSizeDelta;
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
			sizeDelta = Vector3.LerpUnclamped(startSizeDelta, endSizeDelta, curve.Evaluate(evolution));
			sizeDelta += offSet2D.Evaluate(evolution);
			SetSizeDelta(sizeDelta);
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
			SetSizeDelta(startSizeDelta);
		}

		public override void SetValue<V>(V startValue, V endValue)
		{
			if (typeof(V) == typeof(RectTransform))
			{
				RectTransform t = startValue as RectTransform;
				startSizeDelta = new Vector2(t.rect.width, t.rect.height);
				t = endValue as RectTransform;
				endSizeDelta = new Vector2(t.rect.width, t.rect.height); ;
			}
			else if (typeof(V) == typeof(Vector2))
			{
				Vector2? pos = startValue as Vector2?;
				startSizeDelta = (pos != null) ? pos.Value : Vector2.zero;
				pos = endValue as Vector2?;
				endSizeDelta = (pos != null) ? pos.Value : Vector2.zero;
			}

			SetSizeDelta(startSizeDelta);
		}

		#endregion

		#region private methods

		private void SetSizeDelta(Vector2 sizeDelta)
		{
			rectTransform.sizeDelta = sizeDelta;
		}

		[Button]
		private void SetStartValue()
		{
			if (rectTransform == null)
				return;
			startSizeDelta = rectTransform.sizeDelta;
		}

		[Button]
		private void SetEndValue()
		{
			if (rectTransform == null)
				return;
			endSizeDelta = rectTransform.sizeDelta;
		}

		#endregion
	}
}
