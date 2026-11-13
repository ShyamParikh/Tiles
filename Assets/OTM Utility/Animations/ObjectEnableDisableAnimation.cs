using UnityEngine;

namespace com.otm.animation
{
	public class ObjectEnableDisableAnimation : BaseAnimation
	{
		#region private variables

		[SerializeField] private GameObject obj;
		[SerializeField] private bool onAnimationStart;
		[SerializeField] private bool onAnimationOver;

		#endregion

		#region overrided methods

		protected override void OnAnimationStart()
		{
			base.OnAnimationStart();
			obj.SetActive(onAnimationStart);
		}

		protected override void OnAnimationOver()
		{
			base.OnAnimationOver();
			obj.SetActive(onAnimationOver);
		}

		public override void OnUpdate(float evolution)
		{

		}

		public override void SetToStartValue()
		{
			base.SetToStartValue();
			obj.SetActive(onAnimationStart);
		}
		public override void SetToEndValue()
		{
			base.SetToEndValue();
			obj.SetActive(onAnimationOver);
		}
		#endregion
	}
}
