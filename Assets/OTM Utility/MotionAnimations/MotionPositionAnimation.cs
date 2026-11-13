using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.animation
{
	public class MotionPositionAnimation : BaseMotionAnimation
	{
		#region private variables

		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curve = new AnimationCurve(new Keyframe() { time = 0, value = 1 }, new Keyframe() { time = 1, value = 1 });
		[SerializeField] private CurveOffSet3d offSet3D = new CurveOffSet3d();
		[SerializeField] private Transform transform;
		[SerializeField] private Vector3 startPosition;
		[SerializeField] private Vector3 endPosition;
		[SerializeField] private bool useLocalPosition;

		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;

		private Vector3 position;

		#endregion

		#region overrided methods
		protected override void OnAnimationStart()
		{
			position = startPosition;
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
			position = Vector3.LerpUnclamped(startPosition, endPosition, curve.Evaluate(evolution));
			position += offSet3D.Evaluate(evolution);
			SetPosition(position);
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
			SetPosition(startPosition);
		}

		public override void SetValue<V>(V startValue, V endValue)
		{
			if (typeof(V) == typeof(Transform))
			{
				Transform t = startValue as Transform;
				startPosition = (useLocalPosition) ? t.localPosition : t.position;
				t = endValue as Transform;
				endPosition = (useLocalPosition) ? t.localPosition : t.position;
			}
			else if (typeof(V) == typeof(Vector3))
			{
				Vector3? pos = startValue as Vector3?;
				startPosition = (pos != null) ? pos.Value : Vector3.zero;
				pos = endValue as Vector3?;
				endPosition = (pos != null) ? pos.Value : Vector3.zero;
			}
			else if (typeof(V) == typeof(CurveOffSet3d))
			{
				if (startValue != null)
					offSet3D = startValue as CurveOffSet3d;
				else if (endValue != null)
					offSet3D = startValue as CurveOffSet3d;
			}
			SetPosition(startPosition);
		}

		#endregion

		#region private methods

		private void SetPosition(Vector3 position)
		{
			if (useLocalPosition)
				transform.localPosition = position;
			else
				transform.position = position;
		}

		[Button]
		private void SetStartValue()
		{
			if (transform == null)
				return;
			startPosition = (useLocalPosition) ? transform.localPosition : transform.position;
		}

		[Button]
		private void SetEndValue()
		{
			if (transform == null)
				return;
			endPosition = (useLocalPosition) ? transform.localPosition : transform.position;
		}

		#endregion
	}
}
