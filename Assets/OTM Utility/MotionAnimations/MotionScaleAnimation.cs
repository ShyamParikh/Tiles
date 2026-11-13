using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.animation
{
	public class MotionScaleAnimation : BaseMotionAnimation
	{
		#region private variables

		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curve = new AnimationCurve(new Keyframe() { time = 0, value = 1 }, new Keyframe() { time = 1, value = 1 });
		[SerializeField] private CurveOffSet3d offSet3D = new CurveOffSet3d();
		[SerializeField] private Transform transform;
		[SerializeField] private Vector3 startScale;
		[SerializeField] private Vector3 endScale;

		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;

		private Vector3 scale;

		#endregion

		#region overrided methods
		protected override void OnAnimationStart()
		{
			scale = startScale;
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
			if (transform == null)
				return;
			scale = Vector3.LerpUnclamped(startScale, endScale, curve.Evaluate(evolution));
			scale += offSet3D.Evaluate(evolution);
			SetScale(scale);
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

		public override void SetValue<V>(V startValue, V endValue)
		{
			if (typeof(V) == typeof(Transform))
			{
				Transform t = startValue as Transform;
				startScale = t.localScale;
				t = endValue as Transform;
				endScale = t.localScale;
			}
			else if (typeof(V) == typeof(Vector3))
			{
				Vector3? pos = startValue as Vector3?;
				startScale = (pos != null) ? pos.Value : Vector3.zero;
				pos = endValue as Vector3?;
				endScale = (pos != null) ? pos.Value : Vector3.zero;
			}
			SetScale(startScale);
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
