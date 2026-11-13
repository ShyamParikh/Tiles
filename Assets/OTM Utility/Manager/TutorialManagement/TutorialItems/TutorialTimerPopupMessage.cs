using com.otm.ui;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace com.otm.tutorial
{
	public class TutorialTimerPopupMessage : BaseView
	{
		#region private variables

		[SerializeField] private TMP_Text messageText;
		[SerializeField] private TMP_Text timerText;

		private Coroutine coroutine;
		private Action onHide;

		#endregion

		#region overrided methods

		protected override void OnViewHideDone()
		{
			if (coroutine != null)
				StopCoroutine(coroutine);
			base.OnViewHideDone();
			Destroy(gameObject);
		}

		public override void HideView()
		{
			onHide?.Invoke();
			onHide = null;
			base.HideView();
		}

		#endregion
		#region public methods

		public void ShowView(Vector3 postion, string message, int time, Action onHide)
		{
			this.onHide = onHide;
			transform.position = postion;
			messageText.text = message;
			base.ShowView();
			coroutine = StartCoroutine(DoTimerCoroutine(time));
		}

		public void OnCloseButton()
		{
			HideView();
		}

		#endregion

		#region coroutine

		IEnumerator DoTimerCoroutine(int time)
		{
			WaitForSeconds waitForSeconds = new WaitForSeconds(1);
			while (time > 0)
			{
				timerText.text = time + "S auto trun off";
				yield return waitForSeconds;
				time--;
			}
			HideView();
		}

		#endregion
	}
}
