using com.otm.animation;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

namespace com.otm.utility
{
	public class CustomButton : Button
	{
		#region private veriables
		private ButtonAnimation buttonAnimation;
		#endregion

		#region overrided methods

		protected override void Start()
		{
			base.Start();
			buttonAnimation = GetComponent<ButtonAnimation>();
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			if (buttonAnimation == null || !buttonAnimation.WaitForAnimationComplete)
			{
				CallEvent(eventData);
				return;
			}

			DoAnimation(eventData);
		}

		#endregion
		#region private methods
		private void DoAnimation(PointerEventData eventData)
		{
			if (buttonAnimation != null)
			{
				buttonAnimation.PlayAnimation(() => { CallEvent(eventData); });
				return;
			}
			CallEvent(eventData);
		}

		private void CallEvent(PointerEventData eventData)
		{
#if UNITY_EDITOR
			base.OnPointerClick(eventData);
#endif
#if !UNITY_EDITOR
			if (eventData.pointerId == 0)
			{
				base.OnPointerClick(eventData);
			}
#endif
		}

		#endregion
	}

}
