using com.otm.manager;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.otm.manager
{
	public class CustomSceneManager : Manager<CustomSceneManager>
	{
		#region variables
		private string currentScene;
		#endregion

		#region public methods
		public void LoadScene(string sceneName, LoadSceneMode sceneMode, Action managerLoadingDone = null, Action sceneLoadingDone = null)
		{
			GC.Collect();
			ManagerLoader.OnManagerLoadingDone.RegisterEvent("SceneLoading", () =>
			{
				GC.Collect();
				managerLoadingDone?.Invoke();
				StartCoroutine(WaitFor(0.5f, DeRegisterSeceLoadingEvent));
			});

			StartCoroutine(LoadSceneAsync(sceneName, sceneMode, sceneLoadingDone));
		}

		public void UnloadScene(string sceneName, Action onUnloadingDone = null)
		{
			StartCoroutine(UnloadSceneAsync(sceneName, onUnloadingDone));
		}

		#endregion

		#region private methods
		private void DeRegisterSeceLoadingEvent()
		{
			ManagerLoader.OnManagerLoadingDone.DeRegisterEvent("SceneLoading");
		}

		#endregion

		#region coroutines

		private IEnumerator LoadSceneAsync(string sceneName, LoadSceneMode sceneMode, Action onLoadingDone)
		{
			currentScene = sceneName;
			AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, sceneMode);
			while (!operation.isDone)
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.5f);
			onLoadingDone?.Invoke();
		}

		private IEnumerator UnloadSceneAsync(string sceneName, Action onUnloadingDone)
		{
			currentScene = "";
			AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);
			while (operation == null || !operation.isDone)
			{
				yield return null;
			}

			yield return new WaitForSeconds(0.5f);
			onUnloadingDone?.Invoke();
		}

		private IEnumerator WaitFor(float time, Action action)
		{
			yield return new WaitForSeconds(time);
			action?.Invoke();
		}

		#endregion
	}

	public class SceneName
	{
		public const string GAME_PLAY_SCENE = "GamePlay";
	}
}
