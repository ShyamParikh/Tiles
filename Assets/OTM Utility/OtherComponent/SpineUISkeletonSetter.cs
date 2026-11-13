#if UNITY_SPINE
using Spine.Unity;
using UnityEngine;

namespace com.otm.utility
{
	public class SpineUISkeletonSetter : MonoBehaviour
	{
#region public methods

		public SkeletonGraphic SetSkeleton(SkeletonDataAsset skeletonDataAsset, string animationName, string skinName = "default", Vector2? anchorPosition = null)
		{

			RectTransform rectTransform = GetComponent<RectTransform>();
			if (anchorPosition != null)
				rectTransform.anchoredPosition = anchorPosition.Value;
			SkeletonGraphic skeletonGraphic = rectTransform.GetComponent<SkeletonGraphic>();
			if (skeletonGraphic == null)
				skeletonGraphic = rectTransform.gameObject.AddComponent<SkeletonGraphic>();
			skeletonGraphic.skeletonDataAsset = skeletonDataAsset;
			skeletonGraphic.material = Resources.Load<Material>("SpinMaterial/SkeletionUIMatrial");
			skeletonGraphic.MeshGenerator.settings.canvasGroupCompatible = true;
			skeletonGraphic.MeshGenerator.settings.pmaVertexColors = false;
			skeletonGraphic.allowMultipleCanvasRenderers = true;
			skeletonGraphic.layoutScaleMode = SkeletonGraphic.LayoutMode.HeightControlsWidth;
			skeletonGraphic.Initialize(false);
			skeletonGraphic.Skeleton.SetSkin(skinName);
			skeletonGraphic.Skeleton.SetSlotsToSetupPose();
			skeletonGraphic.AnimationState.SetAnimation(0, animationName, true);
			return skeletonGraphic;
		}

#endregion
	}
}
#endif