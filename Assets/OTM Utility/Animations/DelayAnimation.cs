using UnityEngine;
using UnityEngine.Events;

namespace com.otm.animation
{
	public class DelayAnimation : BaseAnimation
	{
		#region private variables
		[SerializeField] private UnityEvent onDelayOver = new UnityEvent();


		#endregion
		#region overrided methods
		public override void OnUpdate(float evolution) { }

		protected override void OnAnimationOver()
		{
			base.OnAnimationOver();
			onDelayOver.Invoke();
		}

		#endregion


	}
}
