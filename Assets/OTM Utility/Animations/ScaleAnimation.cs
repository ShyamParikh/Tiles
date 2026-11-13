using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.animation
{
	public class ScaleAnimation : BaseAnimation
	{
		#region private variables

		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curve = new AnimationCurve();
		[SerializeField] private Transform transform;
		[SerializeField] private bool useCurrentValue;
		[SerializeField] private bool useCurveValueMultiplication;
		[SerializeField] private Vector3 startScale = Vector3.one;
		[SerializeField] private Vector3 endScale;

		private Vector3 startValue;

		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;

		#endregion

		#region overrided mmethods
		protected override void OnAnimationStart()
		{
			if (resetOnStart)
				ResetAnimation();
			base.OnAnimationStart();
			if (transform == null)
				return;
			startValue = (useCurrentValue) ? transform.localScale : startScale;
		}

		public override void Stop()
		{
			base.Stop();
			if (resetOnStop)
				ResetAnimation();
		}

		public override void OnUpdate(float evolution)
		{
			if (transform == null)
				return;
			if (useCurveValueMultiplication)
				SetScale(startValue * curve.Evaluate(evolution));
			else
				SetScale(Vector3.LerpUnclamped(startValue, endScale, curve.Evaluate(evolution)));
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
			SetScale(startScale);
		}

		public override void SetToStartValue()
		{
			base.SetToStartValue();
			SetScale(startScale);
		}

		public override void SetToEndValue()
		{
			base.SetToEndValue();
			SetScale(endScale);
		}

		#endregion

		#region private methods

		private void SetScale(Vector3 scale)
		{
			transform.localScale = scale;
		}

		[Button]
		private void SetStartValue()
		{
			if (transform == null)
				return;
			startScale = transform.localScale;
		}

		[Button]
		private void SetEndValue()
		{
			if (transform == null)
				return;
			endScale = transform.localScale;
		}

		#endregion
	}
}
