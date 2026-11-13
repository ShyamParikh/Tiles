using System.Security;
using UnityEngine;

namespace com.otm.tutorial
{

    public class SimpleDialogThing : TutorialThings
    {
        #region private variables

        [SerializeField] private DialogConfig dialog;

        private TutorialSimpleDialogView tutorialDialogView;

        #endregion

        #region overrided methods
        public override void Active()
        {
            tutorialDialogView = TutorialManager.Instance.GetView<TutorialSimpleDialogView>();
            tutorialDialogView.ShowView(dialog);
        }

        public override void DeActive()
        {
            tutorialDialogView.HideView();
        }

        public override void Stop()
        {
            tutorialDialogView.HideView();
        }

        #endregion

#if UNITY_EDITOR
        private TutorialSimpleDialogView dialogViewEditor;

        public override void ShowStepsInEditor()
        {
            base.ShowStepsInEditor();
            dialogViewEditor = manager.GetViewEditor<TutorialSimpleDialogView>();
            dialogViewEditor?.ShowForEditor(dialog);
        }

        public override void OnUpdateThing()
        {
            dialogViewEditor?.ShowForEditor(dialog);
        }

        public override void HideStepsInEditor()
        {
            dialogViewEditor?.HideForEditor();
            base.HideStepsInEditor();
        }

#endif
    }

}