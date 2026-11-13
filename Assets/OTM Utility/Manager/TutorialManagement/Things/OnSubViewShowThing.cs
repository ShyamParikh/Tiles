using com.otm.ui;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace com.otm.tutorial
{
	public class OnSubViewShowThing : TutorialThings
	{
		#region private variables
		[SerializeField, ValueDropdown("GetSubView")] private Type viewType;
		[SerializeField] private bool waitForShowComplete;
		#endregion

		#region overrided methods
		public override void Active()
		{
			if (!SubViewHelper.IsViewOpen(viewType))
			{
				SubViewHelper.onViewShowEvent.RegisterEvent(OnViewShow);
			}
			else
			{
				if (!waitForShowComplete)
					mono.MoveToNextStep();
				else
				{
					SubView view = SubViewHelper.GetOpneView(viewType);
					if (view != null)
						TutorialManager.Instance.StartCoroutine(WaitForViewShowDone(view));
					else
						mono.MoveToNextStep();
				}
			}

		}

		public override void DeActive()
		{
			SubViewHelper.onViewShowEvent.DeRegisterEvent(OnViewShow);
		}

		public override void Stop()
		{
			SubViewHelper.onViewShowEvent.DeRegisterEvent(OnViewShow);
		}

		#endregion

		#region private methods

		private void OnViewShow(SubView subView)
		{
			if (viewType == subView.GetType())
			{
				if (!waitForShowComplete)
					mono.MoveToNextStep();
				else
					TutorialManager.Instance.StartCoroutine(WaitForViewShowDone(subView));
			}
		}

		private Type[] GetSubView()
		{
			Type[] q = typeof(SubView).Assembly.GetTypes()
				.Where(x => !x.IsAbstract)
				.Where(x => !x.IsGenericTypeDefinition)
				.Where(x => typeof(SubView).IsAssignableFrom(x)).ToArray();
			return q;
		}

		#endregion

		#region coroutine

		IEnumerator WaitForViewShowDone(SubView subView)
		{
			while (subView.IsViewInProgress)
			{
				yield return null;
			}
			mono.MoveToNextStep();
		}

		#endregion
	}
}
