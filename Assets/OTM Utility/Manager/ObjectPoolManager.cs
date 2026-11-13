using System.Collections.Generic;
using UnityEngine;

namespace com.otm.manager
{
	public sealed class ObjectPoolManager : Manager<ObjectPoolManager>
	{
		public enum StartupPoolMode
		{
			Awake,
			Start,
			CallManually
		};

		[System.Serializable]
		public class StartupPool
		{
			public int size;
			public string objectUniqueName;
			public GameObject prefab;
		}

		static List<GameObject> tempList = new List<GameObject>();

		Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>();
		Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>();
		Dictionary<string, GameObject> pooledObjectNameMapping = new Dictionary<string, GameObject>();
		HashSet<GameObject> unkownObject = new HashSet<GameObject>();

		public StartupPoolMode startupPoolMode;
		public StartupPool[] startupPools;

		bool startupPoolsCreated;

		public override void Awake()
		{
			base.Awake();
			if (startupPoolMode == StartupPoolMode.Awake)
				CreateStartupPools();
		}

		void Start()
		{
			if (startupPoolMode == StartupPoolMode.Start)
				CreateStartupPools();
		}

		public static void CreateStartupPools()
		{
			if (!Instance.startupPoolsCreated)
			{
				Instance.startupPoolsCreated = true;
				var pools = Instance.startupPools;
				if (pools != null && pools.Length > 0)
					for (int i = 0; i < pools.Length; ++i)
						CreatePool(pools[i].prefab, pools[i].objectUniqueName, pools[i].size);
			}
		}

		public static void CreatePool<T>(T prefab, string objectName, int initialPoolSize) where T : Component
		{
			CreatePool(prefab.gameObject, objectName, initialPoolSize);
		}

		public static void CreatePool(GameObject prefab, string objectName, int initialPoolSize)
		{
			if (prefab == null || Instance.pooledObjects.ContainsKey(prefab))
				return;
			if (!Instance.pooledObjectNameMapping.ContainsKey(objectName))
				Instance.pooledObjectNameMapping.Add(objectName, prefab);
			else
				Debug.LogWarning("Same object name exist is pool");
			var list = new List<GameObject>();
			Instance.pooledObjects.Add(prefab, list);
			bool active = prefab.activeSelf;
			prefab.SetActive(false);
			Transform parent = Instance.transform;
			for (int i = 0; i < initialPoolSize; i++)
			{
				var obj = Instantiate(prefab);
				obj.transform.SetParent(parent, false);
				list.Add(obj);
				IObjectPool objectPool = obj.GetComponent<IObjectPool>();
				objectPool?.InitObject();
			}
			prefab.SetActive(active);
		}

		public static T Spawn<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
		{
			return Spawn(prefab.gameObject, parent, position, rotation).GetComponent<T>();
		}

		public static T Spawn<T>(string objectName, Transform parent, Vector3 position, Quaternion rotation) where T : Component
		{
			if (Instance.pooledObjectNameMapping.ContainsKey(objectName))
				return Spawn(Instance.pooledObjectNameMapping[objectName], parent, position, rotation).GetComponent<T>();
			return null;
		}

		public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
		{
			return Spawn(prefab.gameObject, null, position, rotation).GetComponent<T>();
		}

		public static T Spawn<T>(string objectName, Vector3 position, Quaternion rotation) where T : Component
		{
			if (Instance.pooledObjectNameMapping.ContainsKey(objectName))
				return Spawn(Instance.pooledObjectNameMapping[objectName], null, position, rotation).GetComponent<T>();
			return null;
		}

		public static T Spawn<T>(T prefab, Transform parent, Vector3 position) where T : Component
		{
			return Spawn(prefab.gameObject, parent, position, Quaternion.identity).GetComponent<T>();
		}

		public static T Spawn<T>(string objectName, Transform parent, Vector3 position) where T : Component
		{
			if (Instance.pooledObjectNameMapping.ContainsKey(objectName))
				return Spawn(Instance.pooledObjectNameMapping[objectName], parent, position).GetComponent<T>();
			return null;
		}

		public static T Spawn<T>(T prefab, Vector3 position) where T : Component
		{
			return Spawn(prefab.gameObject, null, position, Quaternion.identity).GetComponent<T>();
		}

