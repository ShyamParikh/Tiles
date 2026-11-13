using com.otm.utility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.animation
{
	public class AnimationAnimation : BaseAnimation
	{
		#region private variables
		[SerializeField] private Animation animation;
		[SerializeField]
		private AnimationClip animationClip;
		[SerializeField]
		private AnimationClip resetAnimationClip;

		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver = true;
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
		}

		protected override void OnAnimationStartDelayOver()
		{
			base.OnAnimationStartDelayOver();
			if (animation)
				animation.Play(animationClip.name);
		}

		protected override void OnAnimationOver()
		{
			if (resetOnOver)
				ResetAnimation();
			base.OnAnimationOver();
		}

		public override void ResetAnimation()
		{
			base.ResetAnimation();
			if (animation && resetAnimationClip != null)
				animation.Play(resetAnimationClip.name);
		}

		public override void SetToStartValue()
		{
			base.SetToStartValue();
			if (animationClip == null || animation == null)
				return;
			animationClip.UpdateAnimation(animation.gameObject, 0);
		}

		public override void SetToEndValue()
		{
			base.SetToEndValue();
			if (animationClip == null || animation == null)
				return;
			animationClip.UpdateAnimation(animation.gameObject, 1);
		}


		#endregion

#if UNITY_EDITOR

		[Button("SetAnimationTime")]
		private void OnAnimationStateChange()
		{
			if (animationClip != null)
				duration = animationClip.length;
		}

#endif
	}
}
