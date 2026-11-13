using com.otm.ui;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;


namespace com.otm.ui
{
	public class UIObjectInOutAnimation : BaseUiAnimation
	{
		#region private variables

		[SerializeField] private ObjectAnimationConfig[] animationObject = new ObjectAnimationConfig[0];
		[SerializeField] private AnimationCurve showAnimtionCurve;
		[SerializeField] private AnimationCurve hideAnimtionCurve;
		[SerializeField] private float showDuration;
		[SerializeField] private float hideDuration;
		[SerializeField] private bool useUnscaleTime;

		private Coroutine showAnimatonCoroutine;
		private Coroutine hideAnimatonCoroutine;


		#endregion

		#region overrided methods

		public override void ShowAnimation(Action onShow)
		{
			base.ShowAnimation(onShow);
			showAnimatonCoroutine = StartCoroutine(DoShowAnimation());
		}

		public override void HideAnimation(Action action)
		{
			base.HideAnimation(action);
			hideAnimatonCoroutine = StartCoroutine(DoHideAnimation());
		}

		public override void StopShowAnimation()
		{
			base.StopShowAnimation();
			if (showAnimatonCoroutine != null)
				StopCoroutine(showAnimatonCoroutine);
		}

		public override void StopHideAnimation()
		{
			base.StopHideAnimation();
			if (hideAnimatonCoroutine != null)
				StopCoroutine(hideAnimatonCoroutine);
		}

		public override void SetShowAnimationValue()
		{
			base.SetShowAnimationValue();
			for (int j = 0; j < animationObject.Length; j++)
				animationObject[j].SetShowAnimationValue();
		}

		public override void SetHideAnimationValue()
		{
			base.SetHideAnimationValue();
			for (int j = 0; j < animationObject.Length; j++)
				animationObject[j].SetHideAnimationValue();
		}


		#endregion

		#region private methods

		[Button]
		private void SetAnimationConfig()
		{
			if (animationObject.Length > 0)
				return;
			animationObject = new ObjectAnimationConfig[transform.childCount];
			for (int i = 0; i < transform.childCount; i++)
			{
				ObjectAnimationConfig obj = new ObjectAnimationConfig();
				Transform child = transform.GetChild(i);
				obj.rectTransform = child.GetComponent<RectTransform>();
				obj.canvasGroup = child.GetComponent<CanvasGroup>();
				animationObject[i] = obj;
				animationObject[i].SetShowPosition();
				animationObject[i].SetHidePosition();
			}
		}

		#endregion


		#region coroutine

		IEnumerator DoShowAnimation()
		{
			float rate = 1f / showDuration;
			float i = 0;
			while (i < 1f)
			{
				i += (useUnscaleTime) ? Time.unscaledDeltaTime * rate : Time.deltaTime * rate;
				for (int j = 0; j < animationObject.Length; j++)
				{
					animationObject[j].Show(showAnimtionCurve.Evaluate(i));
				}
				yield return null;
			}
			OnShowAnimationComplete();
		}

		IEnumerator DoHideAnimation()
		{
			float rate = 1f / showDuration;
			float i = 0;
			while (i < 1f)
			{
				i += (useUnscaleTime) ? Time.unscaledDeltaTime * rate : Time.deltaTime * rate;
				for (int j = 0; j < animationObject.Length; j++)
				{
					animationObject[j].Hide(showAnimtionCurve.Evaluate(i));
				}
				yield return null;
			}
			OnHideAnimationComplete();
		}

		#endregion


		public struct ObjectAnimationConfig
		{
			public RectTransform rectTransform;
			public CanvasGroup canvasGroup;
			public Vector2 showAnchoredPosition;
			public Vector2 hideAnchoredPosition;

			#region public methods

			public void Show(float value)
			{
				if (rectTransform == null)
					return;
				rectTransform.anchoredPosition = Vector2.Lerp(hideAnchoredPosition, showAnchoredPosition, value);
				if (canvasGroup)
					canvasGroup.alpha = Mathf.Lerp(0, 1f, value);
			}

			public void Hide(float value)
			{
				if (rectTransform == null)
					return;
				rectTransform.anchoredPosition = Vector2.Lerp(showAnchoredPosition, hideAnchoredPosition, value);
				if (canvasGroup)
					canvasGroup.alpha = Mathf.Lerp(1f, 0, value);
			}

			public void SetShowAnimationValue()
			{
				rectTransform.anchoredPosition = showAnchoredPosition;
				if (canvasGroup)
					canvasGroup.alpha = 1f;
			}
			public void SetHideAnimationValue()
			{
				rectTransform.anchoredPosition = hideAnchoredPosition;
				if (canvasGroup)
					canvasGroup.alpha = 0;
			}

			[Button]
			public void SetShowPosition()
			{
				if (rectTransform == null)
					return;
				showAnchoredPosition = rectTransform.anchoredPosition;
			}
			[Button]
			public void SetHidePosition()
			{
				if (rectTransform == null)
					return;
				hideAnchoredPosition = rectTransform.anchoredPosition;
			}

			#endregion

		}
	}

}
