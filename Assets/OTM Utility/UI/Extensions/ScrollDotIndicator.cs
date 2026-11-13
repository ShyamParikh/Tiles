using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace com.otm.ui.extension
{
    public class ScrollDotIndicator : MonoBehaviour
    {
        #region private variables

        [Header("References")]
        [SerializeField] private HorizontalScrollSnap scrollSnap;
        [SerializeField] private Transform dotContainer;        // parent object holding all dot images
        [SerializeField] private GameObject dotPrefab;          // single dot prefab (Image)

        [Header("Dot Settings")]
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private Vector2 activeDotSize = new Vector2(24f, 24f);
        [SerializeField] private Vector2 inactiveDotSize = new Vector2(16f, 16f);
        [SerializeField] private float dotAnimSpeed = 8f;        // lerp speed for color/size transition

        private List<Image> dots = new List<Image>();
        private List<RectTransform> dotRects = new List<RectTransform>();
        private Coroutine[] dotCoroutines;
        private int currentIndex = 0;

        #endregion

        #region unity callback

        private void Start()
        {
            if (scrollSnap == null)
            {
                Debug.LogError("[ScrollDotIndicator] HorizontalScrollSnap reference is missing!");
                return;
            }

            BuildDots();
            scrollSnap.OnSelectionChange.AddListener(OnSelectionChanged);

            // Set initial dot state
            OnSelectionChanged(scrollSnap.SelectedElement);
        }

        private void OnDestroy()
        {
            if (scrollSnap != null)
                scrollSnap.OnSelectionChange.RemoveListener(OnSelectionChanged);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Call this if your scroll content changes at runtime and you need to rebuild dots.
        /// </summary>
        public void RebuildDots()
        {
            BuildDots();
            OnSelectionChanged(scrollSnap.SelectedElement);
        }

        #endregion

        #region private methods

        private void BuildDots()
        {
            // Clear old dots
            foreach (Transform child in dotContainer)
                Destroy(child.gameObject);

            dots.Clear();
            dotRects.Clear();

            if (dotPrefab == null)
            {
                Debug.LogError("[ScrollDotIndicator] Dot prefab is missing!");
                return;
            }

            // Count children in scroll content
            int totalElements = scrollSnap.transform
                .GetComponentInChildren<ScrollRect>()
                .content.childCount;

            dotCoroutines = new Coroutine[totalElements];

            for (int i = 0; i < totalElements; i++)
            {
                GameObject dotGo = Instantiate(dotPrefab, dotContainer);
                dotGo.name = $"Dot_{i}";

                Image img = dotGo.GetComponent<Image>();
                RectTransform rect = dotGo.GetComponent<RectTransform>();

                if (img == null)
                {
                    Debug.LogError("[ScrollDotIndicator] Dot prefab must have an Image component!");
                    return;
                }

                img.color = inactiveColor;
                rect.sizeDelta = inactiveDotSize;

                dots.Add(img);
                dotRects.Add(rect);
            }
        }

        private void OnSelectionChanged(int index)
        {
            if (dots == null || dots.Count == 0)
                return;

            index = Mathf.Clamp(index, 0, dots.Count - 1);

            for (int i = 0; i < dots.Count; i++)
            {
                // Stop any running animation on this dot
                if (dotCoroutines[i] != null)
                    StopCoroutine(dotCoroutines[i]);

                bool isActive = (i == index);
                dotCoroutines[i] = StartCoroutine(
                    AnimateDot(i, isActive ? activeColor : inactiveColor,
                                  isActive ? activeDotSize : inactiveDotSize)
                );
            }

            currentIndex = index;
        }

        #endregion

        #region Coroutine

        private IEnumerator AnimateDot(int dotIndex, Color targetColor, Vector2 targetSize)
        {
            Image img = dots[dotIndex];
            RectTransform rect = dotRects[dotIndex];

            Color startColor = img.color;
            Vector2 startSize = rect.sizeDelta;

            float t = 0f;
            while (t < 1f)
            {
                t += scrollSnap.UseUnScaledTime ? Time.unscaledDeltaTime : Time.deltaTime * dotAnimSpeed;
                img.color = Color.Lerp(startColor, targetColor, t);
                rect.sizeDelta = Vector2.Lerp(startSize, targetSize, t);
                yield return null;
            }

            img.color = targetColor;
            rect.sizeDelta = targetSize;
            dotCoroutines[dotIndex] = null;
        }

        #endregion
    }
}