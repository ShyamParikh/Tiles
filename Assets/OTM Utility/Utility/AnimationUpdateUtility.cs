using UnityEngine;

namespace com.otm.utility
{
	public static class AnimationUpdateUtility
	{
		#region public static methods

		public static void UpdateAnimation(this Animator animator, string stateName, float normalizeValue)
		{
#if UNITY_EDITOR
			if (animator.runtimeAnimatorController == null)
				return;
			UnityEditor.Animations.AnimatorController animatorController = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
			AnimationClip[] animationClips = animatorController.animationClips;
			AnimationClip clip = null;
			foreach (var l in animatorController.layers)
			{
				foreach (var s in l.stateMachine.states)
				{
					if (s.state.name.Equals(stateName))
					{
						for (int i = 0; i < animationClips.Length; i++)
						{
							if (animationClips[i].name.Equals(s.state.motion.name))
							{
								clip = animationClips[i];
								break;
							}
						}
						break;
					}
				}
			}
			clip.SampleAnimation(animator.gameObject, clip.length * normalizeValue);
#endif
		}

		public static void UpdateAnimation(this AnimationClip clip, GameObject gameObject, float normalizeValue)
		{
#if UNITY_EDITOR
			if (gameObject == null || clip == null)
				return;
			clip.SampleAnimation(gameObject, clip.length * normalizeValue);
#endif
		}

		#endregion
	}
}
