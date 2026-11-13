using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.component
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class SpriteRenderMaterialComponent : MonoBehaviour
	{
		#region private variables

		[SerializeField] private Material material;
		[SerializeField] SpriteRenderer spriteRenderer;

		#endregion

		#region public methods

		public void SetMaterial()
		{
			if (spriteRenderer != null && material != null)
				spriteRenderer.material = Instantiate(material);
		}

		#endregion
	}
}
