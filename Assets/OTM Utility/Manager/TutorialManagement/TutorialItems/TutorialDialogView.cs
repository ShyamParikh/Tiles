using com.otm.ui;
using com.otm.utility;
using TMPro;
using UnityEngine;

namespace com.otm.tutorial
{
	public class TutorialDialogView : BaseView
	{
		#region private variables

		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private TMP_Text leftDialogText;
		[SerializeField] private TMP_Text rightDialogText;
		[SerializeField] private GameObject leftDailog;
		[SerializeField] private GameObject rightDailog;


		#endregion

		#region public methods

		public void ShowView(DialogConfig dialogConfig)
		{
			rectTransform.SetAnchor(dialogConfig.anchor);
			rectTransform.anchoredPosition = dialogConfig.anchorPosition;
			leftDialogText.text = dialogConfig.dialog;
			rightDialogText.text = dialogConfig.dialog;
			leftDailog.SetActive(!dialogConfig.isRight);
			rightDailog.SetActive(dialogConfig.isRight);
			base.ShowView();
		}

		protected override void OnViewHideDone()
		{
			base.OnViewHideDone();
			Destroy(gameObject);
		}

		public void ShowForEditor(DialogConfig dialogConfig)
		{
			ShowEditor();
			rectTransform.SetAnchor(dialogConfig.anchor);
			rectTransform.anchoredPosition = dialogConfig.anchorPosition;
			leftDialogText.text = dialogConfig.dialog;
			rightDialogText.text = dialogConfig.dialog;
			leftDailog.SetActive(!dialogConfig.isRight);
			rightDailog.SetActive(dialogConfig.isRight);
		}

		public void HideForEditor()
		{
			leftDailog.SetActive(false);
			rightDailog.SetActive(false);
			HideEditor();
		}

		#endregion
	}

	public struct DialogConfig
	{
		public TextAnchor anchor;
		public Vector2 anchorPosition;
		public bool isRight;
		[Multiline] public string dialog;
	}
}
