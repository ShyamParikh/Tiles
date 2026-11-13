using com.otm.ui;
using com.otm.utility;

#if UNITY_SPINE
using Spine.Unity;
#endif
using UnityEngine;

namespace com.otm.tutorial
{
	public class TapHandView : BaseView
	{
#region private variables

		[SerializeField] private RectTransform mainRectTransform;
		#if UNITY_SPINE
		[SerializeField] private SkeletonGraphic handGraphic;
		[SerializeField] private AnimationReferenceAsset tapAnimation;
#endif

#endregion

#region overrided methods
		public override void ShowView()
		{
			#if UNITY_SPINE
			handGraphic.Skeleton.SetColor(Color.clear);
#endif
			base.ShowView();
		}

		protected override void OnViewShowDone()
		{
#if UNITY_SPINE
			handGraphic.AnimationState.SetAnimation(0, tapAnimation.Animation.Name, true);
			handGraphic.Skeleton.SetColor(Color.white);
#endif
			base.OnViewShowDone();
		}

		protected override void OnViewHideDone()
		{
			base.OnViewHideDone();
			Destroy(gameObject);
		}

#endregion

#region public methods

		public void ShowView(TapHandConfig tapHandConfig)
		{
			base.ShowView();
			mainRectTransform.SetAnchor(tapHandConfig.anchor);
			mainRectTransform.anchoredPosition = tapHandConfig.anchorPosition;
			mainRectTransform.eulerAngles = Vector3.forward * tapHandConfig.rotation;
		}

		public void ShowForEditor(TapHandConfig tapHandConfig)
		{
			ShowEditor();
			mainRectTransform.SetAnchor(tapHandConfig.anchor);
			mainRectTransform.anchoredPosition = tapHandConfig.anchorPosition;
			mainRectTransform.eulerAngles = Vector3.forward * tapHandConfig.rotation;
		}

		public void HideForEditor()
		{
			HideEditor();
		}

#endregion
	}

	public struct TapHandConfig
	{
		public TextAnchor anchor;
		public Vector2 anchorPosition;
		public float rotation;
	}
}
