#if UNITY_2023_1_OR_NEWER
using System;
using System.Threading;
using System.Threading.Tasks;


namespace com.otm.utility
{
	public class ValidationTask : BaseTask
	{
		#region private variables

		private int wait;

		#endregion

		#region public methods

		public void Start(Func<bool> condition, float wait = 0.1f, params CancellationToken[] otherTokens)
		{
			this.wait = (int)(wait * 1000f);
			base.Start(condition, otherTokens);
		}

		#endregion

		#region overided methods
		protected override async Task DoAsyncOpration()
		{
			while (!cancellationTokenSource.IsCancellationRequested && condition.Invoke())
			{
				await Task.Delay(wait, linkedCancellationTokenSource.Token);
				OnTaskUpdate.Invoke();
			}
			Stop();
			OnTaskComplete?.Invoke();
		}

		#endregion
	}
}
#endif