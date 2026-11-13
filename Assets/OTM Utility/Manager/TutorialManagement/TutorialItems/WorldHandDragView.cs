using com.otm.ui;
using System.Collections;
using UnityEngine;

namespace com.otm.tutorial
{
	public class WorldHandDragView : BaseView
	{
		#region private variables

		[SerializeField] private AnimationCurve animationCurve;
		[SerializeField] private float dragDuration;
		[SerializeField] private float resetDuration;
		private Coroutine handAnimationCoroutine;

		#endregion

		#region overrided methods

		public override void HideView()
		{
			if (handAnimationCoroutine != null)
				StopCoroutine(handAnimationCoroutine);
			base.HideView();
		}

		#endregion

		#region public methods
		public void ShowView(WorldHandDragConfig worldHandDragConfig)
		{
			base.ShowView();
			transform.eulerAngles = Vector3.forward * worldHandDragConfig.rotation;
			handAnimationCoroutine = StartCoroutine(DoDragHandAnimation(worldHandDragConfig.startPosition, worldHandDragConfig.endPostion));
		}

		#endregion

		#region coroutine

		IEnumerator DoDragHandAnimation(Vector2 startPostion, Vector2 endPosition)
		{
			while (true)
			{
				float i = 0;
				float rate = 1f / dragDuration;
				while (i < 1f)
				{
					i += Time.unscaledDeltaTime * rate;
					transform.position = Vector2.Lerp(startPostion, endPosition, animationCurve.Evaluate(i));
					yield return null;
				}

				i = 0;
				rate = 1f / resetDuration;
				while (i < 1f)
				{
					i += Time.unscaledDeltaTime * rate;
					transform.position = Vector2.Lerp(endPosition, startPostion, animationCurve.Evaluate(i));
					yield return null;
				}
			}
		}

		#endregion
	}

	public struct WorldHandDragConfig
	{
		public Vector2 startPosition;
		public Vector2 endPostion;
		public float rotation;
	}
}
