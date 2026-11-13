using com.otm.ui;
using com.otm.utility;
using TMPro;
using UnityEngine;

namespace com.otm.tutorial
{
	public class TutorialMascotDialogView : BaseView
	{
		#region private variables

		[SerializeField] private GameObject leftDailogObject;
		[SerializeField] private TMP_Text leftDailogText;
		[SerializeField] private RectTransform leftDialogRectTransform;
		[SerializeField] private RectTransform leftMascostRectTransform;

		[SerializeField] private GameObject rightDailogObject;
		[SerializeField] private TMP_Text rightDailogText;
		[SerializeField] private RectTransform rightDialogRectTransform;
		[SerializeField] private RectTransform rightMascostRectTransform;

		#endregion

		#region public methods

		public void ShowView(MascotDialogConfig mascotDialogConfig)
		{
			if (mascotDialogConfig.isRight)
				SetRightDailog(mascotDialogConfig);
			else
				SetLeftDailog(mascotDialogConfig);
			base.ShowView();
		}

		protected override void OnViewHideDone()
		{
			base.OnViewHideDone();
			Destroy(gameObject);
		}

		public void ShowForEditor(MascotDialogConfig mascotDialogConfig)
		{
			ShowEditor();
			if (mascotDialogConfig.isRight)
				SetRightDailog(mascotDialogConfig);
			else
				SetLeftDailog(mascotDialogConfig);
		}

		public void HideForEditor()
		{
			HideEditor();
		}

		#endregion

		#region private methods

		private void SetLeftDailog(MascotDialogConfig mascotDialogConfig)
		{
			leftDailogObject.SetActive(true);
			rightDailogObject.SetActive(false);
			leftDailogText.text = mascotDialogConfig.dialog;
			leftDialogRectTransform.SetAnchor(mascotDialogConfig.anchor);
			leftDialogRectTransform.anchoredPosition = mascotDialogConfig.anchorPosition;
			leftMascostRectTransform.anchoredPosition = new Vector2(mascotDialogConfig.mascotPosition, leftMascostRectTransform.anchoredPosition.y);
		}

		private void SetRightDailog(MascotDialogConfig mascotDialogConfig)
		{
			rightDailogObject.SetActive(true);
			leftDailogObject.SetActive(false);
			rightDailogText.text = mascotDialogConfig.dialog;
			rightDialogRectTransform.SetAnchor(mascotDialogConfig.anchor);
			rightDialogRectTransform.anchoredPosition = mascotDialogConfig.anchorPosition;
			rightMascostRectTransform.anchoredPosition = new Vector2(mascotDialogConfig.mascotPosition, leftMascostRectTransform.anchoredPosition.y);
		}

		#endregion
	}

	public struct MascotDialogConfig
	{
		public TextAnchor anchor;
		public Vector2 anchorPosition;
		public bool isRight;
		public float mascotPosition;
		[Multiline] public string dialog;
	}
}
