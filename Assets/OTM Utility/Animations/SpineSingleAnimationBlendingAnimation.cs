#if UNITY_SPINE
using UnityEngine;

namespace com.otm.animation
{
	public class SpineSingleAnimationBlendingAnimation : BaseAnimation
	{
#region private variables

		[SerializeField] private SpineSingleAnimationBlening animationBlening;
		[SerializeField, Range(0, 1f)] private float startValue;
		[SerializeField, Range(0, 1f)] private float endValue;
		[SerializeField] private AnimationCurve animationCurve = new AnimationCurve();

#endregion
		public override void OnUpdate(float evolution)
		{
			animationBlening.SetBlendValue(Mathf.Lerp(startValue, endValue, animationCurve.Evaluate(evolution)));
		}

		public override void ResetAnimation()
		{
			base.ResetAnimation();
			animationBlening.SetBlendValue(0);
		}
	}
}
#endif