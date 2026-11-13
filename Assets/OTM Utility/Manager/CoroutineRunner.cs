using System;
using System.Collections;
using UnityEngine;

namespace com.otm.manager
{
	public class CoroutineRunner : RunTimeManager<CoroutineRunner>
	{
		#region public methods

		public void ValidationTask(Func<bool> validation, Action onTaskComplete)
		{
			if (validation == null)
			{
				onTaskComplete?.Invoke();
				return;
			}
			StartCoroutine(ValidationCoroutine(validation, onTaskComplete));
		}

		public Coroutine IntervalTask(Func<bool> condition, float interval, bool scaleWait = false, Action onUpdate = null, Action onComplete = null)
		{
			return StartCoroutine(DoIntervalTask(condition, interval, scaleWait, onUpdate, onComplete));
		}

		public Coroutine WaitForRealTime(float timeInSeconds, Action onTaskComplete)
		{
			return StartCoroutine(WaitForRealTimeCoroutine(timeInSeconds, onTaskComplete));
		}

		public Coroutine WaitForTime(float timeInSeconds, Action onTaskComplete)
		{
			return StartCoroutine(WaitForTimeCoroutine(timeInSeconds, onTaskComplete));
		}

		#endregion

		#region coroutine

		IEnumerator DoIntervalTask(Func<bool> condition, float interval, bool scaleWait = false, Action onUpdate = null, Action onComplete = null)
		{
			WaitForSeconds waitForSeconds = new WaitForSeconds(interval);
			WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(interval);
			while (condition.Invoke())
			{
				if (scaleWait)
					yield return waitForSeconds;
				else
					yield return waitForSecondsRealtime;
				onUpdate?.Invoke();
			}
			onComplete?.Invoke();
		}

		IEnumerator WaitForRealTimeCoroutine(float timeInSeconds, Action onTaskComplete)
		{
			yield return new WaitForSecondsRealtime(timeInSeconds);
			onTaskComplete?.Invoke();
		}

		IEnumerator WaitForTimeCoroutine(float timeInSeconds, Action onTaskComplete)
		{
			yield return new WaitForSeconds(timeInSeconds);
			onTaskComplete?.Invoke();
		}

		IEnumerator ValidationCoroutine(Func<bool> validation, Action onTaskComplete)
		{
			while (validation.Invoke())
			{
				yield return null;
			}
			onTaskComplete?.Invoke();
		}

		#endregion
	}
}
