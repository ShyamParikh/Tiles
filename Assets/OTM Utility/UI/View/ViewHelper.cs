using com.otm.manager;
using com.otm.utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.otm.ui
{
	public static class ViewHelper
	{
		#region public static variables

		public static ActionSystem<BaseView> OnViewHideEvent = new ActionSystem<BaseView>();
		public static ActionSystem<BaseView> OnViewShowEvent = new ActionSystem<BaseView>();
		public static List<BaseView> OpenViews = new List<BaseView>();
		public static List<BaseView> InProgressView = new List<BaseView>();

		#endregion

		#region public static methods 

		public static void ResetHelper()
		{
			OnViewHideEvent.ClearEvents();
			OnViewShowEvent.ClearEvents();
			OpenViews.Clear();
			InProgressView.Clear();
		}

		public static void OnViewHide(BaseView baseView)
		{
			OpenViews.Remove(baseView);
			OnViewHideEvent.Invoke(baseView);
		}

		public static void RemoveOpenView(BaseView baseView)
		{
			OpenViews.Remove(baseView);
		}

		public static void OnViewShow(BaseView baseView)
		{
			OpenViews.Add(baseView);
			OnViewShowEvent.Invoke(baseView);
		}

		public static BaseView GetLastOpenView()
		{
			if (OpenViews.Count > 0)
				return OpenViews.Last();
			return null;
		}

		public static bool IsViewOpen(Type type)
		{
			for (int i = 0; i < OpenViews.Count; i++)
			{
				if (OpenViews[i].GetType() == type)
					return true;
			}
			return false;
		}

		public static bool IsViewOpen<T>() where T : BaseView
		{
			return IsViewOpen(typeof(T));
		}

		public static BaseView GetOpneView(Type type)
		{
			for (int i = 0; i < OpenViews.Count; i++)
			{
				if (OpenViews[i].GetType() == type)
					return OpenViews[i];
			}
			return null;
		}

		public static bool IsOtherViewOpen(params Type[] ignore)
		{
			for (int i = 0; i < OpenViews.Count; i++)
			{
				if (!ignore.Contains(OpenViews[i].GetType()))
					return true;
			}

			return false;
		}

		public static void AddProgressionView(BaseView baseView)
		{
			if (EventSystemHelper.Instance)
				EventSystemHelper.Instance.SetIntractable(false);
			InProgressView.Add(baseView);
		}

		public static void RemoveProgressionView(BaseView baseView)
		{
			InProgressView.Remove(baseView);
			if (InProgressView.Count <= 0 && EventSystemHelper.Instance)
				EventSystemHelper.Instance.SetIntractable(true);

		}

		public static bool CanIntract()
		{
			return EventSystemHelper.EventSystem.enabled;
		}

		public static bool IsViewInProgress(BaseView baseView)
		{
			return InProgressView.Contains(baseView);
		}

		#endregion
	}
}
