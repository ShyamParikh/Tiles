using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.localNotification
{
	public abstract class BaseLocalNotification
	{
		#region protected variables

		protected string channelId;

		#endregion
		public BaseLocalNotification(string channelId)
		{
			this.channelId = channelId;
		}

		#region abstract methods

		public abstract void ScheduleNotification(string title, string message, DateTime scheduleTime, int notificaiotnID, string smallIcon = "small_icon", string LargeIcon = "large_icon");

		public abstract void CreateChannel();

		public abstract void ClearAllNotificaiton();

		public abstract bool IsNotificationAllow();

		public abstract void ScheduleAllNotification();

		public abstract void RemoveNotification(int notificationId);


		#endregion
	}
}