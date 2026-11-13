using Sirenix.OdinInspector;
using UnityEngine;


namespace com.otm.animation
{
	public class PositionAnimation : BaseAnimation
	{
		#region private variables

		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curve = new AnimationCurve();
		[SerializeField] private Transform transform;
		[SerializeField] private Vector3 startPosition;
		[SerializeField] private Vector3 endPosition;

		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool useCurrentAsStartPosition;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;
		[SerializeField, FoldoutGroup("Extra settings")] private bool useLocalPosition = true;

		#endregion

		#region overrided methods
		protected override void OnAnimationStart()
		{
			if (useCurrentAsStartPosition && transform != null)
				startPosition = useLocalPosition ? transform.localPosition : transform.position;
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
			SetPosition(Vector3.LerpUnclamped(startPosition, endPosition, curve.Evaluate(evolution)));
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

		public override void SetToStartValue()
		{
			base.SetToStartValue();
			SetPosition(startPosition);
		}

		public override void SetToEndValue()
		{
			base.SetToEndValue();
			SetPosition(endPosition);
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
