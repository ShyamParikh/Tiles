using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Helper = com.otm.ui.ViewHelper;

namespace com.otm.ui
{
	public abstract class BaseView : SerializedMonoBehaviour
	{
		#region protected veriabels

		[SerializeField] protected bool isBackAllowed;
		[SerializeField] protected bool hasSubview;
		[SerializeField, ShowIf("hasSubview")] protected bool waitForSubViewHide = true;
		[SerializeField, ShowIf("hasSubview")] protected SubView[] subViews = new SubView[0];
		[SerializeField] protected bool ingnoreIntraction;
		[SerializeField] protected BaseUiAnimation viewAnimation;
		[SerializeField] protected BaseViewSFX viewSFX;
		protected Dictionary<Type, SubView> mapViews = new Dictionary<Type, SubView>();
		protected bool canShow;
		protected bool canHide;
		protected GraphicRaycaster raycaster;

		#endregion

		#region private static variables

		#endregion

		#region propertices

		public BaseUiAnimation ViewAnimation => viewAnimation;

		public GraphicRaycaster GraphicRaycaster => raycaster;

		public bool IngnoreIntraction => ingnoreIntraction;

		public bool IsActive
		{
			get { return gameObject.activeSelf; }
		}

		public bool IsViewInProgress => Helper.IsViewInProgress(this);

		public bool BackAllow => isBackAllowed;

		#endregion

		#region virtual methods

		public virtual void Init()
		{
			canHide = IsActive;
			canShow = !IsActive;
			if (hasSubview)
				MapViews();
			if (viewAnimation)
				viewAnimation.Init();
			raycaster = gameObject.GetComponent<GraphicRaycaster>();
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
			HideAllSubView();
			StartCoroutine(WaitForSubViewHide());
		}

		public virtual void OnBackButtonPress()
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
			Helper.RemoveProgressionView(this);
			Helper.OnViewHide(this);
			OnViewHideDone();
		}

		protected void MakeViewHide()
		{
			canHide = false;
			canShow = true;
			Helper.RemoveProgressionView(this);
			Helper.OnViewHide(this);
			OnViewHideDone();
		}

		#endregion

		#region public methods

		public T GetSubView<T>() where T : SubView
		{
			if (mapViews.ContainsKey(typeof(T)))
				return (T)mapViews[typeof(T)];
			return null;
		}

		public bool IsSubViewActive<T>() where T : SubView
		{
			if (mapViews.ContainsKey(typeof(T)))
				return mapViews[typeof(T)].IsActive;
			return false;
		}

		public void ShowSubView<T>() where T : SubView
		{
			if (mapViews.ContainsKey(typeof(T)))
				mapViews[typeof(T)].ShowView();
		}

		public void HideSubView<T>() where T : SubView
		{
			if (mapViews.ContainsKey(typeof(T)))
				mapViews[typeof(T)].HideView();
		}

		public void HideAllSubView()
		{
			for (int i = 0; i < subViews.Length; i++)
			{
				if (subViews[i].IsActive)
					subViews[i].HideView();
			}
		}

		#endregion

		#region protected methods

		private void MapViews()
		{
			for (int i = 0; i < subViews.Length; i++)
			{
				if (!mapViews.ContainsKey(subViews[i].GetType()))
				{
					subViews[i].Init(this);
					mapViews.Add(subViews[i].GetType(), subViews[i]);
				}
			}
		}

		#endregion

		#region private methods

		[Button, HorizontalGroup("Editor")]
		public void ShowEditor()
		{
			if (Application.isPlaying)
			{
				ShowView();
				return;
			}
			gameObject.SetActive(true);
			if (viewAnimation)
				viewAnimation.SetShowAnimationValue();
		}

		[Button, HorizontalGroup("Editor")]
		public void HideEditor()
		{
			if (Application.isPlaying)
			{
				HideView();
				return;
			}
			if (viewAnimation)
				viewAnimation.SetHideAnimationValue();
			gameObject.SetActive(false);
		}

		#endregion

		#region Coroutine

		IEnumerator WaitForSubViewHide()
		{
			if (waitForSubViewHide && hasSubview)
			{
				while (SubViewHelper.IsViewInProgress(subViews))
					yield return null;

			}

			if (viewAnimation)
			{
				bool isAnimationComplete = false;
				viewAnimation.HideAnimation(() => { isAnimationComplete = true; });
				while (!isAnimationComplete || SubViewHelper.IsViewInProgress(subViews))
					yield return null;
				OnHideComplete();
			}
			else
				OnHideComplete();
		}

		#endregion
	}
}