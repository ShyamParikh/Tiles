using com.otm.manager;
using System;
using System.Collections;
using UnityEngine;

namespace com.otm.animation
{
	public class CurrencyAnimation : MonoBehaviour, IObjectPool
	{
		#region private variables

		[SerializeField] private float radius;
		[SerializeField] private int objectsize;
		[SerializeField] private Vector2 randomDelay;
		[SerializeField] private MotionObject motionObjectPrefab;
		[SerializeField] private float waitAfterAnimtionComplete;
		private MotionObject[] motionObjects;
		private Action onAnimationComplete;
		private Action<int> onSigleAnimationComplete;
		private int[] amounts;
		private int objectCount;

		#endregion

		#region public methods
		public void InitObject()
		{
			motionObjects = new MotionObject[objectsize];
			for (int i = 0; i < objectsize; i++)
			{
				motionObjects[i] = Instantiate(motionObjectPrefab, transform);
				motionObjects[i].InitObject();
				motionObjects[i].gameObject.SetActive(false);
			}
		}
		public void OnSpwan() { }
		#endregion
		#region public methods

		public void PlayAnimation(int amount, RectTransform startPoint, RectTransform endPoint, Action onAnimationComplete = null, Action<int> onSigleAnimationComplete = null)
		{
			if (motionObjects == null)
				return;
			PlayAnimation(amount, startPoint.position, endPoint.position, onAnimationComplete, onSigleAnimationComplete);
		}

		public void PlayAnimation(int amount, Vector2 startPoint, Vector2 endPoint, Action onAnimationComplete = null, Action<int> onSigleAnimationComplete = null)
		{
			if (motionObjects == null)
				return;
			SetAmount(amount);
			this.onSigleAnimationComplete = onSigleAnimationComplete;
			this.onAnimationComplete = onAnimationComplete;
			StartCoroutine(DoAnimation(startPoint, endPoint));
		}

		public void SetValue<T, V>(V startValue, V endValue) where T : BaseMotionAnimation
		{
			for (int i = 0; i < motionObjects.Length; i++)
			{
				motionObjects[i].SetValue<T, V>(startValue, endValue);
			}
		}

		#endregion

		#region private methods

		private void SetAmount(int amount)
		{
			objectCount = Mathf.Clamp(Mathf.Abs(amount), 0, objectsize);
			amounts = new int[objectCount];
			int singleObjectValue = amount / objectCount;
			int remainAmount = amount - (singleObjectValue * objectCount);
			for (int i = 0; i < objectCount; i++)
				amounts[i] = singleObjectValue;
			amounts[objectCount - 1] += remainAmount;
		}

		private void PlayPlacementSound()
		{

		}

		#endregion

		#region coroutine

		IEnumerator DoAnimation(Vector2 startPoint, Vector2 endPoint)
		{
			Vector2 randomPoint;
			for (int i = 0; i < objectCount; i++)
			{
				randomPoint = (UnityEngine.Random.insideUnitCircle) * radius + startPoint;
				motionObjects[i].SetValue<MotionPositionAnimation, Vector3>(randomPoint, endPoint);
				motionObjects[i].gameObject.SetActive(true);
				motionObjects[i].PlayAnimation(PlayPlacementSound);
				yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(randomDelay.x, randomDelay.y));
			}
			StartCoroutine(WaiForAnimationComplete());
		}

		IEnumerator WaiForAnimationComplete()
		{
			yield return new WaitForEndOfFrame();
			for (int i = 0; i < objectCount; i++)
			{
				while (motionObjects[i].IsAnimationRunning)
					yield return null;
				if (amounts.Length >= objectCount)
					onSigleAnimationComplete?.Invoke(amounts[i]);
			}
			for (int i = 0; i < motionObjects.Length; i++)
				motionObjects[i].gameObject.SetActive(false);
			yield return new WaitForSecondsRealtime(waitAfterAnimtionComplete);
			onAnimationComplete?.Invoke();
			ObjectPoolManager.Recycle(this);
		}

		#endregion
	}
}
