using com.otm.manager;
using UnityEngine;

namespace com.otm.tutorial
{
	public class MoveToNextStepThing : TutorialThings
	{
		#region private variables

		[SerializeField] private float wait = 0;
		[SerializeField] private bool useUnscaleTime = true;
		private Coroutine timeCoroutine;

		#endregion

		#region overrided methods
		public override void Active()
		{
			if (wait <= 0)
			{
				mono.MoveToNextStep();
				return;
			}
			if (useUnscaleTime)
				DoWaitUnScaleTime();
			else
				DoWaitScaleTime();
		}

		public override void DeActive()
		{

		}

		public override void Stop()
		{
			if (timeCoroutine != null)
				CoroutineRunner.Instance.StopCoroutine(timeCoroutine);
		}

		#endregion

		#region private methods

		private void DoWaitScaleTime()
		{
			timeCoroutine = CoroutineRunner.Instance.WaitForTime(wait, mono.MoveToNextStep);
		}

		private void DoWaitUnScaleTime()
		{
			timeCoroutine = CoroutineRunner.Instance.WaitForRealTime(wait, mono.MoveToNextStep);
		}

		#endregion
	}
}