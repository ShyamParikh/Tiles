#if UNITY_SPINE
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

namespace com.otm.animation
{
	public class SpineAnimation : BaseAnimation
	{
#region private variables

		[SerializeField] private SkeletonAnimation skeletonAnimation;
		[SerializeField] private AnimationReferenceAsset animationAsset;
		private TrackEntry track;

#endregion

#region overrided methods
		protected override void OnAnimationStart()
		{
			base.OnAnimationStart();
			skeletonAnimation.AnimationState.SetAnimation(0, animationAsset, false);
			track = skeletonAnimation.AnimationState.Tracks.Items[0];
		}
		public override void OnUpdate(float evolution)
		{
			track.TrackTime = duration * evolution;
		}

		public override void ResetAnimation()
		{
			base.ResetAnimation();
			track.TrackTime = 0;
		}

#endregion

#region private variables

		[Button]
		private void SetAnimationDuration()
		{
			if (animationAsset == null)
				return;
			duration = animationAsset.Animation.Duration;
		}

#endregion
	}
}
#endif
