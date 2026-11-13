using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace com.otm.animation
{
	public class MultiAnimationGroupComponent : SerializedMonoBehaviour
	{
		#region private variables

		[SerializeField] private Dictionary<string, AnimationGroup> otherAnimation = new Dictionary<string, AnimationGroup>();

		#endregion

		#region unity callback
		private void OnDestroy()
		{
			StopAllAnimation();
		}

		private void OnDisable()
		{
			StopAllAnimation();
		}

		#endregion
		#region override methods

		[Button]
		public void Init()
		{
			InitAnimations();
		}

		public void StopAllAnimation()
		{
			foreach (var animation in otherAnimation)
				animation.Value.StopAnimation();
		}

		public void PlayAnimation(string animationKey)
		{
			if (otherAnimation.ContainsKey(animationKey))
				otherAnimation[animationKey].PlayAnimation();
		}

		public void PlayAnimation(string animationKey, Action onAnimationComplete)
		{
			if (otherAnimation.ContainsKey(animationKey))
				otherAnimation[animationKey].PlayAnimation(onAnimationComplete);
			else
				onAnimationComplete?.Invoke();
		}

		public void StopAnimation(string animationKey)
		{
			if (otherAnimation.ContainsKey(animationKey))
				otherAnimation[animationKey].StopAnimation();
		}

		public AnimationGroup GetAnimation(string animationKey)
		{
			if (otherAnimation.ContainsKey(animationKey))
				return otherAnimation[animationKey];
			return null;

		}

		public bool IsAnimationRunning(string animationKey)
		{
			if (otherAnimation.ContainsKey(animationKey))
				return otherAnimation[animationKey].IsAnimationRunning;
			return false;
		}

		#endregion


		#region private methods

		private void InitAnimations()
		{
			foreach (var animation in otherAnimation)
				animation.Value.Init(this);
		}

		#endregion
	}
}
