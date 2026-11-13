using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.utility
{
    public class CameraRegister : MonoBehaviour
    {
        #region private variables

        [SerializeField] private string cameraId;

        private new Camera camera;

        #endregion

        #region unity callback

        private void Start()
        {
            camera = GetComponent<Camera>();
            if (camera != null)
                CameraHelper.AddCamera(cameraId, camera);
        }

        private void OnDestroy()
        {
            if (camera != null)
                CameraHelper.RemoveCamera(cameraId);
        }

        #endregion
    }
}