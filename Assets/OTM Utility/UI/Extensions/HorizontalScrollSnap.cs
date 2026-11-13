using com.otm.utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.otm.ui.extension
{
	[RequireComponent(typeof(ScrollRect))]
	public class HorizontalScrollSnap : MonoBehaviour, IBeginDragHandler, IEndDragHandler
	{
		#region private variables
		[SerializeField] private ScrollRect scrollRect;
		[SerializeField] private RectTransform center;
		[SerializeField] private int startElement;
		[SerializeField] private bool isVelocityLerp;
		[SerializeField] private float velocityDrag;
		[SerializeField] private float snapSpeed;
		[SerializeField] private bool stopSnaping;
		[SerializeField] private UnityEvent<int> onElementSelectionChange = new UnityEvent<int>();
		[SerializeField] private bool useSwapMotion;
		[SerializeField] private float swapDeltaThreshold;
		[SerializeField] private float minSwapPosition;
		[SerializeField] private float maxSwapPosition;
		[SerializeField] private float swapSpeed;

		[SerializeField] private bool useUnScaledTime;

		// ── AUTO SCROLL (new) ────────────────────────────────────────────────
		[Header("Auto Scroll")]
		[SerializeField] private bool enableAutoScroll = false;          // master toggle (default OFF)
		[SerializeField] private float autoScrollInterval = 3f;         // seconds between each snap
		[SerializeField] private bool autoScrollLoop = true;            // loop back to first element after last
		private Coroutine autoScrollCoroutine;
		private bool isUserDragging;
		// ─────────────────────────────────────────────────────────────────────

		private List<RectTransform> children = new List<RectTransform>();
		private List<Vector2> elementPositions = new List<Vector2>();
		private ISnapScrollUpdate[] snapUpdates = new ISnapScrollUpdate[0];

		private bool canCallUpdate;
		private Vector2 posNeedToSync;
		private Coroutine coroutine;
		private Vector2 minPos;
		private Vector2 maxPos;
		private float gap;
		private float singleItemGap;
		private Coroutine updateCoroutine;
		private bool stopUpdate;
		private float swapTime;
		private Vector3 swapDireaction;
		private int elementAtBeign;

		#endregion

		#region propetice

		public int SelectedElement { get; private set; }

		public int StartElement { get => startElement; set => startElement = value; }

		public UnityEvent<int> OnSelectionChange => onElementSelectionChange;

		public bool UseUnScaledTime => useUnScaledTime;

		#endregion

		#region unity callback

		private void OnEnable()
		{
			StartCoroutine(UpdateScrollRect());

			// ── AUTO SCROLL (new) ─────────────────────────────────────────────
			if (enableAutoScroll)
				StartAutoScroll();
			// ─────────────────────────────────────────────────────────────────
		}

		private void OnDisable()
		{
			children = new List<RectTransform>();
			elementPositions = new List<Vector2>();
			snapUpdates = new ISnapScrollUpdate[0];

			// ── AUTO SCROLL (new) ─────────────────────────────────────────────
			StopAutoScroll();
			// ─────────────────────────────────────────────────────────────────
		}

		private void Update()
		{
			if (stopUpdate)
				return;
			if (children == null || children.Count <= 0 || elementPositions == null || elementPositions.Count <= 0)
				return;
			float oldSelection = SelectedElement;
			SelectedElement = FindNearestElement();
			posNeedToSync = elementPositions[SelectedElement];
			if (coroutine == null)
				scrollRect.content.anchoredPosition = scrollRect.content.anchoredPosition.Clamp(minPos, maxPos);

			if (oldSelection != SelectedElement)
			{
				onElementSelectionChange.Invoke(SelectedElement);
			}

			if (!canCallUpdate)
				return;

			if (isVelocityLerp && scrollRect.velocity.sqrMagnitude > 0.5f)
			{
				scrollRect.velocity = Vector2.Lerp(scrollRect.velocity, Vector2.zero, (useUnScaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * velocityDrag);
				return;
			}
			if (stopSnaping)
				return;
			if (oldSelection != SelectedElement || (oldSelection == SelectedElement && coroutine == null))
			{
				if (isVelocityLerp)
					scrollRect.StopMovement();
				if (coroutine != null)
					StopCoroutine(coroutine);
				coroutine = StartCoroutine(DoPosition());
			}
		}

		private void OnValidate()
		{
			if (scrollRect == null)
				scrollRect = GetComponent<ScrollRect>();
			if (scrollRect == null)
				return;
			scrollRect.vertical = false;
			scrollRect.horizontal = true;
			scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
			if (center == null)
				center = scrollRect.viewport;
		}

		#endregion

		#region overrided methods

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (coroutine != null)
				StopCoroutine(coroutine);
			coroutine = null;
			canCallUpdate = false;
			swapTime = (useUnScaledTime ? Time.unscaledTime : Time.time);
			swapDireaction = eventData.position;
			elementAtBeign = SelectedElement;

			// ── AUTO SCROLL (new) ─────────────────────────────────────────────
			isUserDragging = true;
			StopAutoScroll();
			// ─────────────────────────────────────────────────────────────────
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			canCallUpdate = true;
			if (!useSwapMotion || elementAtBeign != SelectedElement || ((useUnScaledTime ? Time.unscaledTime : Time.time) - swapTime) > swapDeltaThreshold)
			{
				// ── AUTO SCROLL (new) ──────────────────────────────────────────
				isUserDragging = false;
				if (enableAutoScroll)
					StartAutoScroll();
				// ─────────────────────────────────────────────────────────────
				return;
			}
			float deltaPosition = Mathf.Abs((swapDireaction.x - eventData.position.x));
			if (deltaPosition <= minSwapPosition || deltaPosition >= maxSwapPosition)
			{
				// ── AUTO SCROLL (new) ──────────────────────────────────────────
				isUserDragging = false;
				if (enableAutoScroll)
					StartAutoScroll();
				// ─────────────────────────────────────────────────────────────
				return;
			}
			if (swapDireaction.x < eventData.position.x)
				SetSelection(SelectedElement - 1, true);
			else
				SetSelection(SelectedElement + 1, true);

			// ── AUTO SCROLL (new) ─────────────────────────────────────────────
			isUserDragging = false;
			if (enableAutoScroll)
				StartAutoScroll();
			// ─────────────────────────────────────────────────────────────────
		}

		#endregion

		#region public methods

		public void UpdateScroll()
		{
			if (updateCoroutine != null)
				StopCoroutine(updateCoroutine);
			updateCoroutine = StartCoroutine(UpdateScrollRect());
		}

		public void SetSelection(int elementIndex, bool doLerp)
		{
			stopUpdate = true;
			scrollRect.StopMovement();
			if (coroutine != null)
				StopCoroutine(coroutine);
			elementIndex = Mathf.Clamp(elementIndex, 0, children.Count - 1);
			if (elementIndex != SelectedElement)
				onElementSelectionChange?.Invoke(elementIndex);
			SelectedElement = elementIndex;
			posNeedToSync = elementPositions[SelectedElement];
			if (doLerp)
				coroutine = StartCoroutine(SetElementSelection(swapSpeed));
			else
			{
				scrollRect.content.anchoredPosition = posNeedToSync + center.anchoredPosition;
				stopUpdate = false;
			}
		}

		// ── AUTO SCROLL (new) ────────────────────────────────────────────────
		/// <summary>
		/// Manually start auto-scroll at runtime (respects enableAutoScroll toggle).
		/// </summary>
		public void StartAutoScroll()
		{
			StopAutoScroll();
			autoScrollCoroutine = StartCoroutine(AutoScrollCoroutine());
		}

		/// <summary>
		/// Manually stop auto-scroll at runtime.
		/// </summary>
		public void StopAutoScroll()
		{
			if (autoScrollCoroutine != null)
			{
				StopCoroutine(autoScrollCoroutine);
				autoScrollCoroutine = null;
			}
		}
		// ─────────────────────────────────────────────────────────────────────

		#endregion

		#region private methods

		private int FindNearestElement()
		{
			int elementIndex = 0;
			float lastPos = float.MaxValue;
			float pos;

			if (children.Count <= 1)
			{
				snapUpdates[0]?.OnUpdate(1f);
				snapUpdates[0]?.OnFullUpdate(1f);
				return elementIndex;
			}

			for (int i = 0; i < children.Count; i++)
			{
				pos = MathF.Abs(children[i].position.x - center.position.x);
				if (scrollRect.content.anchoredPosition.x >= minPos.x && scrollRect.content.anchoredPosition.x <= maxPos.x)
				{
					if (pos <= singleItemGap)
						snapUpdates[i]?.OnUpdate((1f - (pos / singleItemGap)));
					else
						snapUpdates[i]?.OnUpdate(0);

					snapUpdates[i]?.OnFullUpdate(1f - (pos / gap));
				}
				if (pos < lastPos)
				{
					lastPos = pos;
					elementIndex = i;
				}

			}
			return elementIndex;
		}

		private void GetChildren()
		{
			int childCount = scrollRect.content.childCount;
			Transform c;
			RectTransform rectTransform;
			children = new List<RectTransform>();
			for (int i = 0; i < childCount; i++)
			{
				c = scrollRect.content.GetChild(i);
				if (c.gameObject.activeSelf)
				{
					rectTransform = c.gameObject.GetComponent<RectTransform>();
					if (rectTransform)
					{
						children.Add(rectTransform);
					}
				}
			}
			children = children.OrderBy(x => x.anchoredPosition.x).ToList();
		}

		private void SetElementPositions()
		{

			elementPositions = new List<Vector2>();
			snapUpdates = new ISnapScrollUpdate[children.Count];
			scrollRect.content.anchoredPosition = Vector2.zero;
			for (int i = 0; i < children.Count; i++)
			{
				snapUpdates[i] = children[i].GetComponent<ISnapScrollUpdate>();
				Vector2 pos = GetPositionFromCenter(children[i]);
				pos.y = scrollRect.content.anchoredPosition.y;
				elementPositions.Add(pos);
			}
			if (children.Count > 0)
			{
				maxPos = elementPositions[0];
				minPos = elementPositions[elementPositions.Count - 1];
				gap = Mathf.Abs(children[0].position.x - children[children.Count - 1].position.x);
				singleItemGap = gap / children.Count;
			}
		}

		private Vector2 GetPositionFromCenter(RectTransform rectTransform)
		{
			Vector2 pos;
			Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(center, screenP, null, out pos);
			return pos * -1f;
		}

		private void SetStartElement()
		{
			if (elementPositions.Count <= 0)
				return;
			startElement = Mathf.Clamp(startElement, 0, children.Count - 1);
			scrollRect.content.anchoredPosition = elementPositions[startElement];
			SelectedElement = startElement;
		}

		#endregion

		#region Coroutine

		IEnumerator UpdateScrollRect()
		{
			canCallUpdate = false;
			yield return new WaitForEndOfFrame();
			GetChildren();
			SetElementPositions();
			SetStartElement();
			canCallUpdate = true;
		}

		IEnumerator DoPosition()
		{
			float i = 0;
			Vector2 startPos = scrollRect.content.anchoredPosition;
			Vector2 endPos = posNeedToSync;
			float rate = 1f / (Mathf.Abs(startPos.x - endPos.x) / snapSpeed);
			while (i < 1f)
			{
				i += (useUnScaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * rate;
				scrollRect.velocity = Vector2.Lerp(scrollRect.velocity, Vector2.zero, i);
				scrollRect.content.anchoredPosition = Vector2.Lerp(startPos, endPos, i);
				yield return null;
			}
			coroutine = null;
		}

		IEnumerator SetElementSelection(float speed)
		{
			float i = 0;
			Vector2 startPos = scrollRect.content.anchoredPosition;
			Vector2 endPos = posNeedToSync;
			float rate = 1f / (Mathf.Abs(startPos.x - endPos.x) / speed);
			float pos;
			while (i < 1f)
			{
				i += (useUnScaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * rate;
				scrollRect.velocity = Vector2.Lerp(scrollRect.velocity, Vector2.zero, i);
				scrollRect.content.anchoredPosition = Vector2.Lerp(startPos, endPos, i);
				scrollRect.content.anchoredPosition = scrollRect.content.anchoredPosition.Clamp(minPos, maxPos);
				for (int j = 0; j < children.Count; j++)
				{
					pos = MathF.Abs(children[j].position.x - center.position.x);
					if (scrollRect.content.anchoredPosition.x >= minPos.x && scrollRect.content.anchoredPosition.x <= maxPos.x)
					{
						if (pos <= singleItemGap)
							snapUpdates[j]?.OnUpdate((1f - (pos / singleItemGap)));
						else
							snapUpdates[j]?.OnUpdate(0);

						snapUpdates[j]?.OnFullUpdate(1f - (pos / gap));
					}
				}
				yield return null;
			}
			coroutine = null;
			stopUpdate = false;
		}

		// ── AUTO SCROLL (new) ────────────────────────────────────────────────
		IEnumerator AutoScrollCoroutine()
		{
			// Wait until scroll is initialised
			yield return new WaitUntil(() => children != null && children.Count > 0 && elementPositions.Count > 0);

			while (enableAutoScroll)
			{
				if (useUnScaledTime)
					yield return new WaitForSecondsRealtime(autoScrollInterval);
				else
					yield return new WaitForSeconds(autoScrollInterval);

				// Safety: skip tick if user grabbed the scroll mid-wait
				if (isUserDragging)
					continue;

				int nextElement = SelectedElement + 1;

				if (nextElement >= children.Count)
				{
					if (autoScrollLoop)
						nextElement = 0;
					else
						break; // reached the end and loop is off → stop
				}

				SetSelection(nextElement, true);
			}

			autoScrollCoroutine = null;
		}
		// ─────────────────────────────────────────────────────────────────────

		#endregion

	}
	public interface ISnapScrollUpdate
	{
		void OnUpdate(float value);
		void OnFullUpdate(float value);
	}
}