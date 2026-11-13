using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace com.otm.animation
{
	public abstract class BaseMotionAnimation
	{
		#region protected variables

		[SerializeField] protected float duration = 1;
		[SerializeField, FoldoutGroup("Extra settings", Order = 1)] protected bool loop;
		[SerializeField, FoldoutGroup("Extra settings")] protected Vector2 startDelay;
		[SerializeField, FoldoutGroup("Extra settings")] protected Vector2 overDelay;
		protected MonoBehaviour monoBehaviour;
		protected Coroutine coroutine;
		protected float startDelayMain;
		protected float endDelayMain;

		#endregion

		#region propertices
		public bool IsAnimationInProgress => coroutine != null;
		public virtual float Duration => startDelayMain + duration + endDelayMain;

		public bool UsingUnscaleTime { get; set; }

		public bool IsLoopAnimation => loop;

		#endregion

		#region abstract methods

		public abstract void OnUpdate(float evolution);

		#endregion

		#region vistual methods

		public virtual void Init(MonoBehaviour monoBehaviour, bool useUnscaleTime = false)
		{
			UsingUnscaleTime = useUnscaleTime;
			this.monoBehaviour = monoBehaviour;
			startDelayMain = Random.Range(startDelay.x, startDelay.y);
			endDelayMain = Random.Range(overDelay.x, overDelay.y);
		}

		public virtual void Play()
		{
			coroutine = monoBehaviour.StartCoroutine(DoAnimation());
		}

		public virtual void Stop()
		{
			if (coroutine != null)
				monoBehaviour.StopCoroutine(coroutine);
		}

		protected virtual void OnAnimationStart() { }

		protected virtual void OnAnimationOver() { coroutine = null; }

		protected virtual void OnAnimationStartDelayOver() { }

		public virtual void ResetAnimation() { }

		public virtual void SetValue<V>(V startValue, V endValue)
		{

		}

		public virtual void SetDuration(float duration)
		{
			this.duration = duration;
		}

		#endregion

		#region public methods

		public void SetValue<T, V>(V startValue, V endValue) where T : BaseMotionAnimation
		{
			if (typeof(T) == GetType())
				SetValue(startValue, endValue);
		}

		#endregion

		#region coroutine 

		IEnumerator DoAnimation()
		{
			float i = 0;
			OnAnimationStart();
			yield return monoBehaviour.StartCoroutine(WaitForSeconds(startDelayMain));
			OnAnimationStartDelayOver();
			if (duration > 0)
			{
				float rate = 1f / duration;
				while (i < 1f)
				{
					float deltaTime = UsingUnscaleTime ? Time.unscaledDeltaTime : Time.deltaTime;
					i += deltaTime * rate;
					OnUpdate(i);
					yield return null;
				}
			}
			else
			{
				OnUpdate(1f);
			}
			yield return monoBehaviour.StartCoroutine(WaitForSeconds(endDelayMain));
			OnAnimationOver();
			if (loop)
				Play();
		}

		IEnumerator WaitForSeconds(float wait)
		{
			if (UsingUnscaleTime)
				yield return new WaitForSecondsRealtime(wait);
			else
				yield return new WaitForSeconds(wait);
		}

		#endregion

	}
}
