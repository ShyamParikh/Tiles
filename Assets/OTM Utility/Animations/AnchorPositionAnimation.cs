using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.animation
{
	public class AnchorPositionAnimation : BaseAnimation
	{
		#region private variables

		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curve = new AnimationCurve();
		[SerializeField] private RectTransform rectTransform;
		[SerializeField, HideIf("useCurrentPositionAsStart")] private Vector3 startAnchorPosition;
		[SerializeField, HideIf("useCurrentPositionAsEnd")] private Vector3 endAnchorPosition;
		[SerializeField] private bool use3dPosition;
		[SerializeField] private bool useCurrentPositionAsStart;
		[SerializeField] private bool useCurrentPositionAsEnd;

		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;

		#endregion

		#region overrided methods
		protected override void OnAnimationStart()
		{
			if (resetOnStart)
				ResetAnimation();
			if (useCurrentPositionAsStart)
				startAnchorPosition = use3dPosition ? rectTransform.anchoredPosition3D : rectTransform.anchoredPosition;
			if (useCurrentPositionAsEnd)
				endAnchorPosition = use3dPosition ? rectTransform.anchoredPosition3D : rectTransform.anchoredPosition;
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
			SetAnchorPosition(Vector3.LerpUnclamped(startAnchorPosition, endAnchorPosition, curve.Evaluate(evolution)));
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

		public override void SetToStartValue()
		{
			base.SetToStartValue();
			SetAnchorPosition(startAnchorPosition);
		}

		public override void SetToEndValue()
		{
			base.SetToEndValue();
			SetAnchorPosition(endAnchorPosition);
		}

		#endregion

		#region private methods

		private void SetAnchorPosition(Vector3 position)
		{
			if (use3dPosition)
				rectTransform.anchoredPosition3D = position;
			else
				rectTransform.anchoredPosition = position;
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
			if (rectTransform == null)
				return;
			endAnchorPosition = rectTransform.anchoredPosition3D;
		}

		#endregion
	}
}
