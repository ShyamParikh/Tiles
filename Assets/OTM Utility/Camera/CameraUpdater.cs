using UnityEngine;

namespace com.otm.utility
{
	public class CameraUpdater : MonoBehaviour
	{
		#region private variables
		[SerializeField] private string cameraId;
		[SerializeField] private Vector3 offeset;

		private Camera cam;
		private Camera targetCam;

		#endregion

		#region unity callback

		private void Start()
		{
			targetCam = CameraHelper.GetCamera(cameraId);
			cam = GetComponent<Camera>();
			CameraHelper.OnNewCameraRegister.RegisterEvent(OnNewCameraRigister);
		}

		private void OnDestroy()
		{
			CameraHelper.OnNewCameraRegister.DeRegisterEvent(OnNewCameraRigister);
		}

		private void Update()
		{
			if (cam == null || targetCam == null)
				return;
			if (cam.transform.root != null)
				targetCam.transform.position = cam.transform.root.TransformPoint(cam.transform.localPosition) + offeset;
			else
				targetCam.transform.position = cam.transform.position + offeset;
			targetCam.orthographicSize = cam.orthographicSize;
			targetCam.pixelRect = cam.pixelRect;
		}

		#endregion

		#region private methods

		private void OnNewCameraRigister()
		{
			targetCam = CameraHelper.GetCamera(cameraId);
		}

		#endregion
	}
}
