using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.otm.animation
{
	public class ScaleSeparateAxisAnimation : BaseAnimation
	{
		#region private variables

		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curveX = new AnimationCurve();
		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curveY = new AnimationCurve();
		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curveZ = new AnimationCurve();
		[SerializeField] private Transform transform;
		[SerializeField] private Vector3 startScale = Vector3.one;
		[SerializeField] private Vector3 endScale;

		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;

		private Vector3 scale;

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
			if (startScale != endScale)
			{
				scale.x = Mathf.LerpUnclamped(startScale.x, endScale.x, curveX.Evaluate(evolution));
				scale.y = Mathf.LerpUnclamped(startScale.y, endScale.y, curveY.Evaluate(evolution));
				scale.z = Mathf.LerpUnclamped(startScale.z, endScale.z, curveZ.Evaluate(evolution));
			}
			else
			{
				scale.x = startScale.x * curveX.Evaluate(evolution);
				scale.y = startScale.y * curveY.Evaluate(evolution);
				scale.z = startScale.z * curveZ.Evaluate(evolution);
			}
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
			if (transform == null)
				return;
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
