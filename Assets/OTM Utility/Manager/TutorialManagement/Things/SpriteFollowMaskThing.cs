using System.Net.NetworkInformation;
using UnityEngine;

namespace com.otm.tutorial
{
	public class SpriteFollowMaskThing : TutorialThings
	{
		#region private variables

		[SerializeField] private TutorialFollowSpriteMaskConfig masking;
		[SerializeField] private string dataKey;
		private TutorialSpriteMaskView tutorialSpriteMaskView;

		#endregion

		#region overrided methods
		public override void Active()
		{
			GameObject obj = mono.GetSharedObject(dataKey);
			if (obj != null)
			{
				tutorialSpriteMaskView = TutorialManager.Instance.GetView<TutorialSpriteMaskView>();
				masking.target = obj.transform;
				tutorialSpriteMaskView.ShowView(masking);
			}
		}

		public override void DeActive()
		{
			if (tutorialSpriteMaskView != null)
				tutorialSpriteMaskView.HideView();
		}

		public override void Stop()
		{
			if (tutorialSpriteMaskView != null)
				tutorialSpriteMaskView.HideView();
		}

		#endregion
	}
}
