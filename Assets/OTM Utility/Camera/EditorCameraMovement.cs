using com.otm.manager;
using UnityEngine;

namespace com.otm.utility
{
	public class EditorCameraMovement : BaseCameraMovement
	{
		#region private variables
		private bool wasPointerOverUI;
		private bool isPanning;
		private bool panningStarted;
		#endregion

		#region overrided methods
		public override bool IsValidMovement()
		{
#if UNITY_EDITOR
			return true;
#else
			return false;
#endif
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			switch (panningType)
			{
				case PanningType.Continue:
					HandleContinue();
					break;
				case PanningType.ContinueWithUI:
					HandleContinueWithUI();
					break;
			}
		}

		public override void OnPanningStart()
		{
			if (GetHitPoint(out Vector3 pos, Input.mousePosition))
				startPosition = pos;
		}

		public override void OnPanningOver()
		{
			if (decelerationRate <= 0)
				moveDirection = Vector3.zero;
		}

		#endregion

		#region private methods

		private void HandleContinue()
		{
			if (Input.GetMouseButtonDown(0))
			{
				wasPointerOverUI = EventSystemHelper.Instance.IsPointerOverUIObject();
				if (!wasPointerOverUI)
					OnPanningStart();
			}
			else if (Input.GetMouseButton(0))
			{
				if (!wasPointerOverUI && GetHitPoint(out Vector3 pos, Input.mousePosition))
				{
					if (lerp > 0)
						moveDirection = Vector3.Lerp(Vector3.zero, startPosition - pos, Time.deltaTime * lerp);
					else
						moveDirection = startPosition - pos;
				}
			}
			else if (Input.GetMouseButtonUp(0))
			{
				if (!wasPointerOverUI)
					OnPanningOver();
				wasPointerOverUI = false;
			}

			if (!wasPointerOverUI)
				DoZoom(Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity);
		}


		private void HandleContinueWithUI()
		{
			if (Input.GetMouseButtonDown(0))
			{
				isPanning = true;
				panningStarted = false;
			}
			else if (isPanning && Input.GetMouseButton(0))
			{
				if (EventSystemHelper.Instance.IsPointerOverUIObject())
				{
					OnPanningOver();
					panningStarted = false;
				}
				else if (!panningStarted)
				{
					panningStarted = true;
					OnPanningStart();
				}

				if (panningStarted && GetHitPoint(out Vector3 pos, Input.mousePosition))
				{
					if (lerp > 0)
						moveDirection = Vector3.Lerp(Vector3.zero, startPosition - pos, Time.deltaTime * lerp);
					else
						moveDirection = startPosition - pos;
				}
			}
			else if (Input.GetMouseButtonUp(0))
			{
				isPanning = false;
			}

			if (isPanning)
				DoZoom(Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity);
		}

		#endregion
	}
}
