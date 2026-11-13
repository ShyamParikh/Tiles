using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace com.otm.utility
{
	public abstract class BaseTask
	{
		#region public variables

		public ActionSystem OnTaskUpdate = new ActionSystem();
		public ActionSystem OnTaskComplete = new ActionSystem();

		#endregion

		#region propertices

		public bool IsTaskRunning => linkedCancellationTokenSource != null && !linkedCancellationTokenSource.IsCancellationRequested;

		#endregion

		#region protected variables

		protected Func<bool> condition;
		protected Task task;
		protected CancellationTokenSource cancellationTokenSource;
		protected CancellationTokenSource linkedCancellationTokenSource;
		protected CancellationToken[] cancellationTokens;

		#endregion


		#region virtual methods
		protected virtual void Start(Func<bool> condition, params CancellationToken[] otherTokens)
		{
			if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
			{
				Debug.LogError("Task already running");
				return;
			}
			cancellationTokens = otherTokens;
			this.condition = condition;
			StarOpration();
		}

		public virtual void Stop()
		{
			try
			{
				cancellationTokenSource.Cancel();
				cancellationTokenSource.Dispose();
			}
			catch (Exception)
			{

			}
		}

		#endregion

		#region portecte virtual methods
		protected virtual async void StarOpration()
		{
			if (condition == null)
			{
				Debug.LogError("Condition is null");
				return;
			}

			linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(GetCancellationTokens());
			try
			{
				task = DoAsyncOpration();
				await task;
			}
			catch (Exception)
			{
				if (!IsTaskRunning)
				{
					Debug.Log("Task Cancle");
				}
			}
		}

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		protected virtual async Task DoAsyncOpration()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
		{

		}

		protected virtual CancellationToken[] GetCancellationTokens()
		{
			if (cancellationTokenSource != null)
				cancellationTokenSource.Dispose();
			CancellationToken[] tokens = new CancellationToken[cancellationTokens.Length + 2];

			for (int i = 0; i < cancellationTokens.Length; i++)
			{
				tokens[i] = cancellationTokens[i];
			}
			cancellationTokenSource = new CancellationTokenSource();
			tokens[tokens.Length - 2] = cancellationTokenSource.Token;
			tokens[tokens.Length - 1] = Application.exitCancellationToken;
			return tokens;
		}

		#endregion
	}
}
