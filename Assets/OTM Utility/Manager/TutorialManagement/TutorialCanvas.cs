using com.otm.ui;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.tutorial
{
	public class TutorialCanvas : MonoBehaviour
	{
		#region private variables
		[SerializeField] private Canvas canvas;
		[SerializeField] private List<BaseView> views = new List<BaseView>();
		private Dictionary<Type, BaseView> mapViews = new Dictionary<Type, BaseView>();

		#endregion

		#region unity callback

		public void Awake()
		{
			MapViews();
		}

		#endregion

		#region public methods

		public void SetCamareToCanvas(Camera camera)
		{
			canvas.worldCamera = camera;
		}

		public TBaseView GetView<TBaseView>() where TBaseView : BaseView
		{
			if (mapViews.ContainsKey(typeof(TBaseView)))
			{
				BaseView view = Instantiate(mapViews[typeof(TBaseView)], mapViews[typeof(TBaseView)].transform.parent);
				view.Init();
				return (TBaseView)view;
			}
			return null;
		}

		public TBaseView GetViewEditor<TBaseView>() where TBaseView : BaseView
		{
			if (mapViews.ContainsKey(typeof(TBaseView)))
			{
				return (TBaseView)mapViews[typeof(TBaseView)];
			}
			return null;
		}

		public void MapViews()
		{
			for (int i = 0; i < views.Count; i++)
			{
				views[i].Init();
				if (!mapViews.ContainsKey(views.GetType()))
				{
					mapViews.Add(views[i].GetType(), views[i]);
				}
			}
		}

		#endregion
	}
}
