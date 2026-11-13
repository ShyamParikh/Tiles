using System.Collections;
using TMPro;
using UnityEngine;

namespace com.otm.utility
{
	public class FpsViwer : MonoBehaviour
	{
		public int FramesPerSec { get; protected set; }

		[SerializeField] private TMP_Text counter;
		[SerializeField] private float interval;
		int lastFrameCount;
		float lastTime;
		float timeSpan;
		int frameCount;


		private void Start()
		{
			StartCoroutine(FPS());
		}

		private IEnumerator FPS()
		{
			while (true)
			{
				lastFrameCount = Time.frameCount;
				lastTime = Time.realtimeSinceStartup;
				yield return new WaitForSeconds(interval);
				timeSpan = Time.realtimeSinceStartup - lastTime;
				frameCount = Time.frameCount - lastFrameCount;
				FramesPerSec = Mathf.RoundToInt(frameCount / timeSpan);
				counter.text = "FPS: " + FramesPerSec.ToString();
			}
		}

	}
}