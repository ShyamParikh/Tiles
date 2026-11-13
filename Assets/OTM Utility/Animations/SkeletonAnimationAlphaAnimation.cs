
#if UNITY_SPINE
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace com.otm.animation
{
	public class SkeletonAnimationAlphaAnimation : BaseAnimation
	{
#region private variables

		[SerializeField] private float startAlpha;
		[SerializeField] private float endAlpha;
		[SerializeField] private bool useCurrentAlpha;
		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curve = new AnimationCurve();

		[SerializeField] private SkeletonAnimation skeletonAnimation;

		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;

		private Color color = Color.white;
		private float startValue;

#endregion

#region overrided mmethods
		protected override void OnAnimationStart()
		{
			base.OnAnimationStart();
			color = skeletonAnimation.skeleton.GetColor();
			SetStartAlpha();
			if (resetOnStart)
				ResetAnimation();
		}

		public override void Stop()
		{
			base.Stop();
			if (resetOnStop)
				ResetAnimation(); ;
		}

		public override void OnUpdate(float evolution)
		{
			SetAlpha(Mathf.Lerp(startValue, endAlpha, curve.Evaluate(evolution)));
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
			SetAlpha(startAlpha);
		}

		public override void SetToStartValue()
		{
			base.SetToStartValue();
			SetAlpha(startAlpha);
		}

		public override void SetToEndValue()
		{
			base.SetToEndValue();
			SetAlpha(endAlpha);
		}

#endregion

#region private methods

		private void SetStartAlpha()
		{
			if (!useCurrentAlpha)
			{
				startValue = startAlpha;
				return;
			}
			startValue = skeletonAnimation.skeleton.GetColor().a;
		}

		private void SetAlpha(float alpha)
		{
			color.a = alpha;
			if (skeletonAnimation.skeleton != null)
				skeletonAnimation.skeleton.SetColor(color);
		}

#endregion
	}
} 
#endif