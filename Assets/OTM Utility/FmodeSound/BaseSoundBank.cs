#if UNITY_FMODE
using com.otm.manager;
using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.sfx
{
	public abstract class BaseSoundBank : SerializedScriptableObject
	{
#region protected variables
		[SerializeField] protected EventReference musicEvent;
		[SerializeField] protected FMOD.Studio.STOP_MODE stopMode;
		[SerializeField] protected bool createNewIntanceEachTime;
		[SerializeField] protected bool useStartDelay;
		[SerializeField, ShowIf("useStartDelay"), HorizontalGroup("StartDelay/h"), BoxGroup("StartDelay"), LabelText("Min")]
		protected float minStartDelay;
		[SerializeField, ShowIf("useStartDelay"), HorizontalGroup("StartDelay/h"), BoxGroup("StartDelay"), LabelText("Max")]
		protected float maxStartDelay;
		protected EventInstance Instance;
		protected List<EventInstance> instances = new List<EventInstance>();
		protected Coroutine delayCoroutine;
		protected Dictionary<string, Coroutine> valueLerpCoroutines = new Dictionary<string, Coroutine>();

#endregion

#region virtual methods

		public virtual void OnEnable() { }
		public virtual void OnDisable() { }

		public virtual void CreateInstance()
		{
			EventInstance newInstance = RuntimeManager.CreateInstance(musicEvent);
			Instance = newInstance;
			instances.Add(newInstance);
		}

		public virtual void Play()
		{
			if (createNewIntanceEachTime || !Instance.isValid())
				CreateInstance();
			if (useStartDelay)
			{
				delayCoroutine = CoroutineRunner.Instance.StartCoroutine(DoDelayAndPlay());
				return;
			}
			Instance.start();
		}

		public virtual void Pause()
		{
			if (Instance.isValid())
				Instance.setPaused(true);
			for (int i = 0; i < instances.Count; i++)
			{
				if (instances[i].isValid())
					instances[i].setPaused(true);
			}
		}

		public virtual void Resume()
		{
			if (Instance.isValid())
				Instance.setPaused(false);
			for (int i = 0; i < instances.Count; i++)
			{
				if (instances[i].isValid())
					instances[i].setPaused(false);
			}
		}

		public virtual void Stop()
		{
			if (delayCoroutine != null)
				CoroutineRunner.Instance.StopCoroutine(delayCoroutine);
			StopAllValueLerping();
			Instance.stop(stopMode);
		}

		public virtual bool IsPlaying()
		{
			PLAYBACK_STATE state;
			Instance.getPlaybackState(out state);
			return state != PLAYBACK_STATE.STOPPED;
		}

		public virtual EventInstance GetInstance()
		{
			EventInstance newInstance = RuntimeManager.CreateInstance(musicEvent);
			instances.Add(newInstance);
			return newInstance;
		}

		public virtual void Release()
		{
			if (Instance.isValid())
			{
				Instance.release();
				Instance.clearHandle();
			}
			for (int i = 0; i < instances.Count; i++)
			{
				if (instances[i].isValid())
				{
					instances[i].release();
					instances[i].clearHandle();
				}
			}
			instances.Clear();
		}
		public virtual void SetParameter(string parameterName, float value)
		{
			if (string.IsNullOrEmpty(parameterName))
				return;
			Instance.setParameterByName(parameterName, value);
		}

		public virtual void SetParameterWithLerp(string parameterName, float endValue, float time, AnimationCurve animationCurve = null, Action onComplete = null)
		{
			Instance.getParameterByName(parameterName, out float startValue);
			SetParameterWithLerp(parameterName, startValue, endValue, animationCurve, onComplete);
		}

		public virtual void SetParameterWithLerp(string parameterName, float startValue, float endValue, float time, AnimationCurve animationCurve = null, Action onComplete = null)
		{
			if (string.IsNullOrEmpty(parameterName))
				return;
			if (valueLerpCoroutines.ContainsKey(parameterName) && valueLerpCoroutines[parameterName] != null)
				CoroutineRunner.Instance.StopCoroutine(valueLerpCoroutines[parameterName]);
			Coroutine c = CoroutineRunner.Instance.StartCoroutine(DoValueLerp(parameterName, startValue, endValue, time, animationCurve, onComplete));
			if (!valueLerpCoroutines.ContainsKey(parameterName))
				valueLerpCoroutines.Add(parameterName, c);
			else
				valueLerpCoroutines[parameterName] = c;
		}

#endregion

#region private methods

		private void StopAllValueLerping()
		{
			foreach (var item in valueLerpCoroutines)
			{
				if (item.Value != null)
					CoroutineRunner.Instance.StopCoroutine(item.Value);
			}
			valueLerpCoroutines.Clear();
		}

#endregion

#region coroutine
		IEnumerator DoDelayAndPlay()
		{
			yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(minStartDelay, maxStartDelay));
			Instance.start();
		}

		IEnumerator DoValueLerp(string parameterName, float startValue, float endValue, float time, AnimationCurve animationCurve, Action onComplete)
		{
			float i = 0;
			float rate = 1f / time;
			float lerpValue;
			float evaluate = 0;
			while (i < 1f && Instance.isValid())
			{
				i += Time.unscaledDeltaTime * rate;
				evaluate = (animationCurve != null) ? animationCurve.Evaluate(i) : i;
				lerpValue = Mathf.Lerp(startValue, endValue, i);
				Instance.setParameterByName(parameterName, lerpValue);
				yield return null;
			}
			valueLerpCoroutines[parameterName] = null;
			onComplete?.Invoke();
		}

#endregion
	}
}
#endif
