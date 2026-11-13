using com.otm.ui;
using UnityEngine;

namespace com.otm.tutorial
{
	public class WorldTapHandView : BaseView
	{
		#region overrided methods

		protected override void OnViewHideDone()
		{
			base.OnViewHideDone();
			Destroy(gameObject);
		}

		#endregion

		#region public methods

		public void ShowView(WorldTapHandConfig tapHandConfig)
		{
			base.ShowView();
			transform.position = tapHandConfig.position;
			transform.eulerAngles = Vector3.forward * tapHandConfig.rotation;
		}

		public void ShowForEditor(WorldTapHandConfig tapHandConfig)
		{
			ShowEditor();
			transform.position = tapHandConfig.position;
			transform.eulerAngles = Vector3.forward * tapHandConfig.rotation;
		}

		public void HideForEditor()
		{
			HideEditor();
		}

		#endregion
	}

	public struct WorldTapHandConfig
	{
		public Vector2 position;
		public float rotation;
	}
}
