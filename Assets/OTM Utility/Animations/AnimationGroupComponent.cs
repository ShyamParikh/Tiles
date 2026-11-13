using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.otm.animation
{
	public class AnimationGroupComponent : SerializedMonoBehaviour
	{
		#region private variables

		[SerializeField] private bool playInSequence;
		[SerializeField] private bool playOnEnable;
		[SerializeField] private bool useUnscaleTime;
		[SerializeField] private BaseAnimation[] animations = new BaseAnimation[0];
		[SerializeField] private bool useAnimationEvent;
		[SerializeField, ShowIf("$useAnimationEvent"), HideReferenceObjectPicker] private UnityEvent onStart = new UnityEvent();
		[SerializeField, ShowIf("$useAnimationEvent"), HideReferenceObjectPicker] private UnityEvent onComplete = new UnityEvent();

		private Coroutine sequenceAnimationCoroutine;
		private Coroutine sequenceLoopAnimationCoroutine;
		private Coroutine waitForAllAnimationCompleteCoutine;


		#endregion

		#region unity callback

		private void Awake()
		{
			for (int i = 0; i < animations.Length; i++)
			{
				animations[i].Init(this, useUnscaleTime);
			}
		}

		private void OnEnable()
		{
			if (playOnEnable)
				PlayAnimation();
		}

		private void OnDisable()
		{
			StopAnimation();
		}

		private void OnDestroy()
		{
			StopAnimation();
		}

		#endregion

		#region public methods

		[Button, HorizontalGroup("AnimationButton")]
		public void PlayAnimation()
		{
			if (playInSequence)
				sequenceAnimationCoroutine = StartCoroutine(PlayAnimationInSequence(null));
			else
				PlayAllAnimation(null);
		}

		public void PlayAnimation(Action onAnimationComplete)
		{
			if (playInSequence)
				sequenceAnimationCoroutine = StartCoroutine(PlayAnimationInSequence(onAnimationComplete));
			else
				PlayAllAnimation(onAnimationComplete);
		}

		[Button, HorizontalGroup("AnimationButton")]
		public void StopAnimation()
		{
			if (sequenceAnimationCoroutine != null)
				StopCoroutine(sequenceAnimationCoroutine);
			if (sequenceLoopAnimationCoroutine != null)
				StopCoroutine(sequenceLoopAnimationCoroutine);
			if (waitForAllAnimationCompleteCoutine != null)
				StopCoroutine(waitForAllAnimationCompleteCoutine);
			for (int i = 0; i < animations.Length; i++)
				animations[i].Stop();
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
			if (useAnimationEvent)
				onStart?.Invoke();
			for (int i = 0; i < animations.Length; i++)
				animations[i].Play();
			waitForAllAnimationCompleteCoutine = StartCoroutine(WaitForAllAnimationComplete(onAnimationComplete));
		}

		#endregion

		#region coroutine

		IEnumerator WaitForAllAnimationComplete(Action onAnimationComplete)
		{
			yield return StartCoroutine(WaitForSeconds(GetDuration()));
			onAnimationComplete?.Invoke();
			if (useAnimationEvent)
				onComplete?.Invoke();
		}

		IEnumerator PlayAnimationInSequence(Action onAnimationComplete)
		{
			if (useAnimationEvent)
				onStart?.Invoke();
			List<BaseAnimation> loopAnimation = new List<BaseAnimation>();
			for (int i = 0; i < animations.Length; i++)
			{
				if (animations[i].IsLoopAnimation)
					loopAnimation.Add(animations[i]);
				animations[i].Play();
				yield return StartCoroutine(WaitForSeconds(animations[i].Duration));
				animations[i].Stop();
			}
			sequenceLoopAnimationCoroutine = StartCoroutine(DoLoopAnimationInSequence(loopAnimation));
			yield return sequenceLoopAnimationCoroutine;
			onAnimationComplete?.Invoke();
			if (useAnimationEvent)
				onComplete?.Invoke();
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
						yield return StartCoroutine(WaitForSeconds(loopAnimation[i].Duration));
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
