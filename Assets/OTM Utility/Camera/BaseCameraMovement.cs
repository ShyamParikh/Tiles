using UnityEngine;

namespace com.otm.utility
{
	public abstract class BaseCameraMovement
	{
		#region protected variables
		[SerializeField] protected PanningType panningType;
		[SerializeField] protected float lerp = 10;
		[SerializeField] protected float decelerationRate = 5;
		[SerializeField] protected float zoomSensitivity = 1;

		protected CameraMovement cameraMovement;
		protected Vector3 startPosition;
		protected Vector3 moveDirection;


		#endregion

		#region abstract methods
		public abstract bool IsValidMovement();

		#endregion

		#region virtual methods

		public virtual void Init(CameraMovement camera)
		{
			cameraMovement = camera;
		}

		public virtual void OnPanningStart() { }
		public virtual void OnUpdate() { }
		public virtual void DoZoom(float increment)
		{
			cameraMovement.DoZoom(increment);
		}
		public virtual Vector2 DoPanning()
		{
			moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, Time.deltaTime * decelerationRate);
			return moveDirection;
		}
		public virtual void OnPanningOver() { }

		#endregion

		#region protected methods
		protected bool GetHitPoint(out Vector3 pos, Vector3 position)
		{
			if (cameraMovement)
				return cameraMovement.GetHitPoint(out pos, position);
			pos = Vector3.zero;
			return false;
		}
		#endregion
	}

	public enum PanningType
	{
		Continue,
		ContinueWithUI
	}
}

