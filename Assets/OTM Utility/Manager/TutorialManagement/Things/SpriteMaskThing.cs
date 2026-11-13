using UnityEngine;

namespace com.otm.tutorial
{
	public class SpriteMaskThing : TutorialThings
	{
		#region private variables

		[SerializeField] private TutorialSpriteMaskConfig[] masking = new TutorialSpriteMaskConfig[0];
		private TutorialSpriteMaskView tutorialSpriteMaskView;

		#endregion

		#region overrided methods
		public override void Active()
		{
			tutorialSpriteMaskView = TutorialManager.Instance.GetView<TutorialSpriteMaskView>();
			tutorialSpriteMaskView.ShowView(masking);
		}

		public override void DeActive()
		{
			tutorialSpriteMaskView.HideView();
		}

		public override void Stop()
		{
			tutorialSpriteMaskView.HideView();
		}

#if UNITY_EDITOR

		private TutorialSpriteMaskView tutorialSpriteMaskViewEditor;

		public override void ShowStepsInEditor()
		{
			base.ShowStepsInEditor();
			tutorialSpriteMaskViewEditor = manager.GetViewEditor<TutorialSpriteMaskView>();
			tutorialSpriteMaskViewEditor?.ShowForEditor(masking);
		}

		public override void OnUpdateThing()
		{
			tutorialSpriteMaskViewEditor?.ShowForEditor(masking);
		}

		public override void HideStepsInEditor()
		{
			tutorialSpriteMaskViewEditor?.HideForEditor();
			base.HideStepsInEditor();
		}

#endif
		#endregion
	}
}
