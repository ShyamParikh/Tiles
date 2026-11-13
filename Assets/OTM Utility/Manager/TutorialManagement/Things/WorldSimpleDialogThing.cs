using UnityEngine;

namespace com.otm.tutorial
{
	public class WorldSimpleDialogThing : TutorialThings
	{
		#region private variables
		[SerializeField] private WorldSimpleDialogConfig dialogConfig;
		private TutorialWorldSimpleDialogView simpleDialogView;
		#endregion
		#region overrided mehods
		public override void Active()
		{
			simpleDialogView = TutorialManager.Instance.GetView<TutorialWorldSimpleDialogView>();
			simpleDialogView.ShowView(dialogConfig);
		}

		public override void DeActive()
		{
			simpleDialogView.HideView();
		}

		public override void Stop()
		{
			simpleDialogView.HideView();
		}

		#endregion

#if UNITY_EDITOR
		private TutorialWorldSimpleDialogView simpleDialogViewEditor;

		public override void ShowStepsInEditor()
		{
			base.ShowStepsInEditor();
			simpleDialogViewEditor = manager.GetViewEditor<TutorialWorldSimpleDialogView>();
			simpleDialogViewEditor?.ShowForEditor(dialogConfig);
		}

		public override void OnUpdateThing()
		{
			simpleDialogViewEditor?.ShowForEditor(dialogConfig);
		}

		public override void HideStepsInEditor()
		{
			simpleDialogViewEditor?.HideForEditor();
			base.HideStepsInEditor();
		}

#endif
	}
}
