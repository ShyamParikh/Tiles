using TMPro;
using UnityEngine;

namespace com.otm.ui.extension
{
	public class CurveText : MonoBehaviour
	{
		#region private variables
		[SerializeField] private AnimationCurve curve = new AnimationCurve();
		[SerializeField] private AnimationCurve rotationCurve = new AnimationCurve();
		[SerializeField] private float scale = 1;
		[SerializeField] private float rotationScale = 1;
		[SerializeField] private TMP_Text textMesh;
		#endregion

		#region unity callback

		private void Start()
		{
			WarpText();
		}

		private void Update()
		{
			WarpText();
		}

		private void OnValidate()
		{
			if (textMesh == null)
				textMesh = GetComponent<TMP_Text>();
			WarpText();
		}

		#endregion

		#region private methods
		private void WarpText()
		{
			if (textMesh == null)
				return;
			// Force the text mesh to generate its geometry
			textMesh.ForceMeshUpdate();
			TMP_TextInfo textInfo = textMesh.textInfo;
			if (textInfo == null || textInfo.characterCount == 0) return;

			for (int i = 0; i < textInfo.characterCount; i++)
			{
				TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

				if (!charInfo.isVisible)
					continue;

				// Get the vertices of the character
				int vertexIndex = charInfo.vertexIndex;
				int materialIndex = charInfo.materialReferenceIndex;

				Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

				// Calculate the midpoint of the character
				Vector3 charMidBaseline = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;

				// Offset vertices to center them around the midpoint
				Vector3 offset = charMidBaseline;
				vertices[vertexIndex + 0] -= offset;
				vertices[vertexIndex + 1] -= offset;
				vertices[vertexIndex + 2] -= offset;
				vertices[vertexIndex + 3] -= offset;

				float curveOffset = curve.Evaluate(Mathf.InverseLerp(-1, textInfo.characterCount, i)) * scale;

				float rotationAngle = Mathf.Atan(rotationCurve.Evaluate(Mathf.InverseLerp(-1, textInfo.characterCount, i))) * Mathf.Rad2Deg;
				Quaternion rotation = Quaternion.Euler(0, 0, -rotationAngle * rotationScale);

				vertices[vertexIndex + 0] = rotation * vertices[vertexIndex + 0];
				vertices[vertexIndex + 1] = rotation * vertices[vertexIndex + 1];
				vertices[vertexIndex + 2] = rotation * vertices[vertexIndex + 2];
				vertices[vertexIndex + 3] = rotation * vertices[vertexIndex + 3];

				// Offset vertices by the curve
				Vector3 curveAdjustment = new Vector3(0, curveOffset, 0);
				vertices[vertexIndex + 0] += curveAdjustment;
				vertices[vertexIndex + 1] += curveAdjustment;
				vertices[vertexIndex + 2] += curveAdjustment;
				vertices[vertexIndex + 3] += curveAdjustment;

				// Return vertices back to their original position
				vertices[vertexIndex + 0] += offset;
				vertices[vertexIndex + 1] += offset;
				vertices[vertexIndex + 2] += offset;
				vertices[vertexIndex + 3] += offset;
				//amount += rate;
			}

			// Apply changes to the mesh
			for (int i = 0; i < textInfo.meshInfo.Length; i++)
			{
				TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
				meshInfo.mesh.vertices = meshInfo.vertices;
				textMesh.UpdateGeometry(meshInfo.mesh, i);
			}
		}

		#endregion
	}
}
