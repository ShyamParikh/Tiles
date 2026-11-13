using com.otm.ui;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;

namespace com.otm.tutorial
{
	public class OnViewHideThing : TutorialThings
	{
		#region private variables
		[SerializeField, ValueDropdown("GetBaseView")] private Type viewType;
		#endregion

		#region overrided methods
		public override void Active()
		{
			ViewHelper.OnViewHideEvent.RegisterEvent(OnViewShow);
		}

		public override void DeActive()
		{
			ViewHelper.OnViewHideEvent.DeRegisterEvent(OnViewShow);
		}

		public override void Stop()
		{
			ViewHelper.OnViewHideEvent.DeRegisterEvent(OnViewShow);
		}

		#endregion

		#region private methods

		private void OnViewShow(BaseView view)
		{
			if (viewType == view.GetType())
				mono.MoveToNextStep();
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
	}
}
