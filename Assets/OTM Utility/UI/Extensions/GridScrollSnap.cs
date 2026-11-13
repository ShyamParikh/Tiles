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
	[RequireComponent(typeof(ScrollRect))]
	public class GridScrollSnap : MonoBehaviour, IBeginDragHandler, IEndDragHandler
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

		private List<RectTransform> children = new List<RectTransform>();
		private List<Vector2> elementPositions = new List<Vector2>();
		private ISnapScrollUpdate[] snapUpdates = new ISnapScrollUpdate[0];

		private bool canCallUpdate;
		private Vector2 posNeedToSync;
		private Coroutine coroutine;
		[ShowInInspector] private Vector2 minPos;
		[ShowInInspector] private Vector2 maxPos;
		[ShowInInspector] private float gap;
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
			{
				scrollRect.content.anchoredPosition= scrollRect.content.anchoredPosition.Clamp(minPos,maxPos);
			}

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
			float wholePos;
			if (children.Count <= 1)
			{
				snapUpdates[0]?.OnUpdate(1f);
				snapUpdates[0]?.OnFullUpdate(1f);
				return elementIndex;
			}
			for (int i = 0; i < children.Count; i++)
			{
				pos = Mathf.Abs(Vector3.Distance(children[i].position, center.position));
				if (scrollRect.content.anchoredPosition.x >= minPos.x && scrollRect.content.anchoredPosition.x <= maxPos.x || scrollRect.content.anchoredPosition.y >= minPos.y && scrollRect.content.anchoredPosition.y <= maxPos.y)
				{
					Vector2 p;
					Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, children[i].position);
					RectTransformUtility.ScreenPointToLocalPointInRectangle(center, screenP, null, out p);
					wholePos= Mathf.Abs(Vector3.Distance(p, center.position));
					if (pos <= singleItemGap)
						snapUpdates[i]?.OnUpdate((1f - (pos / singleItemGap)));
					else
						snapUpdates[i]?.OnUpdate(0);
					snapUpdates[i]?.OnFullUpdate(1f - (wholePos/ gap));
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
			children = children.OrderByDescending(x => x.anchoredPosition.y).ThenBy(x=>x.anchoredPosition.x).ToList();
		}

		private void SetElementPositions()
		{
			elementPositions = new List<Vector2>();
			snapUpdates = new ISnapScrollUpdate[children.Count];
			scrollRect.content.anchoredPosition = Vector2.zero;
			maxPos = Vector2.one * float.MinValue;
			minPos = Vector2.one * float.MaxValue;
			for (int i = 0; i < children.Count; i++)
			{
				snapUpdates[i] = children[i].GetComponent<ISnapScrollUpdate>();
				Vector2 pos = GetPositionFromCenter(children[i]);
				elementPositions.Add(pos);
				if (maxPos.x < pos.x)
					maxPos.x = pos.x;
				if (maxPos.y < pos.y)
					maxPos.y = pos.y;

				if (minPos.x > pos.x)
					minPos.x = pos.x;
				if (minPos.y > pos.y)
					minPos.y = pos.y;
			}
			if (children.Count > 0)
			{
				gap = Mathf.Abs(Vector3.Distance(minPos, maxPos));
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
			float rate = 1f / (Mathf.Abs(Vector2.Distance(startPos,endPos)) / snapSpeed);
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
			float rate = 1f / (Mathf.Abs(Vector2.Distance(startPos, endPos)) / snapSpeed);
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
 