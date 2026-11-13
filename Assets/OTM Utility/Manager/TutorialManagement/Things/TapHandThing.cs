using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.tutorial
{
	public class TapHandThing : TutorialThings
	{
		#region private variables

		[SerializeField] private TapHandConfig config;
		private TapHandView tapHandView;

		#endregion

		#region overrided methods
		public override void Active()
		{
			tapHandView = TutorialManager.Instance.GetView<TapHandView>();
			tapHandView.ShowView(config);
		}

		public override void DeActive()
		{
			tapHandView.HideView();
		}

		public override void Stop()
		{
			tapHandView.HideView();
		}

#if UNITY_EDITOR

		private TapHandView tapHandViewEditor;
		public override void ShowStepsInEditor()
		{
			base.ShowStepsInEditor();
			tapHandViewEditor = manager.GetViewEditor<TapHandView>();
			tapHandViewEditor?.ShowForEditor(config);
		}

		public override void OnUpdateThing()
		{
			tapHandViewEditor?.ShowForEditor(config);
		}

		public override void HideStepsInEditor()
		{
			tapHandViewEditor?.HideForEditor();
			base.HideStepsInEditor();
		}

#endif
		#endregion
	}

	public class WorldTapHandThing : TutorialThings
	{
		#region private variables

		[SerializeField] private WorldTapHandConfig config;
		[SerializeField] private bool useSharedObject;
		[SerializeField, ShowIf("useSharedObject")] private string sharedObjectKey;
		[SerializeField, ShowIf("useSharedObject")] private Vector3 offset;
		private WorldTapHandView tapHandView;

		#endregion

		#region overrided methods
		public override void Active()
		{
			tapHandView = TutorialManager.Instance.GetView<WorldTapHandView>();
			if (useSharedObject)
			{
				GameObject obj = mono.GetSharedObject(sharedObjectKey);
				if (obj != null)
				{
					config.position = obj.transform.position + offset;
				}
			}
			tapHandView.ShowView(config);
		}

		public override void DeActive()
		{
			tapHandView.HideView();
		}

		public override void Stop()
		{
			tapHandView.HideView();
		}

#if UNITY_EDITOR

		private WorldTapHandView tapHandViewEditor;
		public override void ShowStepsInEditor()
		{
			base.ShowStepsInEditor();
			tapHandViewEditor = manager.GetViewEditor<WorldTapHandView>();
			tapHandViewEditor?.ShowForEditor(config);
		}

		public override void OnUpdateThing()
		{
			tapHandViewEditor?.ShowForEditor(config);
		}

		public override void HideStepsInEditor()
		{
			tapHandViewEditor?.HideForEditor();
			base.HideStepsInEditor();
		}

#endif
		#endregion
	}
}
