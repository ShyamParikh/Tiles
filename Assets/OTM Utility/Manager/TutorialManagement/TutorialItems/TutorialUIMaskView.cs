using com.otm.ui;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace com.otm.tutorial
{
	public class TutorialUIMaskView : BaseView
	{
		#region private variables

		[SerializeField] private TutorialUIMask tutorialUIMask;
		private List<TutorialUIMask> masks = new List<TutorialUIMask>();

		#endregion

		#region overrided methods

		protected override void OnViewHideDone()
		{
			base.OnViewHideDone();
			Destroy(gameObject);
		}

		public override void HideView()
		{
			for (int i = 0; i < masks.Count; i++)
			{
				masks[i].Hide();
			}
			base.HideView();
		}

		#endregion

		#region public methods

		public void ShowView(TutorialUIMaskConfig[] tutorialUIMaskConfigs)
		{
			base.ShowView();
			CreateMasks(tutorialUIMaskConfigs);
		}

		public void ShowForEditor(TutorialUIMaskConfig[] tutorialUIMaskConfigs)
		{
			ShowEditor();
			masks = GetComponentsInChildren<TutorialUIMask>().ToList();
			if (masks.Count != tutorialUIMaskConfigs.Length)
			{
				CreateMasks(tutorialUIMaskConfigs);
				return;
			}
			for (int i = 0; i < masks.Count; i++)
			{
				masks[i].Show(tutorialUIMaskConfigs[i]);
			}
		}

		public void HideForEditor()
		{
			for (int i = 0; i < masks.Count; i++)
			{
				DestroyImmediate(masks[i].gameObject);
			}
			masks.Clear();
			HideEditor();
		}

		#endregion

		#region private methods

		private void CreateMasks(TutorialUIMaskConfig[] tutorialUIMaskConfigs)
		{
			for (int i = 0; i < tutorialUIMaskConfigs.Length; i++)
			{
				TutorialUIMask mask = Instantiate(tutorialUIMask, transform);
				mask.Show(tutorialUIMaskConfigs[i]);
				masks.Add(mask);
			}
		}

		#endregion
	}

	public enum TutorialUIMaskType
	{
		Round,
		Square,
		RoundSquare
	}

	public struct TutorialUIMaskConfig
	{
		public TutorialUIMaskType tutorialUIMaskType;
		public TextAnchor anchor;
		public Vector2 anchorPosition;
		public Vector2 size;
		[ShowIf("$tutorialUIMaskType", TutorialUIMaskType.RoundSquare)] public float ppu;
	}
}
