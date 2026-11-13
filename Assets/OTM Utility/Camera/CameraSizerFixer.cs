using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace com.otm.utility
{
	public class CameraSizerFixer : MonoBehaviour
	{
		#region private variables

		[SerializeField] private new Camera camera;
		[SerializeField] private Vector2 viewPort;
		[SerializeField] private bool userTransformAsPosition;
		[SerializeField, HideIf("userTransformAsPosition")] private Vector3 position;
		[SerializeField, ShowIf("userTransformAsPosition")] private Transform positionPoint;
		[SerializeField] private bool setManualy;
		[SerializeField, HideIf("setManualy")] private bool onAwake;
		[Header("Camera Animation")]
		[SerializeField] private float animationDuration;
		[SerializeField] private AnimationCurve animationCurve;

		private Coroutine animationCoroutine;

		#endregion

		#region unity callback

		private void Awake()
		{
			if (onAwake && !setManualy)
				SetCamera(viewPort, (userTransformAsPosition) ? positionPoint.position : position);
		}

		private void Start()
		{
			if (!onAwake && !setManualy)
				SetCamera(viewPort, (userTransformAsPosition) ? positionPoint.position : position);
		}

		private void OnValidate()
		{
			if (camera == null)
				camera = GetComponent<Camera>();
		}

		private void OnDrawGizmos()
		{
			Vector3 position = (userTransformAsPosition) ? positionPoint.position : this.position;
			Bounds bounds = new Bounds(position, viewPort);
			Gizmos.color = Color.red;
			Gizmos.DrawLine(new Vector3(bounds.min.x, bounds.max.y, 0), bounds.max);
			Gizmos.DrawLine(bounds.min, new Vector3(bounds.max.x, bounds.min.y, 0));
			Gizmos.DrawLine(bounds.min, new Vector3(bounds.min.x, bounds.max.y, 0));
			Gizmos.DrawLine(new Vector3(bounds.max.x, bounds.min.y, 0), bounds.max);
			SetCamera(viewPort, position);
		}

		#endregion

		#region public methods

		public void SetCamera(Vector2 size, bool doLerp = false)
		{
			SetCamera(size, (userTransformAsPosition) ? positionPoint.position : position, doLerp);
		}

		public void SetCamera(Vector3 position, bool doLerp = false)
		{
			SetCamera(viewPort, position, doLerp);
		}

		public void SetCamera(Vector2 size, Vector3 position, bool doLerp = false)
		{
			if (camera == null)
			{
				Debug.Log("Camera is null");
				return;
			}

			if (animationCoroutine != null)
				StopCoroutine(animationCoroutine);

			if (doLerp)
			{
				animationCoroutine = StartCoroutine(DoCameraAnimation(GetOrthographicSize(size), position));
				return;
			}
			camera.transform.position = position;
			camera.orthographicSize = GetOrthographicSize(size);
		}

		private float GetOrthographicSize(Vector2 size)
		{
			float screenRatio = (float)Screen.width / (float)Screen.height;
			Bounds bounds = new Bounds(position, size);
			float targetRatio = bounds.size.x / bounds.size.y;
			if (screenRatio >= targetRatio)
			{
				return bounds.size.y / 2;
			}
			float differenceInSize = targetRatio / screenRatio;
			return bounds.size.y / 2 * differenceInSize;
		}

		#endregion

		#region Coroutine

		IEnumerator DoCameraAnimation(float size, Vector3 position)
		{
			Vector3 startPosition = camera.transform.position;
			float startSize = camera.orthographicSize;
			float i = 0;
			float rate = 1f / animationDuration;
			while (i < 1)
			{
				i += Time.deltaTime * rate;
				camera.transform.position = Vector3.Lerp(startPosition, position, animationCurve.Evaluate(i));
				camera.orthographicSize = Mathf.Lerp(startSize, size, animationCurve.Evaluate(i));
				yield return null;
			}
		}

		#endregion
	}
}
