using com.otm.utility;
using System.Collections.Generic;
using System;
using System.Linq;
using com.otm.manager;

namespace com.otm.ui
{
	public static class SubViewHelper
	{
		#region public static variables

		public static ActionSystem<SubView> onViewHideEvent = new ActionSystem<SubView>();
		public static ActionSystem<SubView> onViewShowEvent = new ActionSystem<SubView>();
		public static List<SubView> openViews = new List<SubView>();
		public static List<SubView> inProgressView = new List<SubView>();

		#endregion
		#region public static methods 

		public static void ResetHelper()
		{
			onViewHideEvent.ClearEvents();
			onViewShowEvent.ClearEvents();
			openViews.Clear();
			inProgressView.Clear();
		}
		public static void OnViewHide(SubView subView)
		{
			openViews.Remove(subView);
			onViewHideEvent.Invoke(subView);
		}

		public static void RemoveOpenView(SubView subView)
		{
			openViews.Remove(subView);
		}

		public static void OnViewShow(SubView subView)
		{
			openViews.Add(subView);
			onViewShowEvent.Invoke(subView);
		}

		public static bool IsViewOpen(Type type)
		{
			for (int i = 0; i < openViews.Count; i++)
			{
				if (openViews[i].GetType() == type)
					return true;
			}
			return false;
		}

		public static SubView GetOpneView(Type type)
		{
			for (int i = 0; i < openViews.Count; i++)
			{
				if (openViews[i].GetType() == type)
					return openViews[i];
			}
			return null;
		}

		public static void AddProgressionView(SubView subView)
		{
			if (EventSystemHelper.Instance)
				EventSystemHelper.Instance.SetIntractable(false);
			inProgressView.Add(subView);
		}

		public static void RemoveProgressionView(SubView subView)
		{
			inProgressView.Remove(subView);
			if (inProgressView.Count <= 0 && EventSystemHelper.Instance)
				EventSystemHelper.Instance.SetIntractable(true);

		}
		public static bool IsViewInProgress(SubView subView)
		{
			return inProgressView.Contains(subView);
		}

		public static bool IsViewInProgress(params SubView[] subViews)
		{
			for (int i = 0; i < subViews.Length; i++)
			{
				if (IsViewInProgress(subViews[i]))
					return true;
			}
			return false;
		}

		#endregion

	}
}
