using UnityEngine;

namespace com.otm.animation
{
	public class MotionSpriteRenderSetter : BaseMotionAnimation
	{
		#region private variables

		[SerializeField] private SpriteRenderer spriteRenderer;

		#endregion

		#region overrided methods
		public override void OnUpdate(float evolution) { }

		public override void SetValue<V>(V startValue, V endValue)
		{
			SpriteRenderer sr = startValue as SpriteRenderer;
			if (sr != null)
			{
				SetSpriteRenderer(sr);
				return;
			}
			sr = endValue as SpriteRenderer;
			if (sr != null)
			{
				SetSpriteRenderer(sr);
				return;
			}
			Sprite s = startValue as Sprite;
			if (s != null)
			{
				SetRender(s);
				return;
			}
			s = endValue as Sprite;
			SetRender(s);
		}

		#endregion

		#region private methods

		private void SetSpriteRenderer(SpriteRenderer render)
		{
			spriteRenderer.sprite = render.sprite;
			spriteRenderer.material = render.material;
			spriteRenderer.color = render.color;
			spriteRenderer.flipX = render.flipX;
			spriteRenderer.flipY = render.flipY;
			spriteRenderer.drawMode = render.drawMode;
			spriteRenderer.sortingLayerName = render.sortingLayerName;
			spriteRenderer.sortingOrder = render.sortingOrder;
		}

		private void SetRender(Sprite sprite)
		{
			if (spriteRenderer != null)
				spriteRenderer.sprite = sprite;
		}

		#endregion
	}
}
