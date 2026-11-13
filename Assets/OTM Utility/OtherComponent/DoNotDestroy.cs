using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.utility
{
    public class DoNotDestroy : MonoBehaviour
    {

        #region unity callback

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        #endregion
    }
}