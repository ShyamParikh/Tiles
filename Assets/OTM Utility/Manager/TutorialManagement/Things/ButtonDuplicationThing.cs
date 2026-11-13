using com.otm.utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace com.otm.tutorial
{
	public class ButtonDuplicationThing : TutorialThings
	{
		#region private methods
		[SerializeField, OnValueChanged("FindPath")] private Button button;
		[SerializeField] private string sceneName;
		[SerializeField] private GameObjectPathMapping[] path = new GameObjectPathMapping[0];

		private TutorialButtonView buttonView;

		#endregion

		#region override methods

		[Button]
		public override void Active()
		{
			GameObject gm = FindGameObjectUtility.FindGameObject(path, sceneName);
			if (gm != null)
				button = gm.GetComponent<Button>();
			if (button != null)
			{
				buttonView = TutorialManager.Instance.GetView<TutorialButtonView>();
				buttonView.ShowView(button, MoveToNextStep);
			}
		}

		public override void DeActive()
		{

		}

		public override void Stop()
		{
			buttonView.HideView();
		}

		#endregion

		#region private methods

		private void MoveToNextStep()
		{
			buttonView.HideView();
			button.gameObject.SetActive(true);
			button.onClick.Invoke();
			mono?.MoveToNextStep();
		}


		#endregion

		#region editor methods

#if UNITY_EDITOR

		private void FindPath()
		{
			if (button != null)
				path = FindGameObjectUtility.GetGameObjectPath(button.gameObject, out sceneName);
		}

#endif

		#endregion

	}
}
