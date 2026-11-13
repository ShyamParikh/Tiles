using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.otm.manager;
using com.otm.ui;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.tutorial
{
	[HideReferenceObjectPicker, HideLabel]
	public class TutorialStep
	{
		#region private variables

		[TypeFilter("GetThings")] public List<TutorialThings> thingsTodo = new List<TutorialThings>();
		private int tutorialId;
		private TutorialMono tutorialMono;

		#endregion


		#region public methods

		public void Init(int tutorialId, TutorialMono tutorialMono)
		{
			this.tutorialId = tutorialId;
			this.tutorialMono = tutorialMono;
			for (int i = 0; i < thingsTodo.Count; i++)
			{
				thingsTodo[i].Init(tutorialId, tutorialMono);
			}
		}

		public void Active()
		{
			ActiveStep();
		}

		public void Deactive()
		{
			for (int i = 0; i < thingsTodo.Count; i++)
			{
				thingsTodo[i].DeActive();
			}
		}

		public void Stop()
		{
			for (int i = 0; i < thingsTodo.Count; i++)
			{
				thingsTodo[i].Stop();
			}
		}

#if UNITY_EDITOR
		public void UpdateForEditor()
		{
			for (int i = 0; i < thingsTodo.Count; i++)
			{
				thingsTodo[i].UpdateThing();
			}
		}
#endif

		#endregion

		#region private methods

		private void ActiveStep()
		{
			for (int i = 0; i < thingsTodo.Count; i++)
			{
				thingsTodo[i].Active();
			}
		}

		private Type[] GetThings()
		{
			Type[] q = typeof(TutorialThings).Assembly.GetTypes()
				.Where(x => !x.IsAbstract)
				.Where(x => !x.IsGenericTypeDefinition)
				.Where(x => typeof(TutorialThings).IsAssignableFrom(x)).ToArray();
			return q;
		}

		#endregion

		#region coroutine

		IEnumerator WaitAndStartStep(float wait)
		{
			yield return new WaitForSecondsRealtime(wait);
			ActiveStep();
		}

		#endregion

	}
}