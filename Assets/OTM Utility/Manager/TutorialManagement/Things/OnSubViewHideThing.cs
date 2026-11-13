using com.otm.ui;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;

namespace com.otm.tutorial
{
	public class OnSubViewHideThing : TutorialThings
	{
		#region private variables
		[SerializeField, ValueDropdown("GetSubView")] private Type viewType;
		#endregion

		#region overrided methods
		public override void Active()
		{
			SubViewHelper.onViewHideEvent.RegisterEvent(OnViewShow);
		}

		public override void DeActive()
		{
			SubViewHelper.onViewHideEvent.DeRegisterEvent(OnViewShow);
		}

		public override void Stop()
		{
			SubViewHelper.onViewHideEvent.DeRegisterEvent(OnViewShow);
		}

		#endregion

		#region private methods

		private void OnViewShow(SubView view)
		{
			if (viewType == view.GetType())
				mono.MoveToNextStep();
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
	}
}
