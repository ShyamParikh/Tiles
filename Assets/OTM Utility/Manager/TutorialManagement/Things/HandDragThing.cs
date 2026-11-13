using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.tutorial
{
	public class HandDragThing : TutorialThings
	{
		#region private variables

		[SerializeField] private WorldHandDragConfig config;
		private WorldHandDragView worldHandDragView;

		#endregion

		#region overrided methods
		public override void Active()
		{
			worldHandDragView = TutorialManager.Instance.GetView<WorldHandDragView>();
			worldHandDragView.ShowView(config);
		}

		public override void DeActive()
		{
			worldHandDragView.HideView();
		}

		public override void Stop()
		{
			worldHandDragView.HideView();
		}

		#endregion
	}
}
