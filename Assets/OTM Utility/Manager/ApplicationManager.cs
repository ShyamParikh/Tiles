using com.otm.utility;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace com.otm.manager
{
	public class ApplicationManager : RunTimeManager<ApplicationManager>
	{
		#region public variables

		public ActionSystem OnAppPause = new ActionSystem();
		public ActionSystem OnAppResume = new ActionSystem();

		#endregion

		#region unity callback

		public void OnApplicationPause(bool pause)
		{
			if (pause)
				OnAppPause.Invoke();
			else
				OnAppResume.Invoke();
		}

		public override void OnDestroy()
		{
			OnAppPause?.ClearEvents();
			OnAppResume?.ClearEvents();
			base.OnDestroy();
		}

		#endregion

		#region public static methods

		public static void RestartGame(string startSceneName, string emptySceneName)
		{
			LoadScene(emptySceneName, () => { DestroyAllObjects(startSceneName); });
		}

		#endregion

		#region private static methods

		private static void DestroyAllObjects(string sceneName)
		{
			Object[] objects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
			for (int i = 0; i < objects.Length; i++)
			{
				GameObject o = (GameObject)objects[i];
				if (o != null && o.scene != null && !string.IsNullOrEmpty(o.scene.name) && o.scene.name.Equals("DontDestroyOnLoad"))
				{
					Destroy(o);
				}
			}
			LoadScene(sceneName);
		}

		private static void LoadScene(string sceneName, Action onSceneLoaingDone = null)
		{
			AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
			ValidationTask task = new ValidationTask();
			task.OnTaskComplete.RegisterEvent(() =>
			{
				onSceneLoaingDone?.Invoke();
			});
			task.Start(() =>
			{
				return !asyncOperation.isDone;
			});
		}
		#endregion
	}

}