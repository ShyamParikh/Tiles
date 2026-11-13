using com.otm.utility;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.otm.animation
{
	public class MotionAlphaAnimation : BaseMotionAnimation
	{
		#region private variables

		[SerializeField] private AlphaType alphaType;
		[SerializeField] private float startAlpha;
		[SerializeField] private float endAlpha;
		[SerializeField] private bool useCurrentAlpha;
		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curve = new AnimationCurve();

		[SerializeField, ShowIf("$alphaType", AlphaType.Sprite)] private SpriteRenderer spriteRenderer;
		[SerializeField, ShowIf("$alphaType", AlphaType.Image)] private Image image;
		[SerializeField, ShowIf("$alphaType", AlphaType.Text)] private TMP_Text text;
		[SerializeField, ShowIf("$alphaType", AlphaType.CanvasGroup)] private CanvasGroup canvasGroup;


		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;

		private float startValue;

		#endregion

		#region overrided mmethods
		protected override void OnAnimationStart()
		{
			base.OnAnimationStart();
			SetStartAlpha();
			if (resetOnStart)
				ResetAnimation();
		}

		public override void Stop()
		{
			base.Stop();
			if (resetOnStop)
				ResetAnimation(); ;
		}

		public override void OnUpdate(float evolution)
		{
			SetAlpha(Mathf.Lerp(startValue, endAlpha, curve.Evaluate(evolution)));
		}

		protected override void OnAnimationOver()
		{
			base.OnAnimationOver();
			if (resetOnOver)
				ResetAnimation();
		}

		public override void ResetAnimation()
		{
			base.ResetAnimation();
			SetAlpha(startAlpha);
		}

		public override void SetValue<V>(V startValue, V endValue)
		{
			float? value = startValue as float?;
			startAlpha = (value != null) ? value.Value : 1;
			value = endValue as float?;
			endAlpha = (value != null) ? value.Value : 0;
			SetAlpha(startAlpha);
		}

		#endregion

		#region private methods

		private void SetStartAlpha()
		{
			if (!useCurrentAlpha)
			{
				startValue = startAlpha;
				return;
			}
			switch (alphaType)
			{
				case AlphaType.CanvasGroup:
					if (canvasGroup != null)
						startValue = canvasGroup.alpha;
					break;
				case AlphaType.Sprite:
					if (spriteRenderer != null)
						startValue = spriteRenderer.color.a;
					break;
				case AlphaType.Image:
					if (image != null)
						startValue = image.color.a;
					break;
				case AlphaType.Text:
					if (text != null)
						startValue = text.color.a;
					break;
			}
		}

		private void SetAlpha(float alpha)
		{
			switch (alphaType)
			{
				case AlphaType.CanvasGroup:
					if (canvasGroup != null)
						canvasGroup.alpha = alpha;
					break;
				case AlphaType.Sprite:
					if (spriteRenderer != null)
						spriteRenderer.color = spriteRenderer.color.SetAlpha(alpha);
					break;
				case AlphaType.Image:
					if (image != null)
						image.color = image.color.SetAlpha(alpha);
					break;
				case AlphaType.Text:
					if (text != null)
						text.color = text.color.SetAlpha(alpha);
					break;
			}
		}

		[Button]
		private void SetStartValue()
		{
			switch (alphaType)
			{
				case AlphaType.CanvasGroup:
					if (canvasGroup)
						startAlpha = canvasGroup.alpha;
					break;
				case AlphaType.Sprite:
					if (spriteRenderer)
						startAlpha = spriteRenderer.color.a;
					break;
				case AlphaType.Image:
					if (image)
						startAlpha = image.color.a;
					break;
				case AlphaType.Text:
					if (text)
						startAlpha = text.color.a;
					break;
			}
		}

		[Button]
		private void SetEndValue()
		{
			switch (alphaType)
			{
				case AlphaType.CanvasGroup:
					if (canvasGroup)
						endAlpha = canvasGroup.alpha;
					break;
				case AlphaType.Sprite:
					if (spriteRenderer)
						endAlpha = spriteRenderer.color.a;
					break;
				case AlphaType.Image:
					if (image)
						endAlpha = image.color.a;
					break;
				case AlphaType.Text:
					if (text)
						endAlpha = text.color.a;
					break;
			}
		}

		#endregion

		private enum AlphaType
		{
			CanvasGroup,
			Sprite,
			Image,
			Text,
		}
	}
}
