using com.otm.manager;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.camera
{
	public class CameraSizeSetter : Manager<CameraSizeSetter>
	{
		#region private variables
		[SerializeField] private Camera cam;
		[SerializeField] private Vector2 position;
		[SerializeField] private Vector2 size;
		[SerializeField] private bool isPortrait;
		[SerializeField] private bool useAdaptiveFit;
		[SerializeField] private bool showGizmos;
		#endregion

		#region unity callback

		private void Start()
		{
			SetCamera();
		}

		private void OnDrawGizmos()
		{
			if (!showGizmos)
				return;
			Bounds area = new Bounds(position, new Vector3(size.x, size.y, 0));
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(new Vector3(area.min.x, area.max.y, 0), area.max);
			Gizmos.DrawLine(area.min, new Vector3(area.max.x, area.min.y, 0));
			Gizmos.DrawLine(area.min, new Vector3(area.min.x, area.max.y, 0));
			Gizmos.DrawLine(new Vector3(area.max.x, area.min.y, 0), area.max);
		}
		#endregion

		#region private methods
		[Button]
		private void SetCamera()
		{
			cam.rect = new Rect(0, 0, 1f, 1f);
			float width = Screen.width;
			float screenRatio = width / Screen.height;

			if (useAdaptiveFit)
				cam.orthographicSize = CalculateAdaptiveOrthographicSize(screenRatio, size);
			else
				cam.orthographicSize = CalculateOrthographicSize(screenRatio, size);
		}

		private float CalculateOrthographicSize(float screenRatio, Vector2 size)
		{
			if (isPortrait)
				return size.y * 0.5f;
			return size.x / (screenRatio * 2f);
		}

		private float CalculateAdaptiveOrthographicSize(float screenRatio, Vector2 size)
		{
			float targetRatio = size.x / size.y;

			float sizeForHeight = size.y * 0.5f;
			float sizeForWidth = size.x / (screenRatio * 2f);

			// Use the larger value to ensure entire area fits on all resolutions
			if (screenRatio < targetRatio)
				return sizeForWidth;
			else
				return sizeForHeight;
		}

		#endregion
	}
}
