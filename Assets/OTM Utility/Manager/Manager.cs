using com.otm.ui;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace com.otm.manager
{
	public abstract class Manager<T> : MonoBehaviour where T : Manager<T>
	{
		public static T Instance;

		private ManagerInstanceLoader managerInstanceLoader;

		public bool IsLoaded
		{
			get
			{
				if (managerInstanceLoader == null)
					return true;
				return managerInstanceLoader.loaded;
			}
		}

		public virtual void Awake()
		{
			managerInstanceLoader = GetComponent<ManagerInstanceLoader>();
			OnLoadingStart();
			if (!Instance)
			{
				Instance = this as T;
			}
			else
			{
				Destroy(gameObject);
			}
		}

		public virtual void OnDestroy()
		{
			if (Instance)
			{
				Destroy(Instance);
			}
			Instance = null;
		}

		public void OnLoadingStart()
		{
			if (managerInstanceLoader != null)
				managerInstanceLoader.OnLoadingStart();
		}

		public void OnLoadingDone()
		{
			if (managerInstanceLoader != null)
				managerInstanceLoader.OnLoadingDone();
		}
	}

	public abstract class SerializedManager<T> : SerializedMonoBehaviour where T : SerializedManager<T>
	{
		public static T Instance;

		private ManagerInstanceLoader managerInstanceLoader;

		public bool IsLoaded
		{
			get
			{
				if (managerInstanceLoader == null)
					return true;
				return managerInstanceLoader.loaded;
			}
		}

		public virtual void Awake()
		{
			managerInstanceLoader = GetComponent<ManagerInstanceLoader>();
			OnLoadingStart();
			if (!Instance)
			{
				Instance = this as T;
			}
			else
			{
				Destroy(gameObject);
			}
		}

		public virtual void OnDestroy()
		{
			if (Instance)
			{
				Destroy(Instance);
			}
			Instance = null;
		}

		public void OnLoadingStart()
		{
			if (managerInstanceLoader != null)
				managerInstanceLoader.OnLoadingStart();
		}

		public void OnLoadingDone()
		{
			if (managerInstanceLoader != null)
				managerInstanceLoader.OnLoadingDone();
		}
	}


	public abstract class RunTimeManager<T> : MonoBehaviour where T : RunTimeManager<T>
	{
		#region private variables
		private static T instance;
		#endregion

		#region porpertice
		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					GameObject obj = new GameObject(typeof(T).ToString());
					T o = obj.AddComponent<T>();
#if UNITY_EDITOR
					if (Application.isPlaying)
						DontDestroyOnLoad(obj);
					else
					{
						instance = o;
						instance.Awake();
					}
#else
					DontDestroyOnLoad(obj);
#endif

				}

				return instance;
			}
			set { instance = value; }
		}
		#endregion

		#region unity callback

		public virtual void Awake()
		{
			instance = this as T;
		}

		public virtual void OnDestroy()
		{
			if (instance)
				Destroy(instance);
			instance = null;
		}

		#endregion

	}

	public abstract class UIManager<T> : SerializedManager<T> where T : UIManager<T>
	{

		#region protected variables

		[SerializeField] protected List<BaseView> views = new List<BaseView>();
		protected Dictionary<Type, BaseView> mapViews = new Dictionary<Type, BaseView>();
		protected GraphicRaycaster graphicRaycaster;

		#endregion

		#region unity callback

		public override void Awake()
		{
			base.Awake();
			graphicRaycaster = GetComponent<GraphicRaycaster>();
			MapViews();
		}

		#endregion

		#region public methods

		public void SetIntraction(bool isActive)
		{
			graphicRaycaster.enabled = isActive;
			foreach (var v in mapViews)
			{
				if (!v.Value.IngnoreIntraction)
					v.Value.GraphicRaycaster.enabled = isActive;
			}
		}

		public TBaseView GetView<TBaseView>() where TBaseView : BaseView
		{
			if (mapViews.ContainsKey(typeof(TBaseView)))
				return (TBaseView)mapViews[typeof(TBaseView)];
			return null;
		}

		public BaseView GetView(Type type)
		{
			if (mapViews.ContainsKey(type))
				return mapViews[type];
			return null;
		}

		public void ShowView<TBaseView>() where TBaseView : BaseView
		{
			if (mapViews.ContainsKey(typeof(TBaseView)))
				mapViews[typeof(TBaseView)].ShowView();
		}

		public void HideView<TBaseView>() where TBaseView : BaseView
		{
			if (mapViews.ContainsKey(typeof(TBaseView)))
				mapViews[typeof(TBaseView)].HideView();
		}

		public void ShowView(Type type)
		{
			if (mapViews.ContainsKey(type))
				mapViews[type].ShowView();
		}

		public void HideView(Type type)
		{
			if (mapViews.ContainsKey(type))
				mapViews[type].HideView();
		}

		public void AddView(BaseView baseView)
		{
			BaseView view = Instantiate(baseView, transform);
			view.Init();
			if (!mapViews.ContainsKey(view.GetType()))
				mapViews.Add(view.GetType(), view);
			else
				mapViews[view.GetType()] = view;
		}

		public void RemoveView(BaseView baseView)
		{
			BaseView view = GetView(baseView.GetType());
			if (view != null)
				Destroy(view);
			mapViews.Remove(baseView.GetType());
		}

		#endregion

		#region private methods

		private void MapViews()
		{
			for (int i = 0; i < views.Count; i++)
			{
				if (views[i] == null) continue;
				views[i].Init();
				if (!mapViews.ContainsKey(views.GetType()))
				{
					mapViews.Add(views[i].GetType(), views[i]);
				}
			}
		}

		[Button]
		private void SetView()
		{
			views.Clear();
			views.AddRange(transform.GetComponentsInChildren<BaseView>(true));
		}

		#endregion
	}



	public abstract class NoneMonoManager<T> where T : NoneMonoManager<T>
	{
		#region public static variables

		public static T Instance;

		#endregion

		#region costructor

		public NoneMonoManager()
		{
			if (Instance == null)
				Instance = this as T;
		}

		/*	~NoneMonoManager()
			{
				Instance = null;
			}*/

		#endregion

		#region public methods

		public void Destroy()
		{
			Instance = null;
		}

		#endregion
	}


	public enum LoadingType
	{
		auto,
		Manual,
	}
}