using com.otm.utility;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace com.otm.manager
{
	public class InternetManager : SerializedManager<InternetManager>
	{
		#region private veriabels

		[SerializeField] private float interval;
		[SerializeField] private string connectivityCheckUrl = "https://www.google.com/generate_204";
		[SerializeField] private int connectivityCheckTimeout = 5;
		[SerializeField] private bool doRepeatConnectionCheck;
		[SerializeField, ShowIf("doRepeatConnectionCheck")] private IInternetConnectionCheckCondition[] connectionCheckConditions = new IInternetConnectionCheckCondition[0];
		private WaitForSecondsRealtime waitForSecondsRealtime;
		private Coroutine checkInternetInRepeatCoroutine;

		#endregion

		#region propertices
		public bool IsInternetAvailable { get; private set; }

		#endregion

		#region public static variables

		public static ActionSystem OnConnectionLost = new ActionSystem();

		#endregion

		#region unity callback

		private void Start()
		{
			waitForSecondsRealtime = new WaitForSecondsRealtime(interval);
			InitConditions();
			CheckForConnectionInRepeat();
		}

		public override void OnDestroy()
		{
			if (checkInternetInRepeatCoroutine != null)
				StopCoroutine(checkInternetInRepeatCoroutine);
			base.OnDestroy();
			OnConnectionLost.ClearEvents();
		}

		#endregion

		#region private methods

		private void InitConditions()
		{
			for (int i = 0; i < connectionCheckConditions.Length; i++)
			{
				connectionCheckConditions[i].Init();
			}
		}

		#endregion

		#region public methods
		public bool IsReachableToNetwork()
		{
			return Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
		}

		public void CheckForConnection(Action<bool> onCheckDone)
		{
			StartCoroutine(CheckForConnectionCoroutine(onCheckDone));
		}

		public void CheckForConnection(Action onConnectionRestore)
		{
			StartCoroutine(CheckForConnectionUntilRestore(onConnectionRestore));
		}

		public void CheckForConnectionInRepeat()
		{
			if (doRepeatConnectionCheck)
				checkInternetInRepeatCoroutine = StartCoroutine(CheckInternetInRepeat());
			OnLoadingDone();
		}

		public void StopInternetCheckingInRepeat()
		{
			if (checkInternetInRepeatCoroutine != null)
				StopCoroutine(checkInternetInRepeatCoroutine);
		}

		#endregion

		#region Coroutine

		IEnumerator CheckForConnectionCoroutine(Action<bool> onCheckDone)
		{
			if (!IsReachableToNetwork())
			{
				SetConnectionStatus(false, onCheckDone);
				yield break;
			}

			using (UnityWebRequest request = UnityWebRequest.Get(connectivityCheckUrl))
			{
				request.timeout = connectivityCheckTimeout > 0 ? connectivityCheckTimeout : 5;
				yield return request.SendWebRequest();
				bool isConnected = request.result == UnityWebRequest.Result.Success && request.responseCode >= 200 && request.responseCode < 400;
				SetConnectionStatus(isConnected, onCheckDone);
			}
		}

		IEnumerator CheckInternetInRepeat()
		{
			yield return waitForSecondsRealtime;
			for (int i = 0; i < connectionCheckConditions.Length; i++)
			{
				if (!connectionCheckConditions[i].CanCheckConnection())
				{
					CheckForConnectionInRepeat();
					yield break;
				}
			}
			bool isInternetAvailable = false;
			yield return StartCoroutine(CheckForConnectionCoroutine(b => isInternetAvailable = b));
			if (isInternetAvailable)
				CheckForConnectionInRepeat();
			else
			{
				OnConnectionLost.Invoke();
				CheckForConnection(CheckForConnectionInRepeat);
			}
		}

		IEnumerator CheckForConnectionUntilRestore(Action onConnectionRestore)
		{
			float retryInterval = interval > 0 ? interval : 1f;
			WaitForSecondsRealtime waitForRetry = new WaitForSecondsRealtime(retryInterval);

			while (true)
			{
				bool isInternetAvailable = false;
				yield return StartCoroutine(CheckForConnectionCoroutine(b => isInternetAvailable = b));
				if (isInternetAvailable)
				{
					onConnectionRestore?.Invoke();
					yield break;
				}

				yield return waitForRetry;
			}
		}

		private void SetConnectionStatus(bool isInternetAvailable, Action<bool> onCheckDone)
		{
			IsInternetAvailable = isInternetAvailable;
			onCheckDone?.Invoke(isInternetAvailable);
		}

		#endregion

	}
}
