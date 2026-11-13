using UnityEngine;
using UnityEngine.UI;

namespace com.otm.animation
{
	public class MotionImageRenderSetter : BaseMotionAnimation
	{
		#region private variables

		[SerializeField] private Image image;

		#endregion

		#region overrided methods
		public override void OnUpdate(float evolution) { }

		public override void SetValue<V>(V startValue, V endValue)
		{
			Sprite s = startValue as Sprite;
			if (s != null)
			{
				SetRender(s);
				return;
			}
			s = endValue as Sprite;
			if (s != null)
			{
				SetRender(s);
				return;
			}
			Image sr = startValue as Image;
			if (sr != null)
			{
				SetImageRenderer(sr);
				return;
			}
			sr = endValue as Image;
			if (sr != null)
			{
				SetImageRenderer(sr);
				return;
			}

		}

		#endregion

		#region private methods

		private void SetImageRenderer(Image render)
		{
			image.sprite = render.sprite;
			image.color = render.color;
			image.material = render.material;
			image.preserveAspect = render.preserveAspect;
		}

		private void SetRender(Sprite sprite)
		{
			if (image != null)
			{
				image.sprite = sprite;
			}
		}

		#endregion
	}
}
