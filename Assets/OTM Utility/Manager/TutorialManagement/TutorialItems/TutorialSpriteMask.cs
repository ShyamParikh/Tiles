using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.tutorial
{
	public class TutorialSpriteMask : SerializedMonoBehaviour
	{
		#region private variables

		[SerializeField] private SpriteMask maskSpriteRenderer;
		[SerializeField] private SpriteRenderer maskInnerSpriteRenderer;
		[SerializeField] private AnimationCurve animationCurve;
		[SerializeField] private Color showColor;
		[SerializeField] private Color hideColor;
		[SerializeField] private Dictionary<TutorialSpriteMaskType, MaskSpriteConfig> masks = new Dictionary<TutorialSpriteMaskType, MaskSpriteConfig>();
		private Vector3 scale;
		private Coroutine followCoroutine;

		#endregion

		#region public methods

		public void Show(TutorialSpriteMaskConfig maskConfig)
		{
			transform.position = maskConfig.position;
			scale = maskConfig.scale / transform.parent.localScale;
			SetMask(maskConfig.tutorialSpriteMaskType);
			gameObject.SetActive(true);
			if (Application.isPlaying)
			{
				transform.localScale = Vector3.zero;
				transform.eulerAngles = Vector3.forward * maskConfig.rotation;
				maskInnerSpriteRenderer.color = Color.clear;
				StartCoroutine(DoScale(0.21f, Vector3.zero, scale, showColor));
			}
			else
			{
				maskInnerSpriteRenderer.color = showColor;
				transform.localScale = scale;
				transform.eulerAngles = Vector3.forward * maskConfig.rotation;
			}

		}

		public void Show(TutorialFollowSpriteMaskConfig maskConfig)
		{
			transform.position = maskConfig.target.position + maskConfig.offeset;
			scale = maskConfig.scale / transform.parent.localScale;
			SetMask(maskConfig.tutorialSpriteMaskType);
			gameObject.SetActive(true);
			transform.localScale = Vector3.zero;
			StartCoroutine(DoScale(0.21f, Vector3.zero, scale, showColor));
			followCoroutine = StartCoroutine(DoFollow(maskConfig));
		}

		public void Hide()
		{
			if (Application.isPlaying)
				StartCoroutine(DoScale(0.19f, scale, Vector3.zero, hideColor));
			if (followCoroutine != null)
				StopCoroutine(followCoroutine);
		}

		#endregion

		#region private methods

		private void SetMask(TutorialSpriteMaskType tutorialSpriteMaskType)
		{
			if (masks.ContainsKey(tutorialSpriteMaskType))
			{
				maskSpriteRenderer.sprite = masks[tutorialSpriteMaskType].maskSprite;
				maskInnerSpriteRenderer.sprite = masks[tutorialSpriteMaskType].innerMaskSprite;
				return;
			}
			maskSpriteRenderer.sprite = null;
			maskInnerSpriteRenderer.sprite = null;
		}

		#endregion

		#region coroutine

		IEnumerator DoScale(float time, Vector3 startScale, Vector3 endScale, Color endColor)
		{
			float i = 0;
			float rate = 1f / time;
			Color startColor = maskInnerSpriteRenderer.color;
			while (i < 1f)
			{
				i += Time.unscaledDeltaTime * rate;
				transform.localScale = Vector3.Lerp(startScale, endScale, animationCurve.Evaluate(i));
				maskInnerSpriteRenderer.color = Color.Lerp(startColor, endColor, animationCurve.Evaluate(i));
				yield return null;
			}
		}
		IEnumerator DoFollow(TutorialFollowSpriteMaskConfig maskConfig)
		{
			while (true)
			{
				transform.position = maskConfig.target.position + maskConfig.offeset;
				yield return null;
			}
		}

		#endregion

		public struct MaskSpriteConfig
		{
			public Sprite maskSprite;
			public Sprite innerMaskSprite;
		}
	}
}
