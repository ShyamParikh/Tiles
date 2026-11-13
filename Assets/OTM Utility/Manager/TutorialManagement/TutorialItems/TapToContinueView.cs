using com.otm.ui;
using UnityEngine;

namespace com.otm.tutorial
{
	public class TapToContinueView : BaseView
	{
		#region private variables

		[SerializeField] private GameObject backgroundObject;
		[SerializeField] private GameObject tapToContinueTextObject;

		#endregion

		#region public methods

		public void ShowView(bool showBackground, bool showTapToContinue)
		{
			backgroundObject.SetActive(showBackground);
			tapToContinueTextObject.SetActive(showTapToContinue);
			base.ShowView();
		}
		protected override void OnViewHideDone()
		{
			base.OnViewHideDone();
			Destroy(gameObject);
		}

		public void OnButton()
		{
			if (TutorialManager.Instance.CurrentTutorial != null)
				TutorialManager.Instance.CurrentTutorial.MoveToNextStep();
			HideView();
		}

		public void ShowForEditor(bool showBackground, bool showTapToContinue)
		{
			ShowEditor();
			backgroundObject.SetActive(showBackground);
			tapToContinueTextObject.SetActive(showTapToContinue);
		}

		public void HideForEditor()
		{
			HideEditor();
		}

		#endregion
	}
}
