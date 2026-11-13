using com.otm.manager;
using com.otm.utility;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace com.otm.currency
{
	[CreateAssetMenu(menuName = "OTM/Currency/Currency")]
	[Serializable]
	public class Currency : SerializedScriptableObject
	{
		#region private variables

		private DateTime infiniteEndDate;
#if UNITY_2023_OR_NEWER
		private IntervalTask timerTask = new IntervalTask();
#else
		private Coroutine intervalTaskCoroutine;
#endif


		#endregion

		#region protected variables

		[SerializeField] protected string key;
		[SerializeField] protected int defaultValue;

		#endregion

		#region public variables

		public ActionSystem<int> OnCurrencyEarn = new ActionSystem<int>();
		public ActionSystem<int> OnCurrencySpend = new ActionSystem<int>();
		public ActionSystem OnInfiniteOver = new ActionSystem();
		public ActionSystem<TimeSpan> OnInfiniteUpdate = new ActionSystem<TimeSpan>();
		public ActionSystem<int> OnCurrencyChange = new ActionSystem<int>();

		#endregion


		#region propertice

		[ShowInInspector, ReadOnly]
		public virtual int Value
		{
			get { return PlayerPrefsHelper.GetInt(key, defaultValue); }
			private set { PlayerPrefsHelper.SetInt(key, value); }
		}

		public bool IsInfiniteOnGoing => InfiniteTime.TotalSeconds > 0;
		public TimeSpan InfiniteTime { get; set; }

		public string InfiniteEndDateKey
		{
			get { return key + "InfiniteEndDate"; }
		}

		#endregion

		#region virtual methods

		public virtual void Init()
		{
			SetActionEvents();
			PlayerPrefsHelper.RegisterEvent(key, OnValueChangeInPlayerPrefab);
#if UNITY_2023_OR_NEWER
			timerTask.OnTaskUpdate.RegisterEvent(OnTimerUpdate);
			timerTask.OnTaskComplete.RegisterEvent(OnTimerComplete);
#endif
			SetInfiniteTimer();
		}

		public virtual void OnDestroy()
		{
			PlayerPrefsHelper.RemoveEvent(key, OnValueChangeInPlayerPrefab);
			OnCurrencyEarn.ClearEvents();
			OnCurrencySpend.ClearEvents();
#if UNITY_2023_OR_NEWER
			timerTask.OnTaskUpdate.DeRegisterEvent(OnTimerUpdate);
			timerTask.OnTaskComplete.DeRegisterEvent(OnTimerComplete);
#endif
		}

		public virtual void Add(int value)
		{
			if (value < 0)
			{
				if (IsInfiniteOnGoing)
					return;
			}
			Value += value;
			if (value > 0)
				OnCurrencyEarn.Invoke(value);
			else
				OnCurrencySpend.Invoke(Mathf.Abs(value));
		}

		public virtual void Delete()
		{
			Value = defaultValue;
			PlayerPrefsHelper.DeleteKey(InfiniteEndDateKey);
		}

		public virtual void SetValue(int value)
		{
			Value = value;
		}

		public virtual void AddInfinite(int value)
		{
			if (value <= 0)
				return;
			string endDateData = PlayerPrefsHelper.GetString(InfiniteEndDateKey);
			infiniteEndDate = (!string.IsNullOrEmpty(endDateData)) ? JsonConvert.DeserializeObject<DateTime>(endDateData) : DateTime.UtcNow;
			infiniteEndDate = infiniteEndDate.AddSeconds(value);
			PlayerPrefsHelper.SetString(InfiniteEndDateKey, JsonConvert.SerializeObject(infiniteEndDate));
			InfiniteTime = infiniteEndDate - DateTime.UtcNow;
			InitInfiniteTimer();
		}

		#endregion

		#region private methods

		private void SetActionEvents()
		{
			OnCurrencyEarn = new ActionSystem<int>();
			OnCurrencySpend = new ActionSystem<int>();
			OnInfiniteOver = new ActionSystem();
			OnInfiniteUpdate = new ActionSystem<TimeSpan>();
			OnCurrencyChange = new ActionSystem<int>();
		}

		private void SetInfiniteTimer()
		{
			string endDateData = PlayerPrefsHelper.GetString(InfiniteEndDateKey);
			if (string.IsNullOrEmpty(endDateData))
				return;
			infiniteEndDate = JsonConvert.DeserializeObject<DateTime>(endDateData);
			InfiniteTime = infiniteEndDate - DateTime.UtcNow;
			InitInfiniteTimer();
		}

		private void OnTimerComplete()
		{
			PlayerPrefsHelper.DeleteKey(InfiniteEndDateKey);
			InfiniteTime = new TimeSpan();
			OnInfiniteOver.Invoke();
		}

		private void OnTimerUpdate()
		{
			InfiniteTime = infiniteEndDate - DateTime.UtcNow;
			OnInfiniteUpdate.Invoke(InfiniteTime);
		}

		private void OnValueChangeInPlayerPrefab()
		{
			OnCurrencyChange.Invoke(Value);
		}

		private void InitInfiniteTimer()
		{
#if UNITY_2023_OR_NEWER
			if (timerTask != null)
				timerTask.Stop();

			if (InfiniteTime.TotalSeconds > 0)
			{
				timerTask.Start(IsValidRemainTime, 1f);
				return;
			}
#else
			if (intervalTaskCoroutine != null)
				CoroutineRunner.Instance.StopCoroutine(intervalTaskCoroutine);
			if (InfiniteTime.TotalSeconds > 0)
			{
				intervalTaskCoroutine = CoroutineRunner.Instance.IntervalTask(IsValidRemainTime, 1, false, OnTimerUpdate, OnTimerComplete);
				return;
			}
#endif
			OnTimerComplete();
		}

		private bool IsValidRemainTime()
		{
			return infiniteEndDate > DateTime.UtcNow;
		}

		#endregion
	}
}