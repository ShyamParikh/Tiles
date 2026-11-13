using TMPro;
using UnityEngine;

namespace com.otm.animation
{
	public class MotionTextSetter : BaseMotionAnimation
	{
		#region private variables

		[SerializeField] private TMP_Text text;

		#endregion

		#region overrided methods
		public override void OnUpdate(float evolution) { }

		public override void SetValue<V>(V startValue, V endValue)
		{
			string str = startValue as string;
			if (!string.IsNullOrEmpty(str))
			{
				text.text = str;
				return;
			}
			str = endValue as string;
			if (!string.IsNullOrEmpty(str))
				text.text = str;
		}

		#endregion

	}
}
