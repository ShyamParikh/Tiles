using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.otm.animation
{
	public class ButtonAnimation : SerializedMonoBehaviour
	{
		#region private variables

		[SerializeField] private AnimationGroup onButtonClick = new AnimationGroup();
		[SerializeField] private bool waitForAnimationComplete = false;

		private Button button;

		#endregion

		#region propertices

		public bool WaitForAnimationComplete => waitForAnimationComplete;

		#endregion

		#region unity callback

		private void Start()
		{
			button = GetComponent<Button>();
			onButtonClick.Init(this);
			if (button != null && !waitForAnimationComplete)
				button.onClick.AddListener(OnButtonClick);
		}

		private void OnDisable()
		{
			onButtonClick.StopAnimation();
			onButtonClick.ResetAnimation();
		}

		private void OnDestroy()
		{
			onButtonClick.StopAnimation();
			onButtonClick.ResetAnimation();
		}

		private void OnValidate()
		{
			Button b = GetComponent<Button>();
			if (b != null)
				b.transition = Selectable.Transition.None;
		}

		#endregion

		#region public methods

		public void PlayAnimation(Action onAnimationComplete)
		{
			if (!onButtonClick.IsAnimationRunning)
				onButtonClick.PlayAnimation(onAnimationComplete);
		}

		#endregion

		#region private methods

		private void OnButtonClick()
		{
			if (!onButtonClick.IsAnimationRunning)
				onButtonClick.PlayAnimation();
		}

		#endregion

	}
}
