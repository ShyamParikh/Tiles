using com.otm.ui;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace com.otm.tutorial
{
	public class TutorialStaticMessageView : BaseView
	{
		#region private variables

		[SerializeField] private TMP_Text messgaeText;

		#endregion
		#region overrided methods

		protected override void OnViewHideDone()
		{
			base.OnViewHideDone();
			Destroy(gameObject);
		}

		#endregion
		#region public methods

		public void ShowView(string message)
		{
			messgaeText.text = message;
			base.ShowView();
		}
		#endregion
	}
}
