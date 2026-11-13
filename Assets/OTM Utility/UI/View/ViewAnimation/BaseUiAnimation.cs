using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace com.otm.ui
{
	public abstract class BaseUiAnimation : SerializedMonoBehaviour
	{
		#region protected variables

		protected Action onHideComplete;
		protected Action onShowComplete;

		#endregion

		#region virtual methods

		public virtual void Init()
		{

		}

		public virtual void ShowAnimation(Action onShow)
		{
			this.onShowComplete = onShow;
		}

		public virtual void HideAnimation(Action action)
		{
			this.onHideComplete = action;
		}

		public virtual void StopShowAnimation() { }

		public virtual void StopHideAnimation() { }

		public virtual void StopAllAnimation()
		{
		}

		public virtual void PlayOtherAnimation(string animationKey)
		{

		}

		public virtual void StopOtherAnimation(string animationKey)
		{

		}

		public virtual bool IsAnimationRunning(string animationKey)
		{
			return false;
		}

		public virtual void SetShowAnimationValue()
		{

		}
		public virtual void SetHideAnimationValue()
		{

		}

		protected virtual void OnShowAnimationComplete()
		{
			onShowComplete?.Invoke();
		}

		protected virtual void OnHideAnimationComplete()
		{

			onHideComplete?.Invoke();
		}

		#endregion
	}
}