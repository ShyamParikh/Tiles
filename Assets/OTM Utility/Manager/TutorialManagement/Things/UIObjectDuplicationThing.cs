using com.otm.utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace com.otm.tutorial
{
	public class UIObjectDuplicationThing : TutorialThings
	{
		#region private methods
		[SerializeField, OnValueChanged("FindPath")] private RectTransform rectTransform;
		[SerializeField] private bool disableRaycast;
		[SerializeField] private string sceneName;
		[SerializeField] private GameObjectPathMapping[] path = new GameObjectPathMapping[0];

		private RectTransform tutorialRectTransform;

		#endregion

		#region override methods

		[Button]
		public override void Active()
		{
			GameObject gm = FindGameObjectUtility.FindGameObject(path, sceneName);
			if (gm != null)
			{
				rectTransform = gm.GetComponent<RectTransform>();
			}
			if (rectTransform != null)
			{
				tutorialRectTransform = Object.Instantiate(rectTransform, rectTransform.parent);
				tutorialRectTransform.gameObject.SetActive(true);
				rectTransform.gameObject.SetActive(false);
				DisableRaycast();
			}

		}

		[Button]
		public override void DeActive()
		{
			if (tutorialRectTransform != null)
				Object.Destroy(tutorialRectTransform.gameObject);
			if (rectTransform != null)
				rectTransform.gameObject.SetActive(true);
		}

		public override void Stop()
		{
			if (tutorialRectTransform != null)
				Object.Destroy(tutorialRectTransform.gameObject);
			if (rectTransform != null)
				rectTransform.gameObject.SetActive(true);
		}

		#endregion

		#region private methods

		private void DisableRaycast()
		{
			if (!disableRaycast)
				return;
			Graphic[] graphics = tutorialRectTransform.GetComponentsInChildren<Graphic>();
			for (int i = 0; i < graphics.Length; i++)
			{
				graphics[i].raycastTarget = false;
			}
		}

		#endregion

		#region editor methods

#if UNITY_EDITOR

		private void FindPath()
		{
			if (rectTransform != null)
				path = FindGameObjectUtility.GetGameObjectPath(rectTransform.gameObject, out sceneName);
		}

#endif

		#endregion
	}
}
