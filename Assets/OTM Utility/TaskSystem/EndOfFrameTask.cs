#if UNITY_2023_1_OR_NEWER
using System.Threading;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace com.otm.utility
{
	public class EndOfFrameTask : BaseTask
	{
		#region public methods

		public new void Start(Func<bool> condition, params CancellationToken[] otherTokens)
		{
			base.Start(condition, otherTokens);
		}

		#endregion

		#region overided methods
		protected override async Task DoAsyncOpration()
		{
			while (!cancellationTokenSource.IsCancellationRequested && condition.Invoke())
			{
				await Awaitable.EndOfFrameAsync(linkedCancellationTokenSource.Token);
				OnTaskUpdate.Invoke();
			}
			Stop();
			OnTaskComplete?.Invoke();
		}

		#endregion
	}
}
#endif