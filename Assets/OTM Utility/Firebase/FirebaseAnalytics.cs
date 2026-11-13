#if OTM_Firebase
using Firebase.Analytics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using FireAnalytics = Firebase.Analytics.FirebaseAnalytics;

namespace com.otm.firebase
{
	public class FirebaseAnalytics : MonoBehaviour
	{
		#region PUBLIC_VARS

		[HideInInspector] public bool isInit = false;

		#endregion

		#region PUBLIC_FUNCTIONS

		public void Init()
		{
			FireAnalytics.SetAnalyticsCollectionEnabled(true);
			FireAnalytics.SetUserProperty(FireAnalytics.UserPropertySignUpMethod, "Guest");
			string userID = SystemInfo.deviceUniqueIdentifier;
			FireAnalytics.SetUserId(userID);
			FireAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 5, 0));
			isInit = true;
		}

		public void LogEvent(string eventName, Dictionary<string, string> data = null)
		{
			if (!isInit)
				return;
			if (data == null || data.Count <= 0)
				FireAnalytics.LogEvent(eventName);
			else
				FireAnalytics.LogEvent(eventName, ConvertToArrayOfParameters(data));
			if (FirebaseManager.DebugLog)
				Debug.Log("FIREBASE EVENT : " + eventName + "-" + JsonConvert.SerializeObject(data));
		}

		public void LogEvent(string eventName, Parameter[] data)
		{
			if (!isInit)
				return;
			if (data == null || data.Length <= 0)
				FireAnalytics.LogEvent(eventName);
			else
				FireAnalytics.LogEvent(eventName, data);
			if (FirebaseManager.DebugLog)
				Debug.Log("FIREBASE EVENT : " + eventName + "-" + JsonConvert.SerializeObject(data));
		}

		#endregion

		#region PRIVATE_FUNCTIONS

		private Parameter[] ConvertToArrayOfParameters(Dictionary<string, string> data)
		{
			Parameter[] parameters = new Parameter[data.Count];
			int count = 0;
			foreach (var item in data)
			{
				parameters[count] = new Parameter(item.Key, item.Value);
				count++;
			}

			return parameters;
		}

		#endregion
	}
}
#endif