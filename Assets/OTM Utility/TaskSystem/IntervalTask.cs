#if UNITY_2023_1_OR_NEWER
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace com.otm.utility
{
    public class IntervalTask : BaseTask
	{
#region private variables

		private float interval;
		private bool useScaleTime;
		private int invervalInMiliseconds;

#endregion

#region public methods

		public void Start(Func<bool> condition,float interval,bool scaleWait=false, params CancellationToken[] otherTokens)
		{
			this.interval = interval;
			useScaleTime=scaleWait;
			invervalInMiliseconds = (int)(interval * 1000f);
			base.Start(condition, otherTokens);
		}

#endregion

#region overided methods
		protected override async Task DoAsyncOpration()
		{
			while (!cancellationTokenSource.IsCancellationRequested && condition.Invoke())
			{
				if (useScaleTime)
					await Awaitable.WaitForSecondsAsync(interval,linkedCancellationTokenSource.Token);
				else
					await Task.Delay(invervalInMiliseconds, linkedCancellationTokenSource.Token);
				OnTaskUpdate.Invoke();
			}
			Stop();
			OnTaskComplete?.Invoke();
		}
#endregion
	}
}
#endif