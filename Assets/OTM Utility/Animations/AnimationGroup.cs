using com.otm.manager;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.otm.animation
{
	[HideReferenceObjectPicker]
	public class AnimationGroup
	{
		#region private variables

		[SerializeField] private bool waitForEnable;
		[SerializeField] private bool playInSequence;
		[SerializeField] private bool useUnscaleTime;
		[SerializeField] private BaseAnimation[] animations = new BaseAnimation[0];
		[SerializeField] private bool useAnimationEvent;
		[SerializeField, ShowIf("$useAnimationEvent"), HideReferenceObjectPicker] private UnityEvent onInit = new UnityEvent();
		[SerializeField, ShowIf("$useAnimationEvent"), HideReferenceObjectPicker] private UnityEvent onStart = new UnityEvent();
		[SerializeField, ShowIf("$useAnimationEvent"), HideReferenceObjectPicker] private UnityEvent onComplete = new UnityEvent();

		private Coroutine sequenceAnimationCoroutine;
		private Coroutine sequenceLoopAnimationCoroutine;
		private Coroutine waitForAllAnimationCompleteCoutine;
		private Coroutine waitCoroutine;
		private MonoBehaviour monoBehaviour;

		#endregion

		#region propertices

		public bool IsAnimationRunning { get; private set; }

		#endregion


		#region public methods

		public void Init(MonoBehaviour monoBehaviour)
		{
			this.monoBehaviour = monoBehaviour;
			if (useAnimationEvent)
				onInit?.Invoke();
			for (int i = 0; i < animations.Length; i++)
			{
				animations[i].Init(monoBehaviour, useUnscaleTime);
			}
			IsAnimationRunning = false;
		}

		[Button("Play"), HorizontalGroup("Button")]
		public void PlayAnimation()
		{
			if (monoBehaviour == null || (!monoBehaviour.gameObject.activeInHierarchy && !waitForEnable))
				return;
			if (!monoBehaviour.gameObject.activeInHierarchy && waitForEnable)
			{
				if (waitCoroutine != null)
					CoroutineRunner.Instance.StopCoroutine(waitCoroutine);
				waitCoroutine = CoroutineRunner.Instance.StartCoroutine(WaitForEnable(null));
				return;
			}
			if (playInSequence)
				sequenceAnimationCoroutine = monoBehaviour.StartCoroutine(PlayAnimationInSequence(null));
			else
				PlayAllAnimation(null);
		}

		public void Update(float amount)
		{
			for (int i = 0; i < animations.Length; i++)
			{
				animations[i].OnUpdate(amount);
			}
		}

		public void PlayAnimation(Action onAnimationComplete)
		{
			if (monoBehaviour == null || (!monoBehaviour.gameObject.activeInHierarchy && !waitForEnable))
				return;
			if (!monoBehaviour.gameObject.activeInHierarchy && waitForEnable)
			{
				if (waitCoroutine != null)
					CoroutineRunner.Instance.StopCoroutine(waitCoroutine);
				waitCoroutine = CoroutineRunner.Instance.StartCoroutine(WaitForEnable(onAnimationComplete));
				return;
			}
			if (playInSequence)
				sequenceAnimationCoroutine = monoBehaviour.StartCoroutine(PlayAnimationInSequence(onAnimationComplete));
			else
				PlayAllAnimation(onAnimationComplete);
		}

		[Button("Stop"), HorizontalGroup("Button")]
		public void StopAnimation()
		{
			IsAnimationRunning = false;
			if (monoBehaviour == null || !monoBehaviour.gameObject.activeInHierarchy)
				return;
			if (sequenceAnimationCoroutine != null)
				monoBehaviour.StopCoroutine(sequenceAnimationCoroutine);
			if (sequenceLoopAnimationCoroutine != null)
				monoBehaviour.StopCoroutine(sequenceLoopAnimationCoroutine);
			if (waitForAllAnimationCompleteCoutine != null)
				monoBehaviour.StopCoroutine(waitForAllAnimationCompleteCoutine);
			for (int i = 0; i < animations.Length; i++)
				animations[i].Stop();
		}

		public void ResetAnimation()
		{
			for (int i = 0; i < animations.Length; i++)
			{
				animations[i].ResetAnimation();
			}
		}

		public float GetDuration()
		{
			float duration = 0f;
			for (int i = 0; i < animations.Length; i++)
			{
				if (playInSequence)
					duration += animations[i].Duration;
				if (!playInSequence && duration < animations[i].Duration)
					duration = animations[i].Duration;
			}
			return duration;
		}

		[Button]
		public void SetToStartValue()
		{
			for (int i = 0; i < animations.Length; i++)
				animations[i].SetToStartValue();
		}

		[Button]
		public void SetToEndValue()
		{
			for (int i = 0; i < animations.Length; i++)
				animations[i].SetToEndValue();
		}

		#endregion

		#region private methods

		private void PlayAllAnimation(Action onAnimationComplete)
		{
			IsAnimationRunning = true;
			if (useAnimationEvent)
				onStart?.Invoke();
			for (int i = 0; i < animations.Length; i++)
				animations[i].Play();
			waitForAllAnimationCompleteCoutine = monoBehaviour.StartCoroutine(WaitForAllAnimationComplete(onAnimationComplete));
		}

		#endregion

		#region coroutine

		IEnumerator WaitForEnable(Action onAnimationComplete)
		{
			while (!monoBehaviour.gameObject.activeInHierarchy)
				yield return null;
			if (playInSequence)
				sequenceAnimationCoroutine = monoBehaviour.StartCoroutine(PlayAnimationInSequence(onAnimationComplete));
			else
				PlayAllAnimation(onAnimationComplete);
		}

		IEnumerator WaitForAllAnimationComplete(Action onAnimationComplete)
		{
			yield return monoBehaviour.StartCoroutine(WaitForSeconds(GetDuration()));
			onAnimationComplete?.Invoke();
			if (useAnimationEvent)
				onComplete?.Invoke();
			IsAnimationRunning = false;
		}

		IEnumerator PlayAnimationInSequence(Action onAnimationComplete)
		{
			IsAnimationRunning = true;
			if (useAnimationEvent)
				onStart?.Invoke();
			List<BaseAnimation> loopAnimation = new List<BaseAnimation>();
			for (int i = 0; i < animations.Length; i++)
			{
				if (animations[i].IsLoopAnimation)
					loopAnimation.Add(animations[i]);
				animations[i].Play();
				yield return monoBehaviour.StartCoroutine(WaitForSeconds(animations[i].Duration));
				animations[i].Stop();
			}
			sequenceLoopAnimationCoroutine = monoBehaviour.StartCoroutine(DoLoopAnimationInSequence(loopAnimation));
			yield return sequenceLoopAnimationCoroutine;
			onAnimationComplete?.Invoke();
			if (useAnimationEvent)
				onComplete?.Invoke();
			IsAnimationRunning = false;
		}

		IEnumerator DoLoopAnimationInSequence(List<BaseAnimation> loopAnimation)
		{
			if (loopAnimation.Count > 0)
			{
				while (true)
				{
					for (int i = 0; i < loopAnimation.Count; i++)
					{
						loopAnimation[i].Play();
						yield return monoBehaviour.StartCoroutine(WaitForSeconds(loopAnimation[i].Duration));
						loopAnimation[i].Stop();
					}
				}
			}
		}

		IEnumerator WaitForSeconds(float wait)
		{
			if (useUnscaleTime)
				yield return new WaitForSecondsRealtime(wait);
			else
				yield return new WaitForSeconds(wait);
		}

		#endregion
	}
}
