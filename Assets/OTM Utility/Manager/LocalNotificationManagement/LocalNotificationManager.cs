using com.otm.localNotification;
using System;
using UnityEngine;

namespace com.otm.manager
{
	public class LocalNotificationManager : Manager<LocalNotificationManager>
	{
		#region private variables
		[SerializeField] private string channelName;
		private BaseLocalNotification localNotification;

		#endregion

		#region override methods

		public override void Awake()
		{
			base.Awake();
#if MOBILE_NOTIFICATION
#if UNITY_ANDROID
			localNotification = new AndroidLocalnotification(channelName);
#endif
#if UNITY_IOS
			localNotification = new IOSLocalNotification(channelName);
#endif
#endif
		}

		//uncomment this code for local notification testing
		//private void Start()
		//{
		//	for (int i = 1; i <= 10; i++)
		//	{
		//		localNotification.ScheduleNotification("Title " + i, "Message " + i, DateTime.Now.AddSeconds(i * i), 1000 + i);
		//	}
		//}

		private void OnApplicationPause(bool pause)
		{
			if (pause)
				localNotification?.ScheduleAllNotification();
			else
				localNotification?.ClearAllNotificaiton();
		}

		#endregion

		#region public methods
		public void ScheduleNotification(string title, string message, DateTime scheduleTime, int notificaitonId, string smallIcon = "small_icon", string LargeIcon = "large_icon")
		{
			localNotification?.ScheduleNotification(title, message, scheduleTime, notificaitonId, smallIcon, LargeIcon);
		}

		public void ClearAllNotificaiton()
		{
			localNotification?.ClearAllNotificaiton();
		}

		public void RemoveNotification(int notificaitonId)
		{
			localNotification?.RemoveNotification(notificaitonId);
		}

		#endregion

		#region private methods

		#endregion
	}


	#region public enums

	public enum NotificationIds
	{
		HEART_NOTIFICATION_ID = 1001,
	}

	#endregion

}
