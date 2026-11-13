#if UNITY_SPINE
using Spine;
using Spine.Unity;
using UnityEngine;

namespace com.otm.animation
{
	public class SpineSingleAnimationBlening : MonoBehaviour
	{
		#region private variables
		[SerializeField] private SkeletonAnimation skeletonAnimation;
		[SerializeField] private AnimationReferenceAsset animationReferenceAsset;
		[SerializeField, Range(0, 1)] private float range;
		private TrackEntry trask;
		#endregion

		#region unity callback

		private void OnEnable()
		{
			if (animationReferenceAsset != null)
				skeletonAnimation.AnimationName = animationReferenceAsset.name;
			trask = skeletonAnimation.AnimationState.Tracks.Items[0];
			skeletonAnimation.BeforeApply += ApplyBlending;
		}

		private void OnDisable()
		{
			skeletonAnimation.BeforeApply -= ApplyBlending;
		}

		#endregion

		#region private methods
		private void ApplyBlending(ISkeletonAnimation iSkeletonAnimation)
		{
			trask.TrackTime = trask.Animation.Duration * range;
		}
		#endregion

		#region public methods

		public void SetBlendValue(float value)
		{
			range = Mathf.Clamp(value, 0f, 1f);
		}

		#endregion
	}
}
#endif