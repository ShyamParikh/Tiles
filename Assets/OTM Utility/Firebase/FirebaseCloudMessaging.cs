#if OTM_Firebase
using Firebase.Messaging;
using UnityEngine;
using UnityEngine.Android;

namespace com.otm.firebase
{
	public class FirebaseCloudMessaging : MonoBehaviour
	{
#region PUBLIC_VARS

		[HideInInspector] public bool isInit = false;

#endregion

#region private variables

		[SerializeField] private string topicName = "HotelStar";

#endregion

#region public methods

		public void Init()
		{
			FirebaseMessaging.TokenReceived += OnTokenReceive;
			FirebaseMessaging.MessageReceived += OnMessageReceived;
			isInit = true;
		}

		public void OnDestroy()
		{
			FirebaseMessaging.TokenReceived -= OnTokenReceive;
			FirebaseMessaging.MessageReceived -= OnMessageReceived;
		}

		public void Subscribe()
		{
			FirebaseMessaging.SubscribeAsync(topicName);
		}

		public void Unsubscribe()
		{
			FirebaseMessaging.UnsubscribeAsync(topicName);
		}

#endregion

#region private methods

		private void OnTokenReceive(object sender, TokenReceivedEventArgs token)
		{
			Subscribe();
			isInit = true;
			Debug.Log("Received Registration Token: " + token.Token);
			RequestPermission();

		}

		private void RequestPermission()
		{
			if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
			{
				PermissionCallbacks callbacks = new PermissionCallbacks();
				Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS", callbacks);
			}
		}

		public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
		{
			Debug.Log("Received a new message from: " + e.Message.From);
		}
#endregion
	}
}
#endif