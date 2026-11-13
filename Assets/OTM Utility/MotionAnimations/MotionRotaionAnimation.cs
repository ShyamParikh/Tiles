using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.animation
{
	public class MotionRotaionAnimation : BaseMotionAnimation
	{
		#region private variables

		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curve = new AnimationCurve(new Keyframe() { time = 0, value = 1 }, new Keyframe() { time = 1, value = 1 });
		[SerializeField] private CurveOffSet3d offSet3D = new CurveOffSet3d();
		[SerializeField] private Transform transform;
		[SerializeField] private Vector3 startRotation;
		[SerializeField] private Vector3 endRotation;
		[SerializeField] private bool useLocalRotation = true;

		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;

		private Vector3 rotation;

		#endregion

		#region overrided methods
		protected override void OnAnimationStart()
		{
			rotation = startRotation;
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
			rotation = Vector3.LerpUnclamped(startRotation, endRotation, curve.Evaluate(evolution));
			rotation += offSet3D.Evaluate(evolution);
			SetRotation(rotation);
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
			SetRotation(startRotation);
		}

		public override void SetValue<V>(V startValue, V endValue)
		{
			if (typeof(V) == typeof(Transform))
			{
				Transform t = startValue as Transform;
				startRotation = (useLocalRotation) ? t.localEulerAngles : t.eulerAngles;
				t = endValue as Transform;
				endRotation = (useLocalRotation) ? t.localEulerAngles : t.eulerAngles;
			}
			else if (typeof(V) == typeof(Vector3))
			{
				Vector3? pos = startValue as Vector3?;
				startRotation = (pos != null) ? pos.Value : Vector3.zero;
				pos = endValue as Vector3?;
				endRotation = (pos != null) ? pos.Value : Vector3.zero;
			}
			SetRotation(startRotation);
		}

		#endregion

		#region private methods

		private void SetRotation(Vector3 rotation)
		{
			if (useLocalRotation)
				transform.localEulerAngles = rotation;
			else
				transform.eulerAngles = rotation;
		}

		[Button]
		private void SetStartValue()
		{
			if (transform == null)
				return;
			startRotation = (useLocalRotation) ? transform.localEulerAngles : transform.eulerAngles;
		}

		[Button]
		private void SetEndValue()
		{
			if (transform == null)
				return;
			endRotation = (useLocalRotation) ? transform.localEulerAngles : transform.eulerAngles;
		}

		#endregion
	}
}
