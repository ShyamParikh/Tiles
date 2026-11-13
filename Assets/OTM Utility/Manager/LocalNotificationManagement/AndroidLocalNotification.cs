#if UNITY_ANDROID && MOBILE_NOTIFICATION
using System;
using System.Collections.Generic;
using Unity.Notifications.Android;
using UnityEngine;
using UnityEngine.Android;

namespace com.otm.localNotification
{
	public class AndroidLocalnotification : BaseLocalNotification
	{
		#region private variables

		private AndroidNotificationChannel androidNotificationChannel;
		private bool isNotificationAllowed;
		private Dictionary<int, AndroidNoficationConfig> notifications = new Dictionary<int, AndroidNoficationConfig>();

		#endregion
		public AndroidLocalnotification(string channelId) : base(channelId)
		{
			isNotificationAllowed = false;
			if (Application.platform == RuntimePlatform.Android && GetApiLevel() >= 33)
				RequestPermission();
			else
				CreateChannel();
		}

		#region override methods
		public override void ClearAllNotificaiton()
		{
			if (!isNotificationAllowed)
				return;
			AndroidNotificationCenter.CancelAllNotifications();
		}

		public override void CreateChannel()
		{
			isNotificationAllowed = true;
			ClearAllNotificaiton();
			AndroidNotificationCenter.DeleteNotificationChannel(channelId);
			androidNotificationChannel = new AndroidNotificationChannel()
			{
				Id = channelId,
				Name = Application.productName,
				Description = Application.productName + " notification",
				Importance = Importance.Default,
			};
			AndroidNotificationCenter.RegisterNotificationChannel(androidNotificationChannel);
		}

		public override void RemoveNotification(int notificationId)
		{
			if (notifications.ContainsKey(notificationId))
				notifications.Remove(notificationId);
			AndroidNotificationCenter.CancelScheduledNotification(notificationId);
		}

		public override void ScheduleNotification(string title, string message, DateTime scheduleTime, int notificaiotnID, string smallIcon = "small_icon", string LargeIcon = "large_icon")
		{
			if (notifications.ContainsKey(notificaiotnID))
			{
				AndroidNoficationConfig config = notifications[notificaiotnID];
				config.title = title;
				config.message = message;
				config.scheduleTime = scheduleTime;
				config.smallIcon = smallIcon;
				config.LargeIcon = LargeIcon;
				config.notificaiotnID = notificaiotnID;
			}
			else
			{
				notifications.Add(notificaiotnID, new AndroidNoficationConfig()
				{
					title = title,
					message = message,
					scheduleTime = scheduleTime,
					smallIcon = smallIcon,
					LargeIcon = LargeIcon,
					notificaiotnID = notificaiotnID,
				});
			}

		}

		public override bool IsNotificationAllow()
		{
			return isNotificationAllowed;
		}

		public override void ScheduleAllNotification()
		{
			if (!isNotificationAllowed)
				return;
			foreach (var n in notifications)
			{
				if (n.Value.scheduleTime > DateTime.Now)
				{

					AndroidNotification androidNotification = new AndroidNotification();
					androidNotification.Title = n.Value.title;
					androidNotification.Text = n.Value.message;
					androidNotification.ShowTimestamp = false;
					androidNotification.SmallIcon = n.Value.smallIcon;
					androidNotification.LargeIcon = n.Value.LargeIcon;
					androidNotification.FireTime = n.Value.scheduleTime;
					androidNotification.ShouldAutoCancel = false;
					if (AndroidNotificationCenter.CheckScheduledNotificationStatus(n.Key) == NotificationStatus.Scheduled)
						AndroidNotificationCenter.CancelScheduledNotification(n.Key);
					AndroidNotificationCenter.SendNotificationWithExplicitID(androidNotification, channelId, n.Key);
				}
				else
				{
					AndroidNotificationCenter.CancelScheduledNotification(n.Key);
				}
			}
		}

		#endregion

		#region private methods
		private void OnPemissionGrant(string permission)
		{
			CreateChannel();
		}

		private void OnPemissionDeni(string permission)
		{
			Debug.Log("OnPemissionDeni " + permission);
		}

		private int GetApiLevel()
		{
			using var version = new AndroidJavaClass("android.os.Build$VERSION");
			return version.GetStatic<int>("SDK_INT");
		}

		private void RequestPermission()
		{
			if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
			{
				PermissionCallbacks callbacks = new PermissionCallbacks();
				callbacks.PermissionGranted += OnPemissionGrant;
				callbacks.PermissionDenied += OnPemissionDeni;
				Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS", callbacks);
			}
			else
			{
				CreateChannel();
			}
		}

		#endregion

		public class AndroidNoficationConfig
		{
			public string title;
			public string message;
			public DateTime scheduleTime;
			public int notificaiotnID;
			public string smallIcon = "small_icon";
			public string LargeIcon = "large_icon";
		}
	}
}
#endif
