using com.otm.manager;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.utility
{
	public class MobileCameraMovement : BaseCameraMovement
	{
		#region private variables
		private int touchZeroId;
		private int touchOneId;
		private Vector3 touchZeroStart;
		private Vector3 touchOneStart;
		private bool wasPointerOverUI;
		private bool panningStarted;
		private bool isPanning;
		#endregion

		#region overrided methods
		public override bool IsValidMovement()
		{
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
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

		#endregion

		#region private methods

		private void HandleContinue()
		{

			if (Input.touchCount == 2)
			{
				OnTwoFingerTouchHandleContinue();
			}
			else if (Input.touchCount == 1)
			{
				OnSingleFingerHandleContinue();
			}
			else if (Input.touchCount == 0)
			{
				touchZeroId = -1;
				touchOneId = -1;
				wasPointerOverUI = false;
			}
		}

		private void OnTwoFingerTouchHandleContinue()
		{
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);
			if (!wasPointerOverUI)
			{
				touchZeroId = touchZero.fingerId;
				touchOneId = touchOne.fingerId;
				GetHitPoint(out touchZeroStart, touchZero.position);
				GetHitPoint(out touchOneStart, touchOne.position);
				Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
				Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
				float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
				float currentMagnitude = (touchZero.position - touchOne.position).magnitude;
				float difference = currentMagnitude - prevMagnitude;
				if (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
					DoZoom(difference * 0.01f);
			}
		}

		private void OnSingleFingerHandleContinue()
		{
			Touch touch = Input.GetTouch(0);
			if (touchZeroId == -1 && touchOneId == -1 && touch.phase == TouchPhase.Began)
			{
				wasPointerOverUI = EventSystemHelper.Instance.IsPointerOverUIObject(touch.position);
				if (!wasPointerOverUI)
					GetHitPoint(out touchOneStart, touch.position);
			}
			if (!wasPointerOverUI)
			{
				startPosition = (touch.fingerId == touchZeroId) ? touchZeroStart : touchOneStart;
				GetHitPoint(out Vector3 pos, Input.mousePosition);
				if (lerp > 0)
					moveDirection = Vector3.Lerp(Vector3.zero, startPosition - pos, Time.deltaTime * lerp);
				else
					moveDirection = startPosition - pos;
			}
		}

		private void HandleContinueWithUI()
		{
			if (Input.touchCount == 2)
			{
				OnTwoFingerHandleContinueWithUI();
			}
			else if (Input.touchCount == 1)
			{
				OnSingleFingerHandleContinueWithUI();
			}
			else if (Input.touchCount == 0)
			{
				touchZeroId = -1;
				touchOneId = -1;
				isPanning = false;
				panningStarted = false;
			}
		}

		private void OnTwoFingerHandleContinueWithUI()
		{
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);
			if (panningStarted)
			{
				touchZeroId = touchZero.fingerId;
				touchOneId = touchOne.fingerId;
				GetHitPoint(out touchZeroStart, touchZero.position);
				GetHitPoint(out touchOneStart, touchOne.position);
				Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
				Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
				float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
				float currentMagnitude = (touchZero.position - touchOne.position).magnitude;
				float difference = currentMagnitude - prevMagnitude;
				if (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
					DoZoom(difference * 0.01f);
			}
		}

		private void OnSingleFingerHandleContinueWithUI()
		{
			Touch touch = Input.GetTouch(0);
			if (touchZeroId == -1 && touchOneId == -1 && touch.phase == TouchPhase.Began)
				isPanning = true;
			if (!isPanning)
				return;
			if (panningStarted && EventSystemHelper.Instance.IsPointerOverUIObject())
			{
				panningStarted = false;
			}
			else if (!panningStarted)
			{
				if (GetHitPoint(out startPosition, touch.position))
					panningStarted = true;
			}

			if (panningStarted && GetHitPoint(out Vector3 pos, touch.position))
			{
				if (lerp > 0)
					moveDirection = Vector3.Lerp(Vector3.zero, startPosition - pos, Time.deltaTime * lerp);
				else
					moveDirection = startPosition - pos;
			}
		}

		#endregion
	}
}