		public static T Spawn<T>(string objectName, Vector3 position) where T : Component
		{
			if (Instance.pooledObjectNameMapping.ContainsKey(objectName))
				return Spawn(Instance.pooledObjectNameMapping[objectName], position).GetComponent<T>();
			return null;
		}

		public static T Spawn<T>(T prefab, Transform parent) where T : Component
		{
			return Spawn(prefab.gameObject, parent, Vector3.zero, Quaternion.identity).GetComponent<T>();
		}

		public static T Spawn<T>(string objectName, Transform parent) where T : Component
		{
			if (Instance.pooledObjectNameMapping.ContainsKey(objectName))
				return Spawn(Instance.pooledObjectNameMapping[objectName], parent).GetComponent<T>();
			return null;
		}

		public static T Spawn<T>(string objectName) where T : Component
		{
			if (Instance.pooledObjectNameMapping.ContainsKey(objectName))
				return Spawn(Instance.pooledObjectNameMapping[objectName], null, Vector3.zero, Quaternion.identity).GetComponent<T>();
			return null;
		}

		public static GameObject Spawn(string objectName)
		{
			if (Instance.pooledObjectNameMapping.ContainsKey(objectName))
				return Spawn(Instance.pooledObjectNameMapping[objectName], null, Vector3.zero, Quaternion.identity);
			return null;
		}

		public static GameObject Spawn(string objectName, Transform parent)
		{
			if (Instance.pooledObjectNameMapping.ContainsKey(objectName))
				return Spawn(Instance.pooledObjectNameMapping[objectName], parent);
			return null;
		}

		public static T Spawn<T>(T prefab) where T : Component
		{
			return Spawn(prefab.gameObject, null, Vector3.zero, Quaternion.identity).GetComponent<T>();
		}

		public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
		{
			List<GameObject> list;
			Transform trans;
			GameObject obj;
			IObjectPool objectPool;
			if (Instance.pooledObjects.TryGetValue(prefab, out list))
			{
				obj = null;
				if (list.Count > 0)
				{
					while (obj == null && list.Count > 0)
					{
						obj = list[0];
						list.RemoveAt(0);
					}

					if (obj != null)
					{
						trans = obj.transform;
						trans.SetParent(parent, false);
						trans.localPosition = position;
						trans.localRotation = rotation;
						obj.SetActive(true);
						Instance.spawnedObjects.Add(obj, prefab);
						objectPool = obj.GetComponent<IObjectPool>();
						objectPool?.OnSpwan();
						return obj;
					}
				}

				obj = (GameObject)Object.Instantiate(prefab);
				trans = obj.transform;
				trans.SetParent(parent, false);
				trans.localPosition = position;
				trans.localRotation = rotation;
				obj.SetActive(true);
				objectPool = obj.GetComponent<IObjectPool>();
				objectPool?.InitObject();
				objectPool?.OnSpwan();
				Instance.spawnedObjects.Add(obj, prefab);
				return obj;
			}
			else
			{
				obj = (GameObject)Object.Instantiate(prefab);
				trans = obj.transform;
				trans.SetParent(parent, false);
				trans.localPosition = position;
				trans.localRotation = rotation;
				objectPool = obj.GetComponent<IObjectPool>();
				objectPool?.InitObject();
				objectPool?.OnSpwan();
				obj.SetActive(true);
				Instance.unkownObject.Add(obj);
				return obj;
			}
		}

		public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position)
		{
			return Spawn(prefab, parent, position, Quaternion.identity);
		}

