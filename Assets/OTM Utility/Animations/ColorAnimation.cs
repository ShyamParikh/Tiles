using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace com.otm.animation
{
	public class ColorAnimation : BaseAnimation
	{
		#region private variables

		[SerializeField] private ColorType colorType;
		[SerializeField] private Color startColor = Color.white;
		[SerializeField] private Color endColor = Color.white;
		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curve = new AnimationCurve();

		[SerializeField, ShowIf("$colorType", ColorType.Sprite)] private SpriteRenderer spriteRenderer;
		[SerializeField, ShowIf("$colorType", ColorType.Image)] private Image image;
		[SerializeField, ShowIf("$colorType", ColorType.Text)] private TMP_Text text;


		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;

		#endregion

		#region overrided mmethods
		protected override void OnAnimationStart()
		{
			base.OnAnimationStart();
			if (resetOnStart)
				ResetAnimation();
		}

		public override void Stop()
		{
			base.Stop();
			if (resetOnStop)
				ResetAnimation();
		}

		public override void OnUpdate(float evolution)
		{
			SetColor(Color.Lerp(startColor, endColor, curve.Evaluate(evolution)));
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
			SetColor(startColor);
		}

		public override void SetToStartValue()
		{
			base.SetToStartValue();
			SetColor(startColor);
		}

		public override void SetToEndValue()
		{
			base.SetToEndValue();
			SetColor(endColor);
		}

		#endregion

		#region private methods

		private void SetColor(Color color)
		{
			switch (colorType)
			{
				case ColorType.Sprite:
					if (spriteRenderer)
						spriteRenderer.color = color;
					break;
				case ColorType.Image:
					if (image)
						image.color = color;
					break;
				case ColorType.Text:
					if (text)
						text.color = color;
					break;
			}
		}

		[Button]
		private void SetStartValue()
		{
			switch (colorType)
			{
				case ColorType.Sprite:
					if (spriteRenderer)
						startColor = spriteRenderer.color;
					break;
				case ColorType.Image:
					if (image)
						startColor = image.color;
					break;
				case ColorType.Text:
					if (text)
						startColor = text.color;
					break;
			}
		}

		[Button]
		private void SetEndValue()
		{
			switch (colorType)
			{
				case ColorType.Sprite:
					if (spriteRenderer)
						endColor = spriteRenderer.color;
					break;
				case ColorType.Image:
					if (image)
						endColor = image.color;
					break;
				case ColorType.Text:
					if (text)
						endColor = text.color;
					break;
			}
		}

		#endregion

		private enum ColorType
		{
			Sprite,
			Image,
			Text,
		}
	}
}
