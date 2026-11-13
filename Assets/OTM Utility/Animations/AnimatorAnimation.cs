using com.otm.utility;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.animation
{
	public class AnimatorAnimation : BaseAnimation
	{
		#region private variables
		[SerializeField] private Animator animator;
		[SerializeField, ValueDropdown("AnimationName")]
		private string animationName;
		[SerializeField, ValueDropdown("AnimationName")]
		private string resetAnimationName;

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
			if (animator)
				animator.Play(animationName);
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
			if (animator && !string.IsNullOrEmpty(resetAnimationName))
				animator.Play(resetAnimationName);
		}

		public override void SetToStartValue()
		{
			base.SetToStartValue();
			if (animator == null)
				return;
			animator.UpdateAnimation(animationName, 0);
		}

		public override void SetToEndValue()
		{
			base.SetToEndValue();
			if (animator == null)
				return;
			animator.UpdateAnimation(animationName, 1f);
		}

		#endregion

#if UNITY_EDITOR

		private List<string> AnimationName()
		{
			List<string> animationNames = new List<string>();
			if (animator == null)
				return animationNames;
			UnityEditor.Animations.AnimatorController animatorController = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
			foreach (var l in animatorController.layers)
			{
				foreach (var s in l.stateMachine.states)
				{
					animationNames.Add(s.state.name);
				}
			}
			return animationNames;
		}

		[Button("SetAnimationTime")]
		private void OnAnimationStateChange()
		{
			if (animator == null)
				return;
			UnityEditor.Animations.AnimatorController animatorController = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
			foreach (var l in animatorController.layers)
			{
				foreach (var s in l.stateMachine.states)
				{
					if (s.state.name.Equals(animationName))
					{
						AnimationClip clip = s.state.motion as AnimationClip;
						if (clip != null)
							duration = clip.length / s.state.speed;
						else
							duration = 0;
					}
				}
			}
		}

#endif
	}
}
