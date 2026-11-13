using com.otm.manager;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.utility
{
	public class CameraMovement : SerializedManager<CameraMovement>
	{
		#region private variables

		[SerializeField] private Camera panningCamera;
		[SerializeField] private LayerMask wordMask;
		[SerializeField] private bool runTimeBoundry;
		[SerializeField] private float zOffset;
		[SerializeField, HideIf("$runTimeBoundry")] private BoxCollider2D boundry;
		[SerializeField, HideIf("$runTimeBoundry")] private Vector2 startPosition;
		[SerializeField, HideIf("$runTimeBoundry")] private Vector2 minMaxZoom;
		[SerializeField] private float defaultZoom;
		[SerializeField] private AnimationCurve positionAnimationCurve;
		[SerializeField] private AnimationCurve sizeAnimationCurve;
		[SerializeField] private List<BaseCameraMovement> cameraMovements = new List<BaseCameraMovement>();
		[SerializeField] private BaseCameraMovementValidation[] validation = new BaseCameraMovementValidation[0];

		private Vector2 minMaxOrthographicSize;
		private Vector2 maxBoundry;
		private Vector2 minBoundry;
		private Vector2 currentPos;
		private BaseCameraMovement movement;
		private Vector2 camPosition = Vector2.zero;
		private Coroutine animationCoroutine;
		private Vector3 localOffset;
		[ShowInInspector] private bool isCameraAnimationInProgress;

		#endregion

		#region properties

		[ShowInInspector] public bool IsMovementBlock { get; set; }

		#endregion

		#region overrided methods

		public override void Awake()
		{
			base.Awake();
			if (!runTimeBoundry)
			{
				SetCameraMovement();
				SetBoundery();
				SetZoom(Mathf.Clamp(defaultZoom, minMaxOrthographicSize.x, minMaxOrthographicSize.y));
				SetStartPosition();
				localOffset = new Vector3(boundry.bounds.center.x, boundry.bounds.center.y, zOffset);
				panningCamera.transform.localPosition = localOffset;
			}
		}

		#endregion

		#region unity callback

		private void Update()
		{
			if (!CanMove() || boundry == null)
				return;
			movement.OnUpdate();
			camPosition += movement.DoPanning();
			camPosition = camPosition.Clamp(currentPos * -1, currentPos);
			panningCamera.transform.parent.position = camPosition;
		}

		#endregion

		#region public methods

		public void SetCamera(BoxCollider2D boundryCollider, Vector2 startPosition, Vector2 minMaxZoom, float defaultZoom)
		{
			boundry = boundryCollider;
			this.startPosition = startPosition;
			this.minMaxZoom = minMaxZoom;
			this.defaultZoom = defaultZoom;
			SetCameraMovement();
			SetBoundery();
			SetZoom(Mathf.Clamp(defaultZoom, minMaxOrthographicSize.x, minMaxOrthographicSize.y));
			localOffset = new Vector3(boundry.bounds.center.x, boundry.bounds.center.y, zOffset);
			panningCamera.transform.localPosition = localOffset;
			SetStartPosition();
		}

		public void SetZoom(float zoom)
		{
			panningCamera.orthographicSize = zoom;
			CalculateCurrentPosition();
		}

		public void DoZoom(float zoom)
		{
			panningCamera.orthographicSize = Mathf.Clamp(panningCamera.orthographicSize - zoom, minMaxOrthographicSize.x, minMaxOrthographicSize.y);
			CalculateCurrentPosition();
		}

		public bool GetHitPoint(out Vector3 pos, Vector3 position)
		{
			RaycastHit2D hit2D = Physics2D.Raycast(panningCamera.ScreenToWorldPoint(position), Vector3.forward, Mathf.Infinity, wordMask);
			if (hit2D)
			{
				pos = hit2D.point;
				return true;
			}
			pos = Vector3.zero;
			return false;
		}

		public void SetCameraPositionAndSize(Vector2 position, float orthographicSize, Action onComplete = null)
		{
			if (animationCoroutine != null)
				return;
			animationCoroutine = StartCoroutine(DoCameraPositionAndSizeAnimation(position, orthographicSize, onComplete));
		}

		public void SetCameraPositionAndSize(Vector2 position, Vector2 size, Action onComplete = null)
		{
			if (animationCoroutine != null)
				return;
			float aspectRatio = panningCamera.aspect;
			float orthographicSize = size.y / 2f;
			if (size.x / aspectRatio > size.y)
				orthographicSize = size.x / (2f * aspectRatio);
			animationCoroutine = StartCoroutine(DoCameraPositionAndSizeAnimation(position, orthographicSize, onComplete));
		}

		public void SetCameraPositionAndSizeEditor(Vector2 position, Vector2 size)
		{
			float aspectRatio = panningCamera.aspect;
			float orthographicSize = size.y / 2f;
			if (size.x / aspectRatio > size.y)
				orthographicSize = size.x / (2f * aspectRatio);
			panningCamera.transform.parent.position = position;
			panningCamera.orthographicSize = orthographicSize;
		}


		#endregion

		#region private methods

		private bool CanMove()
		{
			if (IsMovementBlock || isCameraAnimationInProgress)
				return false;
			for (int i = 0; i < validation.Length; i++)
			{
				if (validation[i].IsMovementBlock())
					return false;
			}
			return true;
		}

		private void SetStartPosition()
		{
			startPosition.x -= localOffset.x;
			startPosition.y -= localOffset.y;
			camPosition = startPosition;
			panningCamera.transform.parent.position = camPosition;
		}

		private void CalculateCurrentPosition()
		{
			float zoomDiff = minMaxOrthographicSize.y - minMaxOrthographicSize.x;
			float currentOrthographicDiff = minMaxOrthographicSize.y - panningCamera.orthographicSize;
			currentPos.x = (minBoundry.x * currentOrthographicDiff) / zoomDiff;
			currentPos.y = (minBoundry.y * currentOrthographicDiff) / zoomDiff;
			currentOrthographicDiff = panningCamera.orthographicSize - minMaxOrthographicSize.x;
			currentPos.x += (maxBoundry.x * currentOrthographicDiff) / zoomDiff;
			currentPos.y += (maxBoundry.y * currentOrthographicDiff) / zoomDiff;
		}

		private void CalculateCurrentPosition(float orthographicSize)
		{
			float zoomDiff = minMaxOrthographicSize.y - minMaxOrthographicSize.x;
			float currentOrthographicDiff = minMaxOrthographicSize.y - orthographicSize;
			currentPos.x = (minBoundry.x * currentOrthographicDiff) / zoomDiff;
			currentPos.y = (minBoundry.y * currentOrthographicDiff) / zoomDiff;
			currentOrthographicDiff = orthographicSize - minMaxOrthographicSize.x;
			currentPos.x += (maxBoundry.x * currentOrthographicDiff) / zoomDiff;
			currentPos.y += (maxBoundry.y * currentOrthographicDiff) / zoomDiff;
		}

		private void SetBoundery()
		{
			float witdh = Screen.width;
			float screenRatio = witdh / Screen.height;
			minMaxOrthographicSize = minMaxZoom;
			Vector2 size = boundry.bounds.size;
			CalculateMaxZoomOut(screenRatio, size);
			minBoundry = CalculateOffset(screenRatio, minMaxOrthographicSize.x, size);
			maxBoundry = CalculateOffset(screenRatio, minMaxOrthographicSize.y, size);
		}

		private void CalculateMaxZoomOut(float screenRatio, Vector2 panningSize)
		{
			minMaxOrthographicSize.y = CalculateOrthographicSize(screenRatio, panningSize);
			minMaxOrthographicSize.y = Mathf.Clamp(minMaxOrthographicSize.y, minMaxOrthographicSize.y, minMaxZoom.y);
			panningCamera.orthographicSize = minMaxOrthographicSize.y;
		}

		private float CalculateOrthographicSize(float screenRatio, Vector2 size)
		{
			float height = size.x / screenRatio;
			if (height >= size.y)
				height = size.y;
			return height / 2f;
		}

		private Vector2 CalculateOffset(float screenRatio, float height, Vector2 panSize)
		{
			Vector2 offset = new Vector2();
			height *= 2;
			Bounds bounds = new Bounds(Vector3.zero, new Vector3(height * screenRatio, height, 0));
			float size = (panSize.x - bounds.size.x) / 2f;
			offset.x = size;
			offset.x = Mathf.Abs(Mathf.Clamp(offset.x, 0, size));
			size = ((panSize.y - bounds.size.y) / 2f);
			offset.y = size;
			offset.y = Mathf.Abs(Mathf.Clamp(offset.y, 0, size));
			return offset;
		}

		private void SetCameraMovement()
		{
			for (int i = 0; i < cameraMovements.Count; i++)
			{
				if (cameraMovements[i].IsValidMovement())
				{
					cameraMovements[i].Init(this);
					movement = cameraMovements[i];
					break;
				}
			}
		}

		#endregion

		#region coroutine
		IEnumerator DoCameraPositionAndSizeAnimation(Vector2 endPosition, float orthographicSize, Action onComplete = null)
		{
			float i = 0;
			float rate = 1f / 0.5f;
			float startSize = panningCamera.orthographicSize;
			Vector3 startPosition = camPosition;
			endPosition.x -= localOffset.x;
			endPosition.y -= localOffset.y;
			isCameraAnimationInProgress = true;
			orthographicSize = Mathf.Clamp(orthographicSize, minMaxZoom.x, minMaxZoom.y);
			while (i < 1f)
			{
				i += Time.deltaTime * rate;
				SetZoom(Mathf.Lerp(startSize, orthographicSize, sizeAnimationCurve.Evaluate(i)));
				camPosition = Vector2.Lerp(startPosition, endPosition, positionAnimationCurve.Evaluate(i));
				camPosition = camPosition.Clamp(currentPos * -1, currentPos);
				panningCamera.transform.parent.position = camPosition;
				yield return null;
			}
			isCameraAnimationInProgress = false;
			animationCoroutine = null;
			onComplete?.Invoke();
		}

		#endregion
	}
}