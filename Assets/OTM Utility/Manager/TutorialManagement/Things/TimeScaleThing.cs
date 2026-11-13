using UnityEngine;

namespace com.otm.tutorial
{
	public class TimeScaleThing : TutorialThings
	{
		#region private variables
		[SerializeField] private float timeScale;
		[SerializeField] private bool resetOnDisable;
		#endregion

		#region overrided methods
		public override void Active()
		{
			Time.timeScale = timeScale;
		}

		public override void DeActive()
		{
			if (resetOnDisable)
				Time.timeScale = 1;
		}

		public override void Stop()
		{
			Time.timeScale = 1;
		}
		#endregion
	}
}
