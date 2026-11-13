using com.otm.animation;
using com.otm.utility;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.otm.tutorial
{
	public class TutorialUIMask : SerializedMonoBehaviour
	{
		#region private variables

		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private Image maskImage;
		[SerializeField] private Image maskInnerImage;
		[SerializeField] private Dictionary<TutorialUIMaskType, MaskSpriteConfig> masks = new Dictionary<TutorialUIMaskType, MaskSpriteConfig>();
		[SerializeField] private MultiAnimationGroupComponent animationGroupComponent;

		#endregion

		#region public methods

		public void Show(TutorialUIMaskConfig maskConfig)
		{
			animationGroupComponent.Init();
			rectTransform.SetAnchor(maskConfig.anchor);
			rectTransform.anchoredPosition = maskConfig.anchorPosition;
			rectTransform.sizeDelta = maskConfig.size;
			SetMask(maskConfig.tutorialUIMaskType);
			maskInnerImage.pixelsPerUnitMultiplier = maskConfig.ppu;
			gameObject.SetActive(true);
		}

		public void Hide()
		{
			animationGroupComponent.PlayAnimation("Hide");
		}

		#endregion

		#region private methods

		private void SetMask(TutorialUIMaskType tutorialUIMaskType)
		{
			if (masks.ContainsKey(tutorialUIMaskType))
			{
				maskImage.sprite = masks[tutorialUIMaskType].maskSprite;
				maskInnerImage.sprite = masks[tutorialUIMaskType].innerMaskSprite;
				maskInnerImage.type = (tutorialUIMaskType == TutorialUIMaskType.RoundSquare) ? Image.Type.Sliced : Image.Type.Simple;
				return;
			}
			maskImage.sprite = null;
			maskInnerImage.sprite = null;
		}

		#endregion

		public struct MaskSpriteConfig
		{
			public Sprite maskSprite;
			public Sprite innerMaskSprite;
		}
	}
}
