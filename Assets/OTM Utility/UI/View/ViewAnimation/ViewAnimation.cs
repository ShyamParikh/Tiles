using com.otm.animation;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.ui
{
	public class ViewAnimation : BaseUiAnimation
	{
		#region private variables

		[SerializeField] private AnimationGroup showAnimation = new AnimationGroup();
		[SerializeField] private AnimationGroup hideAnimation = new AnimationGroup();
		[SerializeField] private Dictionary<string, AnimationGroup> otherAnimation = new Dictionary<string, AnimationGroup>();

		#endregion

		#region unity callback
		private void OnDestroy()
		{
			StopAllAnimation();
		}
		#endregion

		#region override methods

		public override void Init()
		{
			base.Init();
			InitAnimations();
		}

		public override void ShowAnimation(Action onShow)
		{
			if (!gameObject.activeInHierarchy)
				return;
			base.ShowAnimation(onShow);
			PlayShowAnimation();
		}

		public override void HideAnimation(Action action)
		{
			if (!gameObject.activeInHierarchy)
				return;
			base.HideAnimation(action);
			PlayHideAnimation();
		}

		public override void StopHideAnimation()
		{
			hideAnimation.StopAnimation();
		}

		public override void StopShowAnimation()
		{
			showAnimation.StopAnimation();
		}

		public override void StopAllAnimation()
		{
			hideAnimation.StopAnimation();
			showAnimation.StopAnimation();
			foreach (var animation in otherAnimation)
				animation.Value.StopAnimation();
		}

		public override void PlayOtherAnimation(string animationKey)
		{
			base.PlayOtherAnimation(animationKey);
			if (otherAnimation.ContainsKey(animationKey))
				otherAnimation[animationKey].PlayAnimation();
		}

		public override void StopOtherAnimation(string animationKey)
		{
			base.StopOtherAnimation(animationKey);
			if (otherAnimation.ContainsKey(animationKey))
				otherAnimation[animationKey].StopAnimation();
		}

		public override bool IsAnimationRunning(string animationKey)
		{
			if (otherAnimation.ContainsKey(animationKey))
				return otherAnimation[animationKey].IsAnimationRunning;
			return false;
		}

		public override void SetShowAnimationValue()
		{
			showAnimation.SetToEndValue();
		}
		public override void SetHideAnimationValue()
		{
			hideAnimation.SetToEndValue();
		}

		#endregion

		#region private methods

		private void InitAnimations()
		{
			showAnimation.Init(this);
			hideAnimation.Init(this);
			foreach (var animation in otherAnimation)
				animation.Value.Init(this);
		}

		private void PlayShowAnimation()
		{
			showAnimation.PlayAnimation(OnShowAnimationComplete);
		}

		private void PlayHideAnimation()
		{
			hideAnimation.PlayAnimation(OnHideAnimationComplete);
		}

		#endregion
	}
}
