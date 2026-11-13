using com.otm.ui;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace com.otm.tutorial
{
	public class OnViewShowThing : TutorialThings
	{
		#region private variables
		[SerializeField, ValueDropdown("GetBaseView")] private Type viewType;
		[SerializeField] private bool waitForShowComplete;
		#endregion

		#region overrided methods
		public override void Active()
		{
			if (!ViewHelper.IsViewOpen(viewType))
			{
				ViewHelper.OnViewShowEvent.RegisterEvent(OnViewShow);
			}
			else
			{
				if (!waitForShowComplete)
					mono.MoveToNextStep();
				else
				{
					BaseView view = ViewHelper.GetOpneView(viewType);
					if (view != null)
						TutorialManager.Instance.StartCoroutine(WaitForViewShowDone(view));
					else
						mono.MoveToNextStep();
				}
			}
		}

		public override void DeActive()
		{
			ViewHelper.OnViewShowEvent.DeRegisterEvent(OnViewShow);
		}

		public override void Stop()
		{
			ViewHelper.OnViewShowEvent.DeRegisterEvent(OnViewShow);
		}

		#endregion

		#region private methods

		private void OnViewShow(BaseView baseView)
		{
			if (viewType == baseView.GetType())
			{
				if (!waitForShowComplete)
					mono.MoveToNextStep();
				else
					TutorialManager.Instance.StartCoroutine(WaitForViewShowDone(baseView));
			}
		}

		private Type[] GetBaseView()
		{
			Type[] q = typeof(BaseView).Assembly.GetTypes()
				.Where(x => !x.IsAbstract)
				.Where(x => !x.IsGenericTypeDefinition)
				.Where(x => typeof(BaseView).IsAssignableFrom(x)).ToArray();
			return q;
		}

		#endregion

		#region coroutine

		IEnumerator WaitForViewShowDone(BaseView baseView)
		{
			while (baseView.IsViewInProgress)
			{
				yield return null;
			}
			mono.MoveToNextStep();
		}

		#endregion

	}
}
