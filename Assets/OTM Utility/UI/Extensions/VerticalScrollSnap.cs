using com.otm.utility;
using Sirenix.OdinInspector;
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
	public class VerticalScrollSnap : MonoBehaviour, IBeginDragHandler, IEndDragHandler
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

		[ShowInInspector] private List<RectTransform> children = new List<RectTransform>();
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

		#endregion

		#region propetice

		public int SelectedElement { get; private set; }

		public int StartElement { get => startElement; set => startElement = value; }

		public UnityEvent<int> OnSelectionChange => onElementSelectionChange;

		#endregion

		#region unity callback
		private void OnEnable()
		{
			StartCoroutine(UpdateScrollRect());
		}

		private void OnDisable()
		{
			children = new List<RectTransform>();
			elementPositions = new List<Vector2>();
			snapUpdates = new ISnapScrollUpdate[0];
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
				scrollRect.content.anchoredPosition = scrollRect.content.anchoredPosition.Clamp(maxPos, minPos);

			if (oldSelection != SelectedElement)
			{
				onElementSelectionChange.Invoke(SelectedElement);
			}

			if (!canCallUpdate)
				return;

			if (isVelocityLerp && scrollRect.velocity.sqrMagnitude > 0.5f)
			{
				scrollRect.velocity = Vector2.Lerp(scrollRect.velocity, Vector2.zero, Time.deltaTime * velocityDrag);
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
			scrollRect.vertical = true;
			scrollRect.horizontal = false;
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
		}

		public void OnEndDrag(PointerEventData eventData)
		{

			canCallUpdate = true;

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
			SelectedElement = elementIndex;
			posNeedToSync = elementPositions[SelectedElement];
			if (doLerp)
				coroutine = StartCoroutine(SetElementSelection());
			else
			{
				scrollRect.content.anchoredPosition = posNeedToSync + center.anchoredPosition;
				stopUpdate = false;
			}
		}

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
				pos = MathF.Abs(children[i].position.y - center.position.y);
				if (scrollRect.content.anchoredPosition.y >= minPos.y && scrollRect.content.anchoredPosition.y <= maxPos.y)
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
			children = children.OrderByDescending(x => x.anchoredPosition.y).ToList();
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
				pos.x = scrollRect.content.anchoredPosition.x;
				elementPositions.Add(pos);
			}
			if (children.Count > 0)
			{
				maxPos = elementPositions[0];
				minPos = elementPositions[elementPositions.Count - 1];
				gap = Mathf.Abs(children[0].position.y - children[children.Count - 1].position.y);
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
			float rate = 1f / (Mathf.Abs(startPos.y - endPos.y) / snapSpeed);
			while (i < 1f)
			{
				i += Time.deltaTime * rate;
				scrollRect.velocity = Vector2.Lerp(scrollRect.velocity, Vector2.zero, i);
				scrollRect.content.anchoredPosition = Vector2.Lerp(startPos, endPos, i);
				yield return null;
			}
			coroutine = null;
		}

		IEnumerator SetElementSelection()
		{
			float i = 0;
			Vector2 startPos = scrollRect.content.anchoredPosition;
			Vector2 endPos = posNeedToSync;
			float rate = 1f / (Mathf.Abs(startPos.y - endPos.y) / snapSpeed);
			while (i < 1f)
			{
				i += Time.deltaTime * rate;
				scrollRect.velocity = Vector2.Lerp(scrollRect.velocity, Vector2.zero, i);
				scrollRect.content.anchoredPosition = Vector2.Lerp(startPos, endPos, i);
				yield return null;
			}
			coroutine = null;
			stopUpdate = false;
		}

		#endregion
	}
}
