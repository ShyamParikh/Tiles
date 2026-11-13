using com.otm.ui;
using TMPro;
using UnityEngine;

namespace com.otm.tutorial
{
	public class TutorialWorldSimpleDialogView : BaseView
	{
		#region private variables
		[SerializeField] private TMP_Text leftDialogText;
		[SerializeField] private GameObject leftDialogObject;
		[SerializeField] private TMP_Text rightDialogText;
		[SerializeField] private GameObject rightDialogObject;
		#endregion

		#region public methods

		public void ShowView(WorldSimpleDialogConfig config)
		{
			if (config.isRight)
				SetRightDialog(config);
			else
				SetLeftDialog(config);
			base.ShowView();
		}

		protected override void OnViewHideDone()
		{
			base.OnViewHideDone();
			Destroy(gameObject);
		}

		public void ShowForEditor(WorldSimpleDialogConfig config)
		{
			ShowEditor();
			if (config.isRight)
				SetRightDialog(config);
			else
				SetLeftDialog(config);
		}

		public void HideForEditor()
		{
			HideEditor();
		}

		#endregion

		#region private methods

		private void SetLeftDialog(WorldSimpleDialogConfig config)
		{
			leftDialogObject.SetActive(true);
			rightDialogObject.SetActive(false);
			transform.position = config.position;
			leftDialogText.text = config.dialog;
		}

		private void SetRightDialog(WorldSimpleDialogConfig config)
		{
			rightDialogObject.SetActive(true);
			leftDialogObject.SetActive(false);
			transform.position = config.position;
			rightDialogText.text = config.dialog;
		}

		#endregion
	}

	public struct WorldSimpleDialogConfig
	{
		public Vector3 position;
		public bool isRight;
		[Multiline] public string dialog;
	}
}
