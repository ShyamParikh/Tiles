using com.otm.utility;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.otm.tutorial
{
	public class TutorialMono : SerializedMonoBehaviour
	{
		#region private variables

		[SerializeField] private int tutorialId;
		[SerializeField] private bool selfInit = true;
		[SerializeField] private TutorialTrigger trigger;
		[SerializeField, ListDrawerSettings(CustomAddFunction = "OnNewStep", ShowPaging = false)]
		private List<TutorialStep> steps = new List<TutorialStep>();
		private Dictionary<string, object> sharedData = new Dictionary<string, object>();
		private Dictionary<string, GameObject> sharedGameObject = new Dictionary<string, GameObject>();
		[ShowInInspector] private int currentStep;

		#endregion

		#region propetice

		public int TutorialId => tutorialId;

		public bool IsTutorialPlayed
		{
			get => PlayerPrefsHelper.GetBool("Tutorial_" + tutorialId);
			private set => PlayerPrefsHelper.SetBool("Tutorial_" + tutorialId, value);
		}

		public bool IsInitDone { get; set; }

		#endregion

		#region unity callback

		public void Awake()
		{
			if (selfInit)
				Init();
		}

		public void OnDestroy()
		{
			trigger?.OnDestroy();
		}

		#endregion

		#region public methods

		public void Init()
		{
			if (!TutorialManager.Instance.CanStartTutorial)
				return;
			if (IsTutorialPlayed)
				return;
			IsInitDone = false;
			trigger?.Init(this);
			for (int i = 0; i < steps.Count; i++)
			{
				steps[i].Init(tutorialId, this);
			}

			IsInitDone = true;
		}

		[Button]
		public void StartTutorial()
		{
			if (!TutorialManager.Instance.CanStartTutorial)
				return;
			PlayerPrefsHelper.CanSaveData = false;
			TutorialManager.Instance.CurrentTutorial = this;
			StartCoroutine(WaitForInit());
		}

		[Button]
		public void MoveToNextStep()
		{
			TutorialManager.Instance.CurrentTutorial = this;
			StartCoroutine(WaitAndStartNewStep());
		}

		public void EndTutorial()
		{
			IsTutorialPlayed = true;
			PlayerPrefsHelper.CanSaveData = true;
			PlayerPrefsHelper.SaveData();
			trigger?.OnComplete();
			TutorialManager.Instance.CurrentTutorial = null;
			TutorialManager.OnTutorialEnd.Invoke(tutorialId);
			sharedData.Clear();
			sharedGameObject.Clear();
		}

		public void StopTutorial()
		{
			TutorialManager.Instance.CurrentTutorial = null;
			PlayerPrefsHelper.CanSaveData = true;
			sharedData.Clear();
			sharedGameObject.Clear();
			PlayerPrefsHelper.SaveData();
			steps[currentStep].Stop();
		}

		public void AddSharedData(string key, object data)
		{
			if (!sharedData.ContainsKey(key))
				sharedData.Add(key, data);
			else
				sharedData[key] = data;
		}

		public object GetSharedData(string key)
		{
			if (sharedData.ContainsKey(key))
				return sharedData[key];
			return null;
		}

		public void AddSharedObject(string key, GameObject data)
		{
			if (!sharedGameObject.ContainsKey(key))
				sharedGameObject.Add(key, data);
			else
				sharedGameObject[key] = data;
		}

		public GameObject GetSharedObject(string key)
		{
			if (sharedGameObject.ContainsKey(key))
				return sharedGameObject[key];
			return null;
		}

		#endregion

		#region private methods

		private void DeActiveAllStep()
		{
			for (int i = 0; i < steps.Count; i++)
			{
				steps[i].Deactive();
			}
		}

		private void ActiveStep(Action onStepActive = null)
		{
			onStepActive?.Invoke();
			steps[currentStep].Active();
		}

		#endregion

		#region Coroutine

		IEnumerator WaitForInit()
		{
			while (!IsInitDone)
			{
				yield return null;
			}

			TutorialManager.OnTutorialStart.Invoke(tutorialId);
			currentStep = 0;
			TutorialManager.Instance.CurrentTutorial = this;
			ActiveStep();
		}

		IEnumerator WaitForEndOfFrame()
		{
			yield return new WaitForEndOfFrame();
			if (steps.Count > currentStep)
				ActiveStep();
			else
				EndTutorial();
		}

		IEnumerator WaitAndStartNewStep()
		{
			if (currentStep < steps.Count)
			{
				steps[currentStep].Deactive();
			}

			currentStep++;
			yield return StartCoroutine(WaitForEndOfFrame());
		}

		#endregion

#if UNITY_EDITOR

		#region Editor methods

		private void OnNewStep()
		{
			steps.Add(new TutorialStep());
		}

		[BoxGroup("Find"), Space(20)] public List<TutorialThings> tutorialThingses = new List<TutorialThings>();

		[SerializeField, BoxGroup("Find"), ValueDropdown("GetThingsType")] private Type findStepType;

		[Button, BoxGroup("Find")]
		public void FindTutorialStep()
		{
			tutorialThingses.Clear();
			for (int i = 0; i < steps.Count; i++)
			{
				for (int j = 0; j < steps[i].thingsTodo.Count; j++)
				{
					if (steps[i].thingsTodo[j].GetType() == findStepType)
					{
						tutorialThingses.Add(steps[i].thingsTodo[j]);
					}
				}
			}
		}


		[Button]
		public void StartTutorialEditor()
		{
			for (int i = 0; i < steps.Count; i++)
			{
				steps[i].Init(tutorialId, this);
			}

			StartTutorial();
		}

		public TutorialStep FindStep(int index)
		{
			if (index < 0 && index >= steps.Count)
				return null;
			return steps[index];
		}

		[Button]
		public void MakeUnPlayed()
		{
			PlayerPrefs.DeleteKey("Tutorial_" + tutorialId);
		}

		[OnInspectorGUI]
		private void UpdateSetForEditor()
		{
			for (int i = 0; i < steps.Count; i++)
			{
				steps[i].UpdateForEditor();
			}
		}

		private Type[] GetThingsType()
		{
			Type[] q = typeof(TutorialThings).Assembly.GetTypes()
				.Where(x => !x.IsAbstract)
				.Where(x => !x.IsGenericTypeDefinition)
				.Where(x => typeof(TutorialThings).IsAssignableFrom(x)).ToArray();
			return q;
		}

		#endregion

#endif
	}
}