using com.otm.ui;
using UnityEngine;

namespace com.otm.tutorial
{
	public class TutorialSpriteMaskView : BaseView
	{
		#region private variables

		[SerializeField] private TutorialSpriteMask tutorialSpriteMask;
		private TutorialSpriteMask[] masks = new TutorialSpriteMask[0];

		#endregion

		#region overrided methods

		public override void HideView()
		{
			for (int i = 0; i < masks.Length; i++)
			{
				masks[i].Hide();
			}
			base.HideView();

		}

		protected override void OnViewHideDone()
		{
			base.OnViewHideDone();
			Destroy(gameObject);
		}

		public void ShowView(TutorialSpriteMaskConfig[] maskConfigs)
		{
			base.ShowView();
			CreateMasks(maskConfigs);
		}

		public void ShowView(TutorialFollowSpriteMaskConfig maskConfig)
		{
			base.ShowView();
			masks = new TutorialSpriteMask[1];
			TutorialSpriteMask mask = Instantiate(tutorialSpriteMask, transform);
			mask.Show(maskConfig);
			masks[0] = mask;
		}

		public void ShowForEditor(TutorialSpriteMaskConfig[] maskConfigs)
		{
			ShowEditor();
			masks = GetComponentsInChildren<TutorialSpriteMask>();
			if (masks.Length != maskConfigs.Length)
			{
				for (int i = 0; i < masks.Length; i++)
				{
					Destroy(masks[i].gameObject);
				}
				CreateMasks(maskConfigs);
			}
			for (int i = 0; i < maskConfigs.Length; i++)
			{
				masks[i].Show(maskConfigs[i]);
			}
		}

		public void HideForEditor()
		{
			masks = GetComponentsInChildren<TutorialSpriteMask>();
			for (int i = 0; i < masks.Length; i++)
			{
				DestroyImmediate(masks[i].gameObject);
			}
			masks = new TutorialSpriteMask[0];
		}

		#endregion

		#region private methods

		private void CreateMasks(TutorialSpriteMaskConfig[] maskConfigs)
		{
			masks = new TutorialSpriteMask[maskConfigs.Length];
			for (int i = 0; i < maskConfigs.Length; i++)
			{
				TutorialSpriteMask mask = Instantiate(tutorialSpriteMask, transform);
				mask.Show(maskConfigs[i]);
				masks[i] = mask;
			}
		}

		#endregion
	}

	public enum TutorialSpriteMaskType
	{
		Round,
		Suqure,
	}

	public struct TutorialSpriteMaskConfig
	{
		public TutorialSpriteMaskType tutorialSpriteMaskType;
		public Vector2 position;
		public Vector2 scale;
		public float rotation;
	}

	public struct TutorialFollowSpriteMaskConfig
	{
		public TutorialSpriteMaskType tutorialSpriteMaskType;
		public Vector3 offeset;
		public Transform target;
		public Vector2 scale;
	}
}
