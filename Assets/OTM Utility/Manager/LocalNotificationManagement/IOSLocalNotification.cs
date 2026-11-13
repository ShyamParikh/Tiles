#if UNITY_IOS
using com.otm.utility;
using System;
using System.Collections.Generic;
using Unity.Notifications.iOS;
using UnityEngine;

namespace com.otm.localNotification
{
	public class IOSLocalNotification : BaseLocalNotification
	{
		#region privet variables

		private bool isNotificationAllowed;
		private Dictionary<int, IOSNoficationConfig> notifications = new Dictionary<int, IOSNoficationConfig>();

		#endregion
		#region override methods
		public IOSLocalNotification(string channelId) : base(channelId)
		{
			RequestAuthorization();
		}

		public override void ClearAllNotificaiton()
		{
			if (isNotificationAllowed)
			{
				iOSNotificationCenter.RemoveAllScheduledNotifications();
				iOSNotificationCenter.RemoveAllDeliveredNotifications();
			}
		}

		public override void CreateChannel()
		{
			ClearAllNotificaiton();
		}

		public override bool IsNotificationAllow()
		{
			return isNotificationAllowed;
		}

		public override void RemoveNotification(int notificationId)
		{
			if (notifications.ContainsKey(notificationId))
				notifications.Remove(notificationId);
			iOSNotificationCenter.RemoveScheduledNotification(notificationId.ToString());
		}

		public override void ScheduleAllNotification()
		{
			if (!isNotificationAllowed)
				return;
			foreach (var n in notifications)
			{
				if (n.Value.scheduleTime > DateTime.Now)
				{

					iOSNotification IOSNotification = new iOSNotification();
					IOSNotification.Title = n.Value.title;
					IOSNotification.Identifier = n.Value.notificaiotnID.ToString();
					IOSNotification.Subtitle = n.Value.message;
					IOSNotification.ShowInForeground = true;
					IOSNotification.ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound);
					IOSNotification.CategoryIdentifier = channelId;
					IOSNotification.ThreadIdentifier = "thread1";
					IOSNotification.Trigger = GetNotificationTrigger(n.Value.scheduleTime);
					iOSNotificationCenter.ScheduleNotification(IOSNotification);
				}
				else
				{
					iOSNotificationCenter.RemoveScheduledNotification(n.Key.ToString());
				}
			}
			Debug.Log("All Notification Schedule");

		}

		public override void ScheduleNotification(string title, string message, DateTime scheduleTime, int notificaiotnID, string smallIcon = "small_icon", string LargeIcon = "large_icon")
		{
			if (notifications.ContainsKey(notificaiotnID))
			{
				IOSNoficationConfig config = notifications[notificaiotnID];
				config.title = title;
				config.message = message;
				config.scheduleTime = scheduleTime;
				config.smallIcon = smallIcon;
				config.LargeIcon = LargeIcon;
				config.notificaiotnID = notificaiotnID;
			}
			else
			{
				notifications.Add(notificaiotnID, new IOSNoficationConfig()
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
		#endregion

		#region private methods

		private iOSNotificationTrigger GetNotificationTrigger(DateTime dateTime)
		{
			return new iOSNotificationTimeIntervalTrigger()
			{
				Repeats = false,
				TimeInterval = dateTime - DateTime.Now
			};
			//return new iOSNotificationCalendarTrigger()
			//{
			//	UtcTime = false,
			//	Repeats = false,
			//	Year = dateTime.Year,
			//	Month = dateTime.Month,
			//	Day = dateTime.Day,
			//	Hour = dateTime.Hour,
			//	Minute = dateTime.Minute,
			//	Second = dateTime.Second,
			//};
		}

		private void RequestAuthorization()
		{
			var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
			AuthorizationRequest request = new AuthorizationRequest(authorizationOption, true);
			ValidationTask validationTask = new ValidationTask();
			validationTask.OnTaskComplete.RegisterEvent(() =>
			{
				if (string.IsNullOrEmpty(request.Error))
					isNotificationAllowed = request.Granted;
				else
					isNotificationAllowed = false;
				if (isNotificationAllowed)
					CreateChannel();
			});
			validationTask.Start(() =>
			{
				return !request.IsFinished;
			});

		}

		#endregion
	}

	public class IOSNoficationConfig
	{
		public string title;
		public string message;
		public DateTime scheduleTime;
		public int notificaiotnID;
		public string smallIcon = "small_icon";
		public string LargeIcon = "large_icon";
	}
}
#endif
