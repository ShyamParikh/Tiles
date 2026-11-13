#if UNITY_SPINE
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.animation
{
	public class UISpineAnimation : BaseAnimation
	{
#region private variables

		[SerializeField] private SkeletonGraphic skeletonGraphic;
		[SerializeField] private AnimationReferenceAsset animationAsset;
		private TrackEntry track;

#endregion

#region overrided methods
		protected override void OnAnimationStart()
		{
			base.OnAnimationStart();
			track = skeletonGraphic.AnimationState.SetAnimation(0, animationAsset, false);
			//track = skeletonGraphic.AnimationState.Tracks.Items[0];
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
