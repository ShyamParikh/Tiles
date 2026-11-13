using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.tutorial
{
	[System.Serializable, HideLabel]
	public abstract class TutorialThings
	{
		#region public veriabels

		protected int tutorialId;
		protected TutorialMono mono;

		#endregion

		#region virtual methods

		public virtual void Init(int tutorialId, TutorialMono tutorialMono)
		{
			this.tutorialId = tutorialId;
			mono = tutorialMono;
		}

		#endregion

		#region abstract methods

		public abstract void Active();

		public abstract void DeActive();
		public abstract void Stop();


		#endregion

		#region Editor methods

#if UNITY_EDITOR
		[HideInInspector] public bool showEditor = false;
		[HideInInspector] public TutorialManager manager;
		[Button, HorizontalGroup("EditorButton")]
		public virtual void ShowStepsInEditor()
		{
			SetTutorialManager();
			showEditor = true;
		}

		[Button, HorizontalGroup("EditorButton")]
		public virtual void HideStepsInEditor()
		{
			showEditor = false;
			if (manager != null)
				Object.DestroyImmediate(manager.gameObject);
			manager = null;
		}

		public void UpdateThing()
		{
			if (!showEditor)
				return;
			if (manager == null)
				ShowStepsInEditor();
			OnUpdateThing();
		}

		public virtual void OnUpdateThing()
		{

		}

		private void SetTutorialManager()
		{
			if (manager == null)
			{
				Object[] ms = Resources.FindObjectsOfTypeAll<TutorialManager>();
				for (int i = 0; i < ms.Length; i++)
				{
					if (ms[i].name.Equals("DummyTutorialManager"))
					{
						manager = ms[i] as TutorialManager;
					}
				}
				if (manager == null)
				{
					GameObject gm = new GameObject("DummyTutorialManager");
					gm.hideFlags = HideFlags.DontSaveInEditor;
					manager = gm.AddComponent<TutorialManager>();
					manager.LoadResouces();
				}
			}
		}

#endif

		#endregion
	}
}
