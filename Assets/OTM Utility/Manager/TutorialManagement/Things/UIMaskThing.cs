using UnityEngine;

namespace com.otm.tutorial
{
	public class UIMaskThing : TutorialThings
	{
		#region private variables

		[SerializeField] private TutorialUIMaskConfig[] masking = new TutorialUIMaskConfig[0];
		private TutorialUIMaskView tutorialUIMaskView;

		#endregion

		#region overrided methods
		public override void Active()
		{
			tutorialUIMaskView = TutorialManager.Instance.GetView<TutorialUIMaskView>();
			tutorialUIMaskView.ShowView(masking);
		}

		public override void DeActive()
		{
			tutorialUIMaskView.HideView();
		}

		public override void Stop()
		{
			tutorialUIMaskView.HideView();
		}

#if UNITY_EDITOR
		private TutorialUIMaskView tutorialUIMaskViewEditor;
		public override void ShowStepsInEditor()
		{
			base.ShowStepsInEditor();
			tutorialUIMaskViewEditor = manager.GetViewEditor<TutorialUIMaskView>();
			tutorialUIMaskViewEditor?.ShowForEditor(masking);
		}

		public override void OnUpdateThing()
		{
			tutorialUIMaskViewEditor?.ShowForEditor(masking);
		}

		public override void HideStepsInEditor()
		{
			tutorialUIMaskViewEditor?.HideForEditor();
			base.HideStepsInEditor();
		}

#endif
		#endregion
	}
}
