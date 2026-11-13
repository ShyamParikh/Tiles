using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.utility
{
	public static class CameraHelper
	{
		#region public static variables
		public static ActionSystem OnNewCameraRegister = new ActionSystem();
		#endregion
		#region private variables

		private static Dictionary<string, Camera> cameras = new Dictionary<string, Camera>();

		#endregion

		#region public methods

		public static void AddCamera(string id, Camera camera)
		{
			if (cameras.ContainsKey(id))
				cameras[id] = camera;
			else
				cameras.Add(id, camera);
			OnNewCameraRegister.Invoke();
		}

		public static void RemoveCamera(string id)
		{
			cameras.Remove(id);
		}

		public static Camera GetCamera(string id)
		{
			if (cameras.ContainsKey(id))
				return cameras[id];
			return null;
		}

		#endregion
	}
}