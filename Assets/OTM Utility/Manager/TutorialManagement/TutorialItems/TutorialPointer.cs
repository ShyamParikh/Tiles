using com.otm.animation;
using com.otm.utility;
using System.Collections;
using UnityEngine;

namespace com.otm.tutorial
{
	public class TutorialPointer : MonoBehaviour
	{
		#region private variables
		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private MultiAnimationGroupComponent animationGroupComponent;

		private Coroutine pointerCoroutine;
		#endregion

		#region public methods

		public void Show(TutorialPointerConfig tutorialPointerConfig)
		{
			animationGroupComponent.Init();
			rectTransform.SetAnchor(tutorialPointerConfig.anchor);
			rectTransform.anchoredPosition = tutorialPointerConfig.anchorPosition;
			rectTransform.eulerAngles = Vector3.forward * tutorialPointerConfig.rotation;
			if (Application.isPlaying && tutorialPointerConfig.useAnimation)
				animationGroupComponent.PlayAnimation("PointAnimation");

		}

		public void Show(TutorialPointerFollowConfig tutorialPointerFollowConfig)
		{
            animationGroupComponent.Init();
			rectTransform.eulerAngles = Vector3.forward * tutorialPointerFollowConfig.rotation;
            if (Application.isPlaying && tutorialPointerFollowConfig.useAnimation)
                animationGroupComponent.PlayAnimation("PointAnimation");
			pointerCoroutine = StartCoroutine(DoFollow(tutorialPointerFollowConfig));
        }
		public void Hide()
		{
			if (Application.isPlaying)
				animationGroupComponent.StopAnimation("PointAnimation");
			if (pointerCoroutine != null)
				StopCoroutine(pointerCoroutine);
		}

		#endregion

		#region Coroutines

		IEnumerator DoFollow(TutorialPointerFollowConfig pointerConfig)
		{
			while (true)
			{
				transform.position = pointerConfig.target.position + pointerConfig.offeset;
				yield return null;
			}
		}

		#endregion
	}

}
