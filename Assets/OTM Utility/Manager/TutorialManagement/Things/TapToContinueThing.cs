using UnityEngine;

namespace com.otm.tutorial
{
	public class TapToContinueThing : TutorialThings
	{
		#region private variables

		[SerializeField] private bool showBackground;
		[SerializeField] private bool showTapToContinueText;
		private TapToContinueView tapToContinueView;

		#endregion

		#region overrided methods
		public override void Active()
		{
			tapToContinueView = TutorialManager.Instance.GetView<TapToContinueView>();
			tapToContinueView.ShowView(showBackground, showTapToContinueText);
		}

		public override void DeActive()
		{
			tapToContinueView.HideView();
		}

		public override void Stop()
		{
			tapToContinueView.HideView();
		}

		#endregion

#if UNITY_EDITOR
		private TapToContinueView tapToContinueViewEditor;

		public override void ShowStepsInEditor()
		{
			base.ShowStepsInEditor();
			tapToContinueViewEditor = manager.GetViewEditor<TapToContinueView>();
			tapToContinueViewEditor?.ShowForEditor(showBackground, showTapToContinueText);
		}

		public override void OnUpdateThing()
		{
			tapToContinueViewEditor?.ShowForEditor(showBackground, showTapToContinueText);
		}

		public override void HideStepsInEditor()
		{
			tapToContinueViewEditor?.HideForEditor();
			base.HideStepsInEditor();
		}

#endif
	}
}
