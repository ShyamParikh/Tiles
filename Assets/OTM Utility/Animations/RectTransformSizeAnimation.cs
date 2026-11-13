using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.animation
{
	public class RectTransformSizeAnimation : BaseAnimation
	{
		#region private variables

		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curve = new AnimationCurve();
		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private Vector2 startSize;
		[SerializeField] private Vector2 endSize;

		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;

		#endregion

		#region overrided methods
		protected override void OnAnimationStart()
		{
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
			SetSize(Vector3.Lerp(startSize, endSize, curve.Evaluate(evolution)));
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
			SetSize(startSize);
		}

		public override void SetToStartValue()
		{
			base.SetToStartValue();
			SetSize(startSize);
		}
		public override void SetToEndValue()
		{
			base.SetToEndValue();
			SetSize(endSize);
		}

		#endregion

		#region private methods

		private void SetSize(Vector2 size)
		{
			rectTransform.sizeDelta = size;
		}

		[Button]
		private void SetStartValue()
		{
			if (rectTransform == null)
				return;
			startSize = rectTransform.sizeDelta;
		}

		[Button]
		private void SetEndValue()
		{
			if (rectTransform == null)
				return;
			endSize = rectTransform.sizeDelta;
		}

		#endregion
	}
}
