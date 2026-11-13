using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.otm.manager
{
	public class EventSystemHelper : Manager<EventSystemHelper>
	{
		#region private variables

		[SerializeField] private EventSystem eventSystem;
		[SerializeField] private new Camera camera;

		private bool isPointerWasOnUI;
		private bool isPointerWasOnUILate;
		private WaitForSeconds wait;
		private Coroutine coroutine;

		#endregion

		#region propertices

		public static EventSystem EventSystem => Instance.eventSystem;
		public static Camera Camera => Instance.camera;
		public static bool IsPointerWasOnUi => Instance.isPointerWasOnUI;
		public static bool IsPointerWasOnUiLate => Instance.isPointerWasOnUILate;

		#endregion

		#region unity callback

		public void Start()
		{
			isPointerWasOnUI = false;
			wait = new WaitForSeconds(0.1f);
		}

		public void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				isPointerWasOnUI = IsPointerOverUIObject();
				isPointerWasOnUILate = isPointerWasOnUI;
				if (coroutine != null)
					StopCoroutine(coroutine);
			}

			if (Input.GetMouseButtonUp(0))
			{
				isPointerWasOnUI = false;

			}
		}

		private void LateUpdate()
		{
			if (coroutine != null)
				StopCoroutine(coroutine);
			coroutine = StartCoroutine(LateInvoke());
		}

		#endregion

		#region public methods

		public bool IsPointerOverUIObject()
		{
			PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem);
			eventDataCurrentPosition.position = Input.mousePosition;
			List<RaycastResult> results = new List<RaycastResult>();
			EventSystem.RaycastAll(eventDataCurrentPosition, results);
			return results.Count > 0;
		}

		public bool IsPointerOverUIObject(Vector3 position)
		{
			PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem);
			eventDataCurrentPosition.position = position;
			List<RaycastResult> results = new List<RaycastResult>();
			EventSystem.RaycastAll(eventDataCurrentPosition, results);
			return results.Count > 0;
		}

		public void SetIntractable(bool intractable)
		{
			EventSystem.enabled = intractable;
		}

		public void UpdateCamera(Camera camera)
		{
			this.camera = camera;
		}

		#endregion

		IEnumerator LateInvoke()
		{
			yield return wait;
			isPointerWasOnUILate = false;
		}
	}
}