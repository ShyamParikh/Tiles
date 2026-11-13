#if UNITY_2023_1_OR_NEWER
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace com.otm.utility
{
	public class UnScaleWaitForSecondsTask : BaseTask
	{
		#region public variables

		public ActionSystem<float> OnTimerUpdate = new ActionSystem<float>();
		public ActionSystem OnTimerComplete = new ActionSystem();

		#endregion
		#region private methods

		private float wait;
		private int interval;
		private float mainInterval;

		#endregion

		#region public methods

		public void Start(float wait, float interval, params CancellationToken[] otherTokens)
		{
			this.wait = wait;
			this.interval = (int)(interval * 1000f);
			mainInterval = interval;
			base.Start(IsValidWait, otherTokens);
		}

		#endregion

		#region private method

		private void OnUpdate()
		{
			OnTimerUpdate.Invoke(wait);
		}

		private void OnComplete()
		{
			OnTimerComplete.Invoke();
		}

		private bool IsValidWait()
		{
			return wait > 0;
		}

		#endregion

		#region overided methods
		protected override async Task DoAsyncOpration()
		{
			while (!linkedCancellationTokenSource.IsCancellationRequested && condition.Invoke())
			{
				await Task.Delay(interval, linkedCancellationTokenSource.Token);
				wait -= mainInterval;
				OnTaskUpdate.Invoke();
				OnUpdate();
			}
			Stop();
			OnComplete();
			OnTaskComplete?.Invoke();
		}
		#endregion
	}
}
#endif