using com.otm.utility;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.otm.manager
{
	public class ManagerLoader : SerializedMonoBehaviour
	{
		#region private variables

		[SerializeField] private float startProcessAmount;
		[SerializeField] private float maxProcessAmount;
		[SerializeField, OnValueChanged("DisableManager")]
		private List<ManagerInstanceLoader> preLoadingMangers = new List<ManagerInstanceLoader>();

		[SerializeField, OnValueChanged("DisableManager")]
		private List<ManagerInstanceLoader> managers = new List<ManagerInstanceLoader>();

		[SerializeField] private UnityEvent onLoadingStart = new UnityEvent();
		[SerializeField] private UnityEvent onLoadingOver = new UnityEvent();
		[SerializeField] private bool clearEventOnDestroy;
		[SerializeField] private float waitAfterPreloadingManager;

		[SerializeField] private bool showManagerLoadingLogs;

		#endregion

		#region public static variables

		public static readonly ActionSystem<float> OnProgress = new ActionSystem<float>();
		public static readonly ActionSystem OnManagerLoadingDone = new ActionSystem();

		#endregion

		#region unity callback

		public void Awake()
		{
			StartCoroutine(LoadManager());
		}

		public void OnDestroy()
		{
			if (clearEventOnDestroy)
				OnManagerLoadingDone.ClearEvents();
		}

		#endregion

		#region private methods

		private void DisableManager()
		{
			for (int i = 0; i < managers.Count; i++)
			{
				if (managers[i] != null)
					managers[i].gameObject.SetActive(false);
			}

			for (int i = 0; i < preLoadingMangers.Count; i++)
			{
				if (preLoadingMangers[i] != null)
					preLoadingMangers[i].gameObject.SetActive(false);
			}
		}

		#endregion

		#region Coroutine

		IEnumerator LoadManager()
		{
			yield return StartCoroutine(LoadPreManagers());
			if (waitAfterPreloadingManager > 0)
				yield return new WaitForSeconds(waitAfterPreloadingManager);
			onLoadingStart.Invoke();
			float progress = 0;
			float unitPorcess = (maxProcessAmount - startProcessAmount) / managers.Count;
			OnProgress.Invoke(progress);
			for (int i = 0; i < managers.Count; i++)
			{
				if (showManagerLoadingLogs)
					Debug.Log("===========================" + managers[i].name + "===========================");
				if (showManagerLoadingLogs)
					Debug.Log("Start");
				managers[i].StartPreExtraLoading();
				while (!managers[i].CanLoadingStart())
					yield return null;
				if (showManagerLoadingLogs)
					Debug.Log("PreLoadingDone");
				managers[i].gameObject.SetActive(true);
				while (!managers[i].IsLoaded())
					yield return null;
				if (showManagerLoadingLogs)
					Debug.Log("Complete");
				progress += unitPorcess;
				OnProgress.Invoke(progress + startProcessAmount);
				if (showManagerLoadingLogs)
					Debug.Log("===========================" + managers[i].name + "===========================");
			}
			if (showManagerLoadingLogs)
				Debug.Log("All Manager Loading Done");
			onLoadingOver.Invoke();
			OnManagerLoadingDone?.Invoke();
		}

		IEnumerator LoadPreManagers()
		{
			for (int i = 0; i < preLoadingMangers.Count; i++)
			{
				preLoadingMangers[i].gameObject.SetActive(true);
				yield return null;
			}
		}

		#endregion
	}
}