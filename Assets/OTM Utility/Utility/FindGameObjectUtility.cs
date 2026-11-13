using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.otm.utility
{
	public static class FindGameObjectUtility
	{
		public static GameObjectPathMapping[] GetGameObjectPath(GameObject gameObject, out string sceneName)
		{
			List<GameObjectPathMapping> path = new List<GameObjectPathMapping>();
			if (gameObject == null)
			{
				sceneName = "";
				return new GameObjectPathMapping[0];
			}
			sceneName = gameObject.scene.name;
			Transform parent = gameObject.transform.parent;
			path.Add(new GameObjectPathMapping(gameObject.name, gameObject.transform.GetSiblingIndex()));
			while (parent != null)
			{
				path.Insert(0, new GameObjectPathMapping(parent.name, parent.GetSiblingIndex()));
				parent = parent.parent;
			}
			return path.ToArray();
		}

		public static GameObject FindGameObject(GameObjectPathMapping[] path, string sceneName)
		{
			if (path.Length <= 0)
				return null;
			Scene scene = SceneManager.GetSceneByName(sceneName);
			if (scene == null || !scene.isLoaded)
				return null;
			GameObject[] gameObjects = scene.GetRootGameObjects();
			GameObject obj = FindRelatedGameObject(gameObjects, path[0].gameObjectName, path[0].siblingIndex);
			if (obj == null)
				return null;
			Transform lastObject = obj.transform;
			for (int i = 1; i < path.Length; i++)
			{
				if (lastObject.childCount <= path[i].siblingIndex)
					return null;
				Transform t = lastObject.GetChild(path[i].siblingIndex);
				if (!t.name.Equals(path[i].gameObjectName))
				{
					t = FindChild(lastObject, path[i].gameObjectName);
					if (t == null)
						return null;
				}
				lastObject = t;
			}
			return lastObject.gameObject;
		}

		private static GameObject FindRelatedGameObject(GameObject[] gameObjects, string name, int siblingIndex)
		{
			for (int i = 0; i < gameObjects.Length; i++)
			{
				if (gameObjects[i].name.Equals(name))
					return gameObjects[i];
			}
			return null;
		}

		private static Transform FindChild(Transform parent, string childName)
		{
			Transform t = null;
			for (int i = 0; i < parent.childCount; i++)
			{
				t = parent.GetChild(i);
				if (t.name.Equals(childName))
					break;
			}
			return t;
		}

	}

	[HideReferenceObjectPicker]
	public class GameObjectPathMapping
	{
		public string gameObjectName;
		public int siblingIndex;

		public GameObjectPathMapping(string gameObjectName, int siblingIndex)
		{
			this.gameObjectName = gameObjectName;
			this.siblingIndex = siblingIndex;
		}
	}
}
