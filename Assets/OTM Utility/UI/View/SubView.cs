using Sirenix.OdinInspector;
using UnityEngine;
using Helper = com.otm.ui.SubViewHelper;

namespace com.otm.ui
{
	public class SubView : SerializedMonoBehaviour
	{

		#region protected veriabels

		[SerializeField] protected BaseUiAnimation viewAnimation;
		[SerializeField] protected BaseViewSFX viewSFX;
		protected bool canShow;
		protected bool canHide;
		protected BaseView parentView;

		#endregion

		#region private static variables

		#endregion

		#region propertices

		public bool IsActive
		{
			get { return gameObject.activeSelf; }
		}
		public bool IsViewInProgress => Helper.IsViewInProgress(this);

		#endregion

		#region virtual methods

		public virtual void Init(BaseView baseView)
		{
			canHide = IsActive;
			canShow = !IsActive;
			parentView = baseView;
			if (viewAnimation)
				viewAnimation.Init();
		}
		public virtual void OnDestroy()
		{
			Helper.RemoveProgressionView(this);
			Helper.RemoveOpenView(this);
		}

		public virtual void ShowView()
		{
			if (!canShow || IsViewInProgress)
				return;
			Helper.AddProgressionView(this);
			gameObject?.SetActive(true);
			Helper.OnViewShow(this);
			viewSFX?.PlayShowSound();
			if (viewAnimation)
				viewAnimation.ShowAnimation(OnShowComplete);
			else
				OnShowComplete();
		}

		public virtual void HideView()
		{
			if (!canHide || IsViewInProgress)
				return;
			viewSFX?.PlayHideSound();
			Helper.AddProgressionView(this);
			if (viewAnimation)
				viewAnimation.HideAnimation(OnHideComplete);
			else
				OnHideComplete();
		}

		protected virtual void OnBackgroundClick()
		{
			HideView();
		}

		protected virtual void OnViewShowDone()
		{
		}

		protected virtual void OnViewHideDone()
		{

		}

		#endregion

		#region protected methods
		protected void OnShowComplete()
		{
			canHide = true;
			canShow = false;
			Helper.RemoveProgressionView(this);
			OnViewShowDone();
		}

		protected void OnHideComplete()
		{
			canHide = false;
			canShow = true;
			gameObject.SetActive(false);
			OnViewHideDone();
			Helper.RemoveProgressionView(this);
			Helper.OnViewHide(this);
		}

		#endregion

		#region private methods

		[Button, HorizontalGroup("Editor")]
		private void ShowEditor()
		{
			gameObject.SetActive(true);
			if (viewAnimation)
				viewAnimation.SetShowAnimationValue();
		}

		[Button, HorizontalGroup("Editor")]
		private void HideEditor()
		{
			if (viewAnimation)
				viewAnimation.SetHideAnimationValue();
			gameObject.SetActive(false);
		}

		#endregion
	}
}