using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.otm.utility
{
	[ExecuteInEditMode]
	public class SpriteRenderAlphaGroup : SerializedMonoBehaviour
	{
		#region private variable

		[SerializeField] private Dictionary<SpriteRenderer, float> spriteRenderers = new Dictionary<SpriteRenderer, float>();
		[SerializeField] private bool preserveAlpha;
		[SerializeField, Range(0, 1)] private float alpha;

		private float alphaValue;


		#region propertices

		public float Alpha
		{
			get { return alpha; }
			set { alpha = value; }
		}

		#endregion

		#endregion

		#region unity callback
		private void Update()
		{
			UpdateAlpha();
		}

		#endregion

		#region private methods
		private void UpdateAlpha()
		{
			if (alphaValue == alpha)
				return;

			alphaValue = alpha;

			foreach (var item in spriteRenderers)
			{
				if (preserveAlpha)
					item.Key.color = item.Key.color.SetAlpha(item.Value * alpha);
				else
					item.Key.color = item.Key.color.SetAlpha(alpha);
			}

		}

		[Button]
		private void SetSpriteRenderers()
		{
			List<SpriteRenderer> childerns = GetComponentsInChildren<SpriteRenderer>(true).ToList();
			SpriteRenderer sp = GetComponent<SpriteRenderer>();
			if (sp != null && !childerns.Contains(sp))
				childerns.Add(sp);

			for (int i = 0; i < childerns.Count; i++)
			{
				if (!spriteRenderers.ContainsKey(childerns[i]))
					spriteRenderers.Add(childerns[i], childerns[i].color.a);
			}
		}

		#endregion
	}
}
