using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.animation
{
    public class MotionAnchorePosition3dAnimation : BaseMotionAnimation
	{
		#region private variables

		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curve = new AnimationCurve(new Keyframe() { time = 0, value = 1 }, new Keyframe() { time = 1, value = 1 });
		[SerializeField] private CurveOffSet3d offSet3D = new CurveOffSet3d();
		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private Vector3 startAnchorPosition;
		[SerializeField] private Vector3 endAnchorPosition;

		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;

		private Vector3 anchorPosition;
		private Vector3 randomCurveValue;

		#endregion

		#region overrided methods
		protected override void OnAnimationStart()
		{
			anchorPosition = startAnchorPosition;
			offSet3D.SetRandomCruveValue();
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
			anchorPosition += offSet3D.Evaluate(evolution);
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
			if (typeof(V) == typeof(Transform))
			{
				RectTransform t = startValue as RectTransform;
				startAnchorPosition = t.anchoredPosition3D;
				t = endValue as RectTransform;
				endAnchorPosition = t.anchoredPosition3D;
			}
			else if (typeof(V) == typeof(Vector3))
			{
				Vector3? pos = startValue as Vector3?;
				startAnchorPosition = (pos != null) ? pos.Value : Vector3.zero;
				pos = endValue as Vector3?;
				endAnchorPosition = (pos != null) ? pos.Value : Vector3.zero;
			}
			SetAnchorPosition(startAnchorPosition);
		}

		#endregion

		#region private methods

		private void SetAnchorPosition(Vector3 anchoredPosition)
		{
			rectTransform.anchoredPosition3D = anchoredPosition;
		}

		[Button]
		private void SetStartValue()
		{
			if (rectTransform == null)
				return;
			startAnchorPosition = rectTransform.anchoredPosition3D;
		}

		[Button]
		private void SetEndValue()
		{
			if (rectTransform ==null)
				return;
			endAnchorPosition = rectTransform.anchoredPosition3D;
		}

		#endregion
	}
}
