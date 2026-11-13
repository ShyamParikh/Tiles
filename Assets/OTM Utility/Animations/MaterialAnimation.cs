using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.animation
{
	public class MaterialAnimation : BaseAnimation
	{
		#region private variables

		[SerializeField] private SpriteRenderer spriteRenderer;
		[SerializeField] private MaterialPropety[] propeties = new MaterialPropety[0];

		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStart;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnStop;
		[SerializeField, FoldoutGroup("Extra settings")] private bool resetOnOver;

		[ShowInInspector] private Material materialInstance;


		#endregion

		#region overrided methods

		public override void Init(MonoBehaviour group, bool useUnscaleTime = false)
		{
			base.Init(group, useUnscaleTime);
			materialInstance = spriteRenderer.material;
			for (int i = 0; i < propeties.Length; i++)
			{
				propeties[i].Init(materialInstance);
			}
		}

		protected override void OnAnimationStart()
		{
			if (resetOnStart)
				ResetAnimation();
			base.OnAnimationStart();
		}

		public override void Stop()
		{
			base.Stop();
			if (resetOnStop)
				ResetAnimation();
		}

		public override void OnUpdate(float evolution)
		{
			for (int i = 0; i < propeties.Length; i++)
			{
				propeties[i].Update(evolution);
			}
		}

		protected override void OnAnimationOver()
		{
			base.OnAnimationOver();
			if (resetOnOver)
				ResetAnimation();
		}

		public override void ResetAnimation()
		{
			base.ResetAnimation();
			ResetMaterialValue();
		}

		#endregion

		#region private methods

		private void ResetMaterialValue()
		{
			for (int i = 0; i < propeties.Length; i++)
			{
				propeties[i].Reset();
			}
		}

		#endregion

		private abstract class MaterialPropety
		{
			#region public variables
			public string propertyName;
			[SerializeField, HideReferenceObjectPicker] protected AnimationCurve curve = new AnimationCurve();
			protected Material material;
			#endregion

			#region virtual methods
			public virtual void Init(Material material)
			{
				this.material = material;
			}

			#endregion

			#region abstract Methods

			public abstract void Update(float evolution);
			public abstract void Reset();

			#endregion
		}

		private class ColorPropety : MaterialPropety
		{
			#region public variables
			public Color startColor = Color.white;
			public Color endColor = Color.white;

			#endregion

			#region overrided methods

			public override void Reset()
			{
				material.SetColor(propertyName, startColor);
			}

			public override void Update(float evolution)
			{
				material.SetColor(propertyName, Color.Lerp(endColor, startColor, curve.Evaluate(evolution)));
			}

			#endregion
		}

		private class FloatPropety : MaterialPropety
		{
			#region public variables
			public float startValue = 0;
			public float endValue = 1;
			#endregion

			#region overrided methods

			public override void Reset()
			{
				material.SetFloat(propertyName, startValue);
			}

			public override void Update(float evolution)
			{
				material.SetFloat(propertyName, Mathf.Lerp(startValue, endValue, curve.Evaluate(evolution)));
			}

			#endregion
		}

		private class IntPropety : MaterialPropety
		{
			#region public variables
			public int startValue = 0;
			public int endValue = 1;
			#endregion

			#region overrided methods

			public override void Reset()
			{
				material.SetInt(propertyName, startValue);
			}

			public override void Update(float evolution)
			{
				material.SetInt(propertyName, (int)Mathf.Lerp(startValue, endValue, curve.Evaluate(evolution)));
			}

			#endregion
		}

		private class OffsetPropety : MaterialPropety
		{
			#region public variables
			public Vector2 startValue = Vector2.zero;
			public Vector2 endValue = Vector2.one;
			#endregion

			#region overrided methods

			public override void Reset()
			{
				material.SetTextureOffset(propertyName, startValue);
			}

			public override void Update(float evolution)
			{
				material.SetTextureOffset(propertyName, Vector2.Lerp(startValue, endValue, curve.Evaluate(evolution)));
			}

			#endregion
		}

		private class ScalePropety : MaterialPropety
		{
			#region public variables
			public Vector2 startValue = Vector2.zero;
			public Vector2 endValue = Vector2.one;
			#endregion

			#region overrided methods

			public override void Reset()
			{
				material.SetTextureScale(propertyName, startValue);
			}

			public override void Update(float evolution)
			{
				material.SetTextureScale(propertyName, Vector2.Lerp(startValue, endValue, curve.Evaluate(evolution)));
			}

			#endregion
		}

		private class TexturePropety : MaterialPropety
		{
			#region public variables
			public Texture2D[] textures = new Texture2D[0];
			#endregion

			#region overrided methods

			public override void Reset()
			{
				if (textures.Length > 0)
					material.SetTexture(propertyName, textures[0]);
			}

			public override void Update(float evolution)
			{
				int index = (int)Mathf.Lerp(0, textures.Length, curve.Evaluate(evolution));
				index = Mathf.Clamp(index, 0, textures.Length - 1);
				material.SetTexture(propertyName, textures[index]);
			}

			#endregion
		}
	}
}
