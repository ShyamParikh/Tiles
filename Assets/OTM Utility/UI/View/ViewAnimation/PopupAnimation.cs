using com.otm.ui;
using System;
using System.Collections;
using UnityEngine;

namespace com.otm.ui
{
	public class PopupAnimation : BaseUiAnimation
	{
		#region private variables

		[SerializeField] private float showDuration = 0.4f;
		[SerializeField] private float hideDuration = 0.3f;
		[SerializeField] private bool useUnscaleTime;

		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private AnimationCurve showAlphaAnimationCurve;
		[SerializeField] private AnimationCurve hideAlphaAnimationCurve;

		[SerializeField] private Transform popupTransform;
		[SerializeField] private Transform[] otherPopopTranforms = new Transform[0];
		[SerializeField] private AnimationCurve showScaleAnimationCurve;
		[SerializeField] private AnimationCurve hideScaleAnimationCurve;

		private Coroutine showAnimationCoroutine;
		private Coroutine hideAnimationCoroutine;

		#endregion

		#region overrided methods

		public override void ShowAnimation(Action onShow)
		{
			base.ShowAnimation(onShow);
			StopCoroutines();
			showAnimationCoroutine = StartCoroutine(DoShowAnimation());
		}

		public override void HideAnimation(Action action)
		{
			base.HideAnimation(action);
			StopCoroutines();
			hideAnimationCoroutine = StartCoroutine(DoHideAnimation());
		}

		public override void SetShowAnimationValue()
		{
			base.SetShowAnimationValue();
			popupTransform.localScale = Vector3.one;
			for (int j = 0; j < otherPopopTranforms.Length; j++)
				otherPopopTranforms[j].localScale = Vector3.one;
			if (canvasGroup)
				canvasGroup.alpha = 1f;
		}

		public override void SetHideAnimationValue()
		{
			base.SetHideAnimationValue();
			popupTransform.localScale = Vector3.zero;
			for (int j = 0; j < otherPopopTranforms.Length; j++)
				otherPopopTranforms[j].localScale = Vector3.zero;
			if (canvasGroup)
				canvasGroup.alpha = 0f;
		}

		#endregion

		#region private methods

		private void StopCoroutines()
		{
			if (showAnimationCoroutine != null)
				StopCoroutine(showAnimationCoroutine);
			if (hideAnimationCoroutine != null)
				StopCoroutine(hideAnimationCoroutine);
		}

		#endregion

		#region coroutine

		IEnumerator DoShowAnimation()
		{
			float i = 0;
			float rate = 1f / showDuration;
			float deltaTime = 0;
			while (i < 1f)
			{
				deltaTime = (useUnscaleTime) ? Time.unscaledDeltaTime : Time.deltaTime;
				i += deltaTime * rate;
				if (canvasGroup != null)
					canvasGroup.alpha = Mathf.Lerp(0, 1f, showAlphaAnimationCurve.Evaluate(i));
				popupTransform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, showScaleAnimationCurve.Evaluate(i));
				for (int j = 0; j < otherPopopTranforms.Length; j++)
					otherPopopTranforms[j].localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, showScaleAnimationCurve.Evaluate(i));
				yield return null;
			}
			OnShowAnimationComplete();
		}

		IEnumerator DoHideAnimation()
		{
			float i = 0;
			float rate = 1f / hideDuration;
			float deltaTime = 0;
			while (i < 1f)
			{
				deltaTime = (useUnscaleTime) ? Time.unscaledDeltaTime : Time.deltaTime;
				i += deltaTime * rate;
				if (canvasGroup != null)
					canvasGroup.alpha = Mathf.Lerp(1f, 0, hideAlphaAnimationCurve.Evaluate(i));
				popupTransform.localScale = Vector3.LerpUnclamped(Vector3.one, Vector3.zero, hideScaleAnimationCurve.Evaluate(i));
				for (int j = 0; j < otherPopopTranforms.Length; j++)
					otherPopopTranforms[j].localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, showScaleAnimationCurve.Evaluate(i));
				yield return null;
			}
			OnHideAnimationComplete();
		}

		#endregion
	}
}