		public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			return Spawn(prefab, null, position, rotation);
		}

		public static GameObject Spawn(GameObject prefab, Transform parent)
		{
			return Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
		}

		public static GameObject Spawn(GameObject prefab, Vector3 position)
		{
			return Spawn(prefab, null, position, Quaternion.identity);
		}

		public static GameObject Spawn(GameObject prefab)
		{
			return Spawn(prefab, null, Vector3.zero, Quaternion.identity);
		}

		public static void Recycle<T>(T obj) where T : Component
		{
			Recycle(obj.gameObject);
		}

		public static void Recycle(GameObject obj)
		{
			if (Instance.spawnedObjects.ContainsKey(obj))
			{
				Recycle(obj, Instance.spawnedObjects[obj]);
			}
			else if (Instance.unkownObject.Contains(obj))
			{
				Instance.unkownObject.Remove(obj);
				Destroy(obj);
			}
		}

		static void Recycle(GameObject obj, GameObject prefab)
		{
			Instance.pooledObjects[prefab].Add(obj);
			Instance.spawnedObjects.Remove(obj);
			if (obj != null)
			{
				obj.transform.SetParent(Instance.transform, false);
				obj.SetActive(false);
			}
		}

		public static void RecycleAll<T>(T prefab) where T : Component
		{
			RecycleAll(prefab.gameObject);
		}

		public static void RecycleAll(GameObject prefab)
		{
			foreach (var item in Instance.spawnedObjects)
				if (item.Value == prefab)
					tempList.Add(item.Key);
			for (int i = 0; i < tempList.Count; ++i)
				Recycle(tempList[i]);
			tempList.Clear();
		}

		public static void RecycleAll()
		{
			tempList.AddRange(Instance.spawnedObjects.Keys);
			for (int i = 0; i < tempList.Count; ++i)
				Recycle(tempList[i]);
			tempList.Clear();
		}

		public static bool IsSpawned(GameObject obj)
		{
			return Instance.spawnedObjects.ContainsKey(obj);
		}

		public static int CountPooled<T>(T prefab) where T : Component
		{
			return CountPooled(prefab.gameObject);
		}

		public static int CountPooled(GameObject prefab)
		{
			List<GameObject> list;
			if (Instance.pooledObjects.TryGetValue(prefab, out list))
				return list.Count;
			return 0;
		}

		public static int CountSpawned<T>(T prefab) where T : Component
		{
			return CountSpawned(prefab.gameObject);
		}

		public static int CountSpawned(GameObject prefab)
		{
			int count = 0;
			foreach (var instancePrefab in Instance.spawnedObjects.Values)
				if (prefab == instancePrefab)
					++count;
			return count;
		}

		public static int CountAllPooled()
		{
			int count = 0;
			foreach (var list in Instance.pooledObjects.Values)
				count += list.Count;
			return count;
		}

		public static List<GameObject> GetPooled(GameObject prefab, List<GameObject> list, bool appendList)
		{
			if (list == null)
				list = new List<GameObject>();
			if (!appendList)
				list.Clear();
			List<GameObject> pooled;
			if (Instance.pooledObjects.TryGetValue(prefab, out pooled))
				list.AddRange(pooled);
			return list;
		}

		public static List<T> GetPooled<T>(T prefab, List<T> list, bool appendList) where T : Component
		{
			if (list == null)
				list = new List<T>();
			if (!appendList)
				list.Clear();
			List<GameObject> pooled;
			if (Instance.pooledObjects.TryGetValue(prefab.gameObject, out pooled))
				for (int i = 0; i < pooled.Count; ++i)
					list.Add(pooled[i].GetComponent<T>());
			return list;
		}


		public static List<GameObject> GetSpawned(GameObject prefab, List<GameObject> list, bool appendList)
		{
			if (list == null)
				list = new List<GameObject>();
			if (!appendList)
				list.Clear();
			foreach (var item in Instance.spawnedObjects)
				if (item.Value == prefab)
					list.Add(item.Key);
			return list;
		}

		public static List<T> GetSpawned<T>(T prefab, List<T> list, bool appendList) where T : Component
		{
			if (list == null)
				list = new List<T>();
			if (!appendList)
				list.Clear();
			var prefabObj = prefab.gameObject;
			foreach (var item in Instance.spawnedObjects)
				if (item.Value == prefabObj)
					list.Add(item.Key.GetComponent<T>());
			return list;
		}

		public static void DestroyPooled(GameObject prefab)
		{
			List<GameObject> pooled;
			if (Instance.pooledObjects.TryGetValue(prefab, out pooled))
			{
				for (int i = 0; i < pooled.Count; ++i)
					GameObject.Destroy(pooled[i]);
				pooled.Clear();
			}
		}

		public static void DestroyPooled<T>(T prefab) where T : Component
		{
			DestroyPooled(prefab.gameObject);
		}

		public static void DestroyAll(GameObject prefab)
		{
			RecycleAll(prefab);
			DestroyPooled(prefab);
		}

		public static void DestroyAll<T>(T prefab) where T : Component
		{
			DestroyAll(prefab.gameObject);
		}
	}

	public interface IObjectPool
	{
		void InitObject();
		void OnSpwan();
	}

}