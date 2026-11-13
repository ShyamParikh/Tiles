using com.otm.manager;
using UnityEngine;

namespace com.otm.tutorial
{
	public class AutoTrunOffMessageThing : TutorialThings
	{
		#region private variables
		[SerializeField, Multiline] private string message;
		[SerializeField] private int turnOffTimeInSeconds;
		[SerializeField] private Vector3 position;
		private TutorialTimerPopupMessage tutorialTimerPopupMessage;
		#endregion
		#region overrided mehods
		public override void Active()
		{
			tutorialTimerPopupMessage = TutorialManager.Instance.GetView<TutorialTimerPopupMessage>();
			tutorialTimerPopupMessage.ShowView(position, message, turnOffTimeInSeconds, OnHide);
		}

		public override void DeActive()
		{
			tutorialTimerPopupMessage?.HideView();
		}

		public override void Stop()
		{
			tutorialTimerPopupMessage?.HideView();
		}

		#endregion

		#region private methods

		private void OnHide()
		{
			CoroutineRunner.Instance.WaitForTime(1f, mono.MoveToNextStep);
		}

		#endregion
	}
}
