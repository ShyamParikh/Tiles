using com.otm.utility;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.animation
{
	public class AnimatorSingleAnimationBlending : BaseAnimation
	{
		#region private variables
		[SerializeField] private Animator animator;
		[SerializeField, ValueDropdown("AnimationName")]
		private string animationName;

		#endregion

		#region override methods

		public override void OnUpdate(float evolution)
		{
			animator.Play(animationName, -1, evolution);
		}

		public override void ResetAnimation()
		{
			animator.Play(animationName, -1, 0);
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
			if (animator == null || animator.runtimeAnimatorController == null)
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

#endif
	}
}
