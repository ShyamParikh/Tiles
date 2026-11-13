using com.otm.manager;
using com.otm.utility;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace com.otm.currency
{
	[CreateAssetMenu(menuName = "OTM/Currency/RefillCurrency")]
	[Serializable]
	public class RefillCurrency : Currency
	{
		#region protected variables

		[SerializeField] protected int unitTimeUpdateInSeconds;

		#endregion

		#region public variables

		public ActionSystem<TimeSpan> OnRefillTimeUpdate = new ActionSystem<TimeSpan>();
		public ActionSystem OnTimerStart = new ActionSystem();
		public ActionSystem OnTimerOver = new ActionSystem();

		#endregion

		#region private variables

		private DateTime refillTime;
#if UNITY_2023_OR_NEWER
		private IntervalTask intervalTask = new IntervalTask();
#else
		private Coroutine intervalTaskCoroutine;
#endif

		#endregion

		#region propertices

		public TimeSpan ReFillTime => refillTime - DateTime.UtcNow;

		public int DefaultValue => defaultValue;

		private DateTime lastUpdateTime;

		private DateTime LastUpdateTime
		{
			get
			{
				string time = PlayerPrefsHelper.GetString(key + "_Time", "");
				lastUpdateTime = (string.IsNullOrEmpty(time)) ? DateTime.UtcNow : JsonConvert.DeserializeObject<DateTime>(time);
				return lastUpdateTime;
			}
			set
			{
				lastUpdateTime = value;
				PlayerPrefsHelper.SetString(key + "_Time", JsonConvert.SerializeObject(lastUpdateTime));
			}
		}

		public int UnitTimeUpdateInSeconds => unitTimeUpdateInSeconds;

		#endregion

		#region overrided methods

		public override void Init()
		{
			base.Init();
			ApplicationManager.Instance.OnAppPause.RegisterEvent(OnApplicationPuase);
			ApplicationManager.Instance.OnAppResume.RegisterEvent(OnApplicationResume);
#if UNITY_2023_OR_NEWER
			intervalTask.OnTaskUpdate.RegisterEvent(OnTimerUpdate);
			intervalTask.OnTaskComplete.RegisterEvent(OnTimerComplete);
#endif
			AddRemainEnergy();
			OnCurrencyChange.RegisterEvent(CheckForRefill);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
#if UNITY_2023_OR_NEWER
			intervalTask.OnTaskUpdate.DeRegisterEvent(OnTimerUpdate);
			intervalTask.OnTaskComplete.DeRegisterEvent(OnTimerComplete);
#endif
			OnCurrencyChange.DeRegisterEvent(CheckForRefill);
			OnTimerStart.ClearEvents();
			OnTimerOver.ClearEvents();
			OnRefillTimeUpdate.ClearEvents();
		}

		#endregion

		#region private methods

		private void OnApplicationPuase()
		{
			StopTask();
		}

		private void OnApplicationResume()
		{
			AddRemainEnergy();
		}

		private void StopTask()
		{
#if UNITY_2023_OR_NEWER
		intervalTask.Stop();
#else
			if (intervalTaskCoroutine != null)
				CoroutineRunner.Instance.StopCoroutine(intervalTaskCoroutine);
			intervalTaskCoroutine = null;
#endif
		}

		private void AddRemainEnergy()
		{
			if (Value >= defaultValue)
				return;
			TimeSpan timeSpan = DateTime.UtcNow - LastUpdateTime;
			int energy = Mathf.FloorToInt((float)(timeSpan.TotalSeconds / unitTimeUpdateInSeconds));
			int remainSeconds = (int)(timeSpan.TotalSeconds - (energy * unitTimeUpdateInSeconds));
			if (energy > 0)
			{
				LastUpdateTime = DateTime.UtcNow.AddSeconds(-remainSeconds);
				int v = Mathf.Clamp((Value + energy), 0, defaultValue);
				SetValue(v);
			}

			if (Value >= defaultValue)
				return;
			int time = (remainSeconds > 0) ? (unitTimeUpdateInSeconds - remainSeconds) : unitTimeUpdateInSeconds;
			refillTime = DateTime.UtcNow.AddSeconds(time);
			StopTask();
			StartRefillTimer();
		}

		private void OnTimerUpdate()
		{
			OnRefillTimeUpdate.Invoke(refillTime - DateTime.UtcNow);
		}

		private void OnTimerComplete()
		{
			if (Value < defaultValue)
				Add(1);
			LastUpdateTime = DateTime.UtcNow;
			OnTimerOver.Invoke();
			if (Value >= defaultValue)
				return;
			refillTime = DateTime.UtcNow.AddSeconds(unitTimeUpdateInSeconds);
			StopTask();
			StartRefillTimer();
		}

		private bool IsValidRemainTime()
		{
			return refillTime > DateTime.UtcNow;
		}

		private void StartRefillTimer()
		{
			OnTimerStart.Invoke();
#if UNITY_2023_OR_NEWER
			intervalTask.Start(IsValidRemainTime, 1);
#else
			intervalTaskCoroutine = CoroutineRunner.Instance.IntervalTask(IsValidRemainTime, 1, false, OnTimerUpdate, OnTimerComplete);
#endif
		}

		private void CheckForRefill(int value)
		{
			if (Value >= defaultValue)
			{
				LastUpdateTime = DateTime.UtcNow;
				StopTask();
				return;
			}
#if UNITY_2023_OR_NEWER
			if (intervalTask.IsTaskRunning)
				return;
#else
			if (intervalTaskCoroutine != null)
				return;
#endif
			LastUpdateTime = DateTime.UtcNow;
			refillTime = DateTime.UtcNow.AddSeconds(unitTimeUpdateInSeconds);
			StartRefillTimer();
		}

		#endregion

	}
}
