using System.Collections.Generic;
using com.otm.manager;
using com.otm.ui;
using com.otm.utility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.tutorial
{
	public class TutorialManager : RunTimeManager<TutorialManager>
	{
		#region private variables
		[SerializeField] private Camera cam;
		[SerializeField] private List<TutorialMono> tutorialMonos = new List<TutorialMono>();
		[SerializeField] private bool canStartTutorial;
		private TutorialCanvas canvas;

		#endregion

		#region public static variables

		public static ActionSystem<int> OnTutorialStart = new ActionSystem<int>();
		public static ActionSystem<int> OnTutorialEnd = new ActionSystem<int>();
		public static ActionSystem OnTutorialStop = new ActionSystem();

		#endregion

		#region propertice
		[ShowInInspector] public TutorialMono CurrentTutorial { get; set; }
		public bool IsTutorialRunning => CurrentTutorial != null;
		public bool CanStartTutorial => canStartTutorial;
		public Camera Camera => cam;

		#endregion

		#region unity callback

		public override void Awake()
		{
			base.Awake();
			LoadTutorialResources();
			InitTutorials();
		}

		public override void OnDestroy()
		{
			OnTutorialStart.ClearEvents();
			OnTutorialEnd.ClearEvents();
			OnTutorialStop.ClearEvents();
			base.OnDestroy();
		}

		#endregion

		#region private methods

		private void LoadTutorialResources()
		{
			TutorialCanvas c = Resources.Load<TutorialCanvas>("TutorialCanvas");
			canvas = Instantiate(c, transform);
			canvas.SetCamareToCanvas(cam);
		}

		private void InitTutorials()
		{
			for (int i = 0; i < tutorialMonos.Count; i++)
			{
				if (!tutorialMonos[i].IsTutorialPlayed)
				{
					tutorialMonos[i].Init();
				}
			}
		}

		#endregion

		#region public methods

		public void UpdateCamera(Camera camera)
		{
			canvas.SetCamareToCanvas(camera);
			cam = camera;
		}

		public TBaseView GetView<TBaseView>() where TBaseView : BaseView
		{
			return canvas.GetView<TBaseView>();
		}

		public TBaseView GetViewEditor<TBaseView>() where TBaseView : BaseView
		{
			return canvas.GetViewEditor<TBaseView>();
		}

		public void LoadResouces()
		{
			if (canvas == null)
			{
				TutorialCanvas c = Resources.Load<TutorialCanvas>("TutorialCanvas");
				canvas = Instantiate(c, transform);
				canvas.MapViews();
				canvas.SetCamareToCanvas(FindAnyObjectByType<Camera>());
			}
		}

		#endregion
	}
}