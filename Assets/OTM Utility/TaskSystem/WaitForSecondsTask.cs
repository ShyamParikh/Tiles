#if UNITY_2023_1_OR_NEWER
using System;
using System.Threading;
using UnityEngine;
using System.Threading.Tasks;

namespace com.otm.utility
{
	public class WaitForSecondsTask : BaseTask
	{
		#region public variables

		public ActionSystem<float> OnTimerUpdate = new ActionSystem<float>();
		public ActionSystem OnTimerComplete = new ActionSystem();

		#endregion

		#region private methods

		private float wait;
		private float inverval;

		#endregion

		#region public methods

		public void Start(float wait, float inverval, params CancellationToken[] otherTokens)
		{
			this.wait = wait;
			this.inverval = inverval;
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
			while (!cancellationTokenSource.IsCancellationRequested && condition.Invoke())
			{
				await Awaitable.WaitForSecondsAsync(inverval, linkedCancellationTokenSource.Token);
				wait -= inverval;
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