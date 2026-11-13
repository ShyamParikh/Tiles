using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.animation
{
	public class RotationAnimation : BaseAnimation
	{
		#region private variables

		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curve = new AnimationCurve();
		[SerializeField] private Transform transform;
		[SerializeField] private Vector3 startRotation;
		[SerializeField] private Vector3 endRotation;

		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;
		[SerializeField, FoldoutGroup("Extra settings")] private bool useLocalRotation = true;

		#endregion

		#region overrided mmethods
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
			if (transform == null)
				return;
			SetRotation(Vector3.Lerp(startRotation, endRotation, curve.Evaluate(evolution)));
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
		public override void SetToStartValue()
		{
			base.SetToStartValue();
			SetRotation(startRotation);
		}

		public override void SetToEndValue()
		{
			base.SetToEndValue();
			SetRotation(endRotation);
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
