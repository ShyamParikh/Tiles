using TMPro;
using UnityEngine;

namespace com.otm.ui
{
	[ExecuteAlways]
	[RequireComponent(typeof(TMP_Text))]
	[AddComponentMenu("OTM/UI/Curved Text")]
	public class CurvedText : MonoBehaviour
	{
		#region private variables

		[SerializeField] private TMP_Text textMesh;
		[SerializeField] private float radius = 500f;
		[SerializeField, Range(0f, 1f)] private float curveStrength = 1f;
		[SerializeField] private bool curveUp = true;
		[SerializeField] private bool updateEveryFrame;

		private bool isDirty;

		#endregion

		#region unity callback

		private void Awake()
		{
			SetTextMesh();
		}

		private void OnEnable()
		{
			SetTextMesh();
			TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
			SetDirty();
		}

		private void OnDisable()
		{
			TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
		}

		private void LateUpdate()
		{
			if (updateEveryFrame || isDirty)
				WarpText();
		}

		private void OnValidate()
		{
			SetTextMesh();
			SetDirty();
		}

		private void OnRectTransformDimensionsChange()
		{
			SetDirty();
		}

		#endregion

		#region public methods

		public void SetDirty()
		{
			isDirty = true;
		}

		public void WarpText()
		{
			if (textMesh == null)
				return;

			float safeRadius = Mathf.Max(0.01f, Mathf.Abs(radius));
			float strength = Mathf.Abs(curveStrength);
			if (Mathf.Approximately(strength, 0f))
			{
				textMesh.ForceMeshUpdate();
				isDirty = false;
				return;
			}

			textMesh.ForceMeshUpdate();
			TMP_TextInfo textInfo = textMesh.textInfo;
			if (textInfo == null || textInfo.characterCount == 0)
			{
				isDirty = false;
				return;
			}

			float direction = curveUp ? 1f : -1f;
			float radiusDirection = Mathf.Sign(radius);
			if (Mathf.Approximately(radiusDirection, 0f))
				radiusDirection = 1f;

			for (int i = 0; i < textInfo.characterCount; i++)
			{
				TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
				if (!characterInfo.isVisible)
					continue;

				int vertexIndex = characterInfo.vertexIndex;
				int materialIndex = characterInfo.materialReferenceIndex;
				Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

				Vector3 charMidBaseline = (vertices[vertexIndex] + vertices[vertexIndex + 2]) * 0.5f;
				float angle = (charMidBaseline.x / safeRadius) * strength * radiusDirection;
				float rotationAngle = angle * Mathf.Rad2Deg * direction;
				Vector3 curvedMidBaseline = new Vector3(
					Mathf.Sin(angle) * safeRadius,
					direction * (safeRadius - Mathf.Cos(angle) * safeRadius),
					charMidBaseline.z);

				Matrix4x4 matrix = Matrix4x4.TRS(
					curvedMidBaseline,
					Quaternion.Euler(0f, 0f, rotationAngle),
					Vector3.one);

				for (int j = 0; j < 4; j++)
					vertices[vertexIndex + j] = matrix.MultiplyPoint3x4(vertices[vertexIndex + j] - charMidBaseline);
			}

			for (int i = 0; i < textInfo.meshInfo.Length; i++)
			{
				TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
				meshInfo.mesh.vertices = meshInfo.vertices;
				textMesh.UpdateGeometry(meshInfo.mesh, i);
			}

			isDirty = false;
		}

		#endregion

		#region private methods

		private void SetTextMesh()
		{
			if (textMesh == null)
				textMesh = GetComponent<TMP_Text>();
		}

		private void OnTextChanged(Object changedObject)
		{
			if (changedObject == textMesh)
				SetDirty();
		}

		#endregion
	}
}
