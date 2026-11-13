using com.otm.manager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace com.otm.utility
{

	public class CheatButton : MonoBehaviour
	{
		#region private variables

		[SerializeField] private int tapCount = 5;
		[SerializeField] private float resetWait;
		[SerializeField] private TMP_Text text;

		private int count;
		private float wait;
		private Coroutine coroutine;

		#endregion

		#region unity callback

		private void OnEnable()
		{
			count = 0;
			text.text = "";
		}

		private void OnDisable()
		{
			count = 0;
			text.text = "";
			if (coroutine != null)
				StopCoroutine(coroutine);
		}

		private void Update()
		{
			if (wait >= resetWait)
				count = 0;
			wait += Time.unscaledDeltaTime;
		}

		#endregion

		#region public methods

		public void OnButtonClick()
		{
			count++;
			wait = 0;
			if (count >= tapCount)
			{
#if !UNITY_EDITOR
				coroutine = StartCoroutine(ShowKeyBoard());
#else
				ReporterManager.Instance.ShowCheat();
#endif
			}
		}

		#endregion

		#region private methods

		//private void ShowKeyboard()
		//{
		//	Debug.Log("KeyBorad open");
		//	TouchScreenKeyboard keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false, "?????");

		//	text.text = "";
		//	EndOfFrameTask endOfFrameTask = new EndOfFrameTask();
		//	endOfFrameTask.OnTaskUpdate.RegisterEvent(() =>
		//	{
		//		Debug.Log(text);
		//		text.text = keyboard.text;
		//	});
		//	endOfFrameTask.OnTaskComplete.RegisterEvent(() =>
		//	{
		//		count = 0;
		//		Debug.Log(text);
		//		ReporterManager.Instance.ShowCheat(text.text);
		//	});
		//	endOfFrameTask.Start(() => { return keyboard.status == TouchScreenKeyboard.Status.Visible; });
		//}

		#endregion

		#region coroutine

		IEnumerator ShowKeyBoard()
		{
			TouchScreenKeyboard keyboard = TouchScreenKeyboard.Open("");
			bool isActive = true;
			while (isActive)
			{
				if (!string.IsNullOrEmpty(keyboard.text))
				{
					char c = keyboard.text.Last();
					if (c == '\n' || keyboard.status == TouchScreenKeyboard.Status.Canceled || keyboard.status == TouchScreenKeyboard.Status.Done)
					{
						keyboard.active = false;
						isActive = false;
					}
					else
					{
						text.text = keyboard.text;
					}
				}
				yield return new WaitForEndOfFrame();
			}
			yield return new WaitForEndOfFrame();
			count = 0;
			ReporterManager.Instance.ShowCheat(text.text);
		}

		#endregion

	}
}
