using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.otm.ui
{
	public class UITrail : MonoBehaviour
	{
		#region private variables

		[SerializeField] private Image prefabImage;
		[SerializeField] private Transform parent;
		[SerializeField] private Gradient colorOverTime;
		[SerializeField] private AnimationCurve sizeOverTime;
		[SerializeField] private float spawnRate = 0.05f;
		[SerializeField] private float life = 0.5f;

		private List<Image> disableImage = new List<Image>();
		private List<Image> images = new List<Image>();
		private WaitForSeconds wait;
		private RectTransform mainRect;

		#endregion

		#region unity callback

		private void OnEnable()
		{
			mainRect = gameObject.GetComponent<RectTransform>();
			wait = new WaitForSeconds(spawnRate);
			StartCoroutine(SpawnTrail());
		}

		private void OnDisable()
		{
			StopAllCoroutines();
			for (int i = 0; i < images.Count; i++)
			{
				images[i].gameObject.SetActive(false);
			}
			disableImage.Clear();
			disableImage.AddRange(images);
		}

		#endregion

		#region private methods

		private Image GetImage()
		{
			if (disableImage.Count > 0)
			{
				Image image = disableImage[0];
				disableImage.RemoveAt(0);
				return image;
			}
			Image img = Instantiate(prefabImage, parent);
			images.Add(img);
			return img;
		}

		#endregion

		#region coroutine

		IEnumerator SpawnTrail()
		{
			while (true)
			{
				Image trailImage = GetImage();
				trailImage.transform.position = mainRect.position;
				StartCoroutine(DoEnd(trailImage));
				if (spawnRate > 0)
					yield return new WaitForSeconds(spawnRate);
				else
					yield return null;
			}
		}

		IEnumerator DoEnd(Image img)
		{
			Color c = img.color;
			float i = 0;
			float rate = 1f / life;
			img.gameObject.SetActive(true);
			while (i < 1f)
			{
				i += Time.deltaTime * rate;
				img.color = colorOverTime.Evaluate(i);
				img.transform.localScale = Vector3.one * sizeOverTime.Evaluate(i);
				yield return null;
			}
			img.gameObject.SetActive(false);
			disableImage.Add(img);
		}
		#endregion
	}
}
