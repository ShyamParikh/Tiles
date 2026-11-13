using com.otm.manager;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.otm.animation
{
	public class MotionObject : SerializedMonoBehaviour, IObjectPool
	{
		#region private variables

		[SerializeField] private bool waitForEnable;
		[SerializeField] private bool playInSequence;
		[SerializeField] private bool useScaleTime;
		[SerializeField] private BaseMotionAnimation[] animations = new BaseMotionAnimation[0];
		[SerializeField] private bool useAnimationEvent;
		[SerializeField, ShowIf("$useAnimationEvent"), HideReferenceObjectPicker] private UnityEvent onInit = new UnityEvent();
		[SerializeField, ShowIf("$useAnimationEvent"), HideReferenceObjectPicker] private UnityEvent onStart = new UnityEvent();
		[SerializeField, ShowIf("$useAnimationEvent"), HideReferenceObjectPicker] private UnityEvent onComplete = new UnityEvent();
		[SerializeField, ShowIf("$useAnimationEvent"), HideReferenceObjectPicker] private UnityEvent onEnable = new UnityEvent();
		[SerializeField, ShowIf("$useAnimationEvent"), HideReferenceObjectPicker] private UnityEvent onDisable = new UnityEvent();

		private Coroutine sequenceAnimationCoroutine;
		private Coroutine sequenceLoopAnimationCoroutine;
		private Coroutine waitForAllAnimationCompleteCoutine;
		private Coroutine waitCoroutine;
		private Dictionary<Type, List<BaseMotionAnimation>> animationMapping = new Dictionary<Type, List<BaseMotionAnimation>>();


		#endregion

		#region propertices

		public bool IsAnimationRunning { get; private set; }

		public BaseMotionAnimation[] Animations => animations;

		#endregion

		#region unity callback

		private void OnEnable()
		{
			onEnable?.Invoke();
		}

		private void OnDisable()
		{
			onDisable?.Invoke();
		}

		#endregion


		#region public methods
		public void InitObject()
		{
			if (useAnimationEvent)
				onInit?.Invoke();
			Type type;
			for (int i = 0; i < animations.Length; i++)
			{
				animations[i].Init(this, useScaleTime);
				type = animations[i].GetType();
				if (!animationMapping.ContainsKey(type))
					animationMapping.Add(type, new List<BaseMotionAnimation>());
				animationMapping[type].Add(animations[i]);
			}
			IsAnimationRunning = false;
		}

		public void OnSpwan() { }

		[Button("Play"), HorizontalGroup("Button")]
		public void PlayAnimation()
		{
			if (!gameObject.activeInHierarchy && !waitForEnable)
				return;
			if (!gameObject.activeInHierarchy && waitForEnable)
			{
				if (waitCoroutine != null)
					CoroutineRunner.Instance.StopCoroutine(waitCoroutine);
				waitCoroutine = CoroutineRunner.Instance.StartCoroutine(WaitForEnable(null));
				return;
			}
			if (playInSequence)
				sequenceAnimationCoroutine = StartCoroutine(PlayAnimationInSequence(null));
			else
				PlayAllAnimation(null);
		}

		public void PlayAnimation(Action onAnimationComplete)
		{
			if (!gameObject.activeInHierarchy && !waitForEnable)
				return;
			if (!gameObject.activeInHierarchy && waitForEnable)
			{
				if (waitCoroutine != null)
					CoroutineRunner.Instance.StopCoroutine(waitCoroutine);
				waitCoroutine = CoroutineRunner.Instance.StartCoroutine(WaitForEnable(onAnimationComplete));
				return;
			}
			if (playInSequence)
				sequenceAnimationCoroutine = StartCoroutine(PlayAnimationInSequence(onAnimationComplete));
			else
				PlayAllAnimation(onAnimationComplete);
		}

		[Button("Stop"), HorizontalGroup("Button")]
		public void StopAnimation()
		{
			IsAnimationRunning = false;
			if (!gameObject.activeInHierarchy)
				return;
			if (sequenceAnimationCoroutine != null)
				StopCoroutine(sequenceAnimationCoroutine);
			if (sequenceLoopAnimationCoroutine != null)
				StopCoroutine(sequenceLoopAnimationCoroutine);
			if (waitForAllAnimationCompleteCoutine != null)
				StopCoroutine(waitForAllAnimationCompleteCoutine);
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

		public MotionObject SetValue<T, V>(V startValue, V endValue) where T : BaseMotionAnimation
		{
			if (!animationMapping.ContainsKey(typeof(T)))
				return this;
			List<BaseMotionAnimation> animation = animationMapping[typeof(T)];
			if (animation == null)
				return this;
			for (int i = 0; i < animation.Count; i++)
			{
				animation[i].SetValue<T, V>(startValue, endValue);
			}
			return this;
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
			waitForAllAnimationCompleteCoutine = StartCoroutine(WaitForAllAnimationComplete(onAnimationComplete));
		}

		#endregion

		#region coroutine

		IEnumerator WaitForEnable(Action onAnimationComplete)
		{
			while (!gameObject.activeInHierarchy)
				yield return null;
			if (playInSequence)
				sequenceAnimationCoroutine = StartCoroutine(PlayAnimationInSequence(onAnimationComplete));
			else
				PlayAllAnimation(onAnimationComplete);
		}

		IEnumerator WaitForAllAnimationComplete(Action onAnimationComplete)
		{
			yield return StartCoroutine(WaitForSeconds(GetDuration()));
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
			List<BaseMotionAnimation> loopAnimation = new List<BaseMotionAnimation>();
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
			IsAnimationRunning = false;
		}

		IEnumerator DoLoopAnimationInSequence(List<BaseMotionAnimation> loopAnimation)
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
			if (useScaleTime)
				yield return new WaitForSecondsRealtime(wait);
			else
				yield return new WaitForSeconds(wait);
		}



		#endregion
	}

	public struct MotionObjectAnimationReffrence
	{
		public MotionObject motionObject;
		[ValueDropdown("$GetMotion")] public int selectedAnimation;
#if UNITY_EDITOR
		[ReadOnly, HideReferenceObjectPicker] public BaseMotionAnimation animation;
#endif

		#region private methods

#if UNITY_EDITOR
		[OnInspectorGUI]
		private void OnSelectionChange()
		{
			if (motionObject == null)
			{
				animation = null;
				return;
			}
			if (selectedAnimation < motionObject.Animations.Length)
				animation = motionObject.Animations[selectedAnimation];
		}
#endif

		private int[] GetMotion()
		{
			if (motionObject == null)
				return new int[0];
			int[] indexs = new int[motionObject.Animations.Length];
			for (int i = 0; i < indexs.Length; i++)
			{
				indexs[i] = i;
			}
			return indexs;
		}

		#endregion

		#region public methods

		public void SetValue<V>(V startValue, V endValue)
		{
			if (motionObject == null)
				return;
			if (selectedAnimation < motionObject.Animations.Length)
				motionObject.Animations[selectedAnimation].SetValue(startValue, endValue);
		}

		public void PlayAnimation()
		{
			motionObject?.PlayAnimation();
		}

		public void PlayAnimation(Action onAnimationComplete)
		{
			motionObject?.PlayAnimation(onAnimationComplete);
		}

		public void StopAnimation()
		{
			motionObject?.StopAnimation();
		}

		#endregion
	}
}
