using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.utility
{

	public class SystemUtility : MonoBehaviour
	{
		public static string GetDeviceId()
		{

#if UNITY_EDITOR || UNITY_ANDROID
			return SystemInfo.deviceUniqueIdentifier;
#endif
#if UNITY_IOS
			if (UnityEngine.iOS.Device.advertisingTrackingEnabled)
				return UnityEngine.iOS.Device.advertisingIdentifier;
			else
				return "";
#endif

		}

		public static string GetAdvetismentId()
		{
#if UNITY_EDITOR
			return UnityEngine.WSA.Application.advertisingIdentifier;
#endif
#if UNITY_ANDROID && !UNITY_EDITOR

            string advertisingID = "";
            try
            {
                AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaClass client = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
                AndroidJavaObject adInfo = client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", currentActivity);
                advertisingID = adInfo.Call<string>("getId").ToString();
            }
            catch (Exception)
            {
            }
            return advertisingID;
#endif
#if UNITY_IOS
			if (UnityEngine.iOS.Device.advertisingTrackingEnabled)
				return UnityEngine.iOS.Device.advertisingIdentifier;
			else
				return "";
#endif
		}

		public static string GetVersion()
		{
			return Application.version;
		}

		public static string GetGameHash()
		{
			Hash128 hash = new Hash128();
			hash.Append(GetVersion());
			return hash.ToString();
		}

		public static string GetHash(string data)
		{
			Hash128 hash = new Hash128();
			hash.Append(data);
			return hash.ToString();
		}
	}

}