using System;
using System.Collections.Generic;

namespace com.otm.ui
{
	public abstract class QueueView : BaseView
	{
		#region private variables

		private List<Action> viewList = new List<Action>();

		#endregion

		#region propertices

		public bool HasQueue => viewList.Count >= 2;

		#endregion

		#region overrided methods
		protected override void OnViewHideDone()
		{
			base.OnViewHideDone();
			if (viewList.Count > 0)
				viewList.RemoveAt(0);
			ShowPendingView();
		}

		#endregion

		#region virtual methods

		public virtual void ShowView(Action action = null)
		{
			if (AddToQueue(action))
			{
				ShowPendingView();
			}
		}

		#endregion

		#region private variables

		private bool AddToQueue(Action action)
		{
			viewList.Add(action);
			if (viewList.Count <= 1)
				return true;
			return false;
		}

		private void ShowPendingView()
		{
			if (viewList.Count <= 0)
				return;
			if (viewList.Count > 0)
			{
				viewList[0]?.Invoke();
				base.ShowView();
			}
		}

		#endregion
	}
}
