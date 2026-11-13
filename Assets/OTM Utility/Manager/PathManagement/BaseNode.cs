using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;
using com.otm.manager;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.otm.path
{
	public abstract class BaseNode : SerializedMonoBehaviour
	{

		#region protected variables

		[SerializeField] protected int nodeId;
		[SerializeField] protected NodeType nodeType;
		[SerializeField, ShowIf("nodeType", NodeType.QueueNode)] protected Transform[] queuePoints = new Transform[0];
		[SerializeField, OnValueChanged("OnShortingOrderChange")] protected int sortingOrder;

		#endregion

		#region private variables

		[SerializeField] private float weightFactor = 1;
		[SerializeField] private List<BaseNode> neighbourNodes = new List<BaseNode>();
		private BaseNode parentNode = null;
		private float weight = int.MaxValue;

		#endregion

		#region propertices
#if UNITY_EDITOR
		public Transform[] QueuePoints { get { return queuePoints; } set { queuePoints = value; } }
#endif
		public int SortingOrder { get { return sortingOrder; } set { sortingOrder = value; } }

		public int NodeId
		{
			get => nodeId;
			set => nodeId = value;
		}

		public NodeType NodeType => nodeType;

		public BaseNode ParentNode
		{
			get => parentNode;
			set => parentNode = value;
		}

		public List<BaseNode> NeighbourNodes
		{
			get => neighbourNodes;
			set => neighbourNodes = value;
		}

		public float Weight
		{
			get => weight * weightFactor;
			set => weight = value;
		}
		public int QueueSize => queuePoints.Length;

		#endregion
		#region virtual methods
		public virtual void Init()
		{
		}

		public virtual Vector3 GetStandPoint(Transform transform)
		{
			if (nodeType == NodeType.QueueNode && QueueManager.Instance.IsInQueue(this, transform))
			{
				int queueStandPointIndex = QueueManager.Instance.GetQueuePoint(this, transform);
				queueStandPointIndex = Mathf.Clamp(queueStandPointIndex, 0, queuePoints.Length - 1);
				return queuePoints[queueStandPointIndex].position;
			}
			return this.transform.position;
		}

		#endregion

		#region public methods

		public void ResetWeightAndParent()
		{
			weight = int.MaxValue;
			parentNode = null;
		}

		public bool IsQueueFull()
		{
			if (nodeType == NodeType.QueueNode)
				return QueueManager.Instance.GetQueueCount(this) >= queuePoints.Length;
			return false;
		}

		#endregion

		#region EditorMethods

#if UNITY_EDITOR

		private static Mesh arrowMesh;
		public static Mesh ArrowMesh
		{
			get
			{
#if UNITY_EDITOR
				if (arrowMesh == null)
					arrowMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Editor Default Resources/GizmoMesh/Arrow.fbx");
				return arrowMesh;
#endif
#if !UNITY_EDITOR
                return null;
#endif
			}
		}

		private TextMeshPro textMeshPro;

		private HideFlags Flag => HideFlags.HideAndDontSave;

		public void ShowSortingOrder(float density = 1)
		{
			HideSorrtingOrder();
			Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Editor Default Resources/NodeRender.png");
			for (int i = 0; neighbourNodes != null && i < neighbourNodes.Count; i++)
			{
				if (neighbourNodes[i] == null)
					continue;
				float distance = Vector3.Distance(transform.localPosition, neighbourNodes[i].transform.position) * density;
				float normalValue = 1f / distance;
				GameObject obj = new GameObject("SortingOrderObject");
				obj.hideFlags = Flag;
				SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
				spriteRenderer.sprite = sp;
				spriteRenderer.sortingOrder = sortingOrder;
				obj.transform.parent = transform;
				obj.transform.localPosition = transform.InverseTransformPoint(transform.localPosition);
				obj.transform.localScale = new Vector3(distance, 0.05f, 1);
				Vector3 dir = neighbourNodes[i].transform.localPosition - transform.localPosition;
				float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
				obj.transform.eulerAngles = Vector3.forward * angle;

			}
		}

		private void OnShortingOrderChange()
		{
			SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < spriteRenderers.Length; i++)
			{
				if (spriteRenderers[i].gameObject.name.Equals("SortingOrderObject") && spriteRenderers[i].gameObject.hideFlags == Flag)
				{
					spriteRenderers[i].sortingOrder = sortingOrder;
				}
			}
		}

		[OnInspectorGUI]
		private void UpdatePathSortingOrder()
		{
			SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < spriteRenderers.Length; i++)
			{
				if (spriteRenderers[i].gameObject.name.Equals("SortingOrderObject") && spriteRenderers[i].gameObject.hideFlags == Flag)
				{
					spriteRenderers[i].sortingOrder = sortingOrder;
				}
			}
		}

		public void HideSorrtingOrder()
		{
			List<Transform> renderTransform = new List<Transform>();
			for (int i = 0; i < transform.childCount; i++)
			{
				if (transform.GetChild(i).name.Equals("SortingOrderObject") && transform.GetChild(i).gameObject.hideFlags == Flag)
				{
					renderTransform.Add(transform.GetChild(i));
				}
			}
			for (int i = 0; i < renderTransform.Count; i++)
			{
				DestroyImmediate(renderTransform[i].gameObject);
			}
		}

		private void OnDrawGizmos()
		{
#if UNITY_EDITOR
			//Path p = GetComponentInParent<Path>();
			//if (p == null || !p.canShowPathEditor)
			//{
			//	if (textMeshPro != null)
			//		DestroyImmediate(textMeshPro.gameObject);
			//	return;
			//}
#endif
			Gizmos.color = Color.blue;
			var position = transform.position;
			Gizmos.DrawWireSphere(transform.position, 0.1f);
			Vector3 dir;
			Quaternion quaternion;
			float angle;
			for (int i = 0; neighbourNodes != null && i < neighbourNodes.Count; i++)
			{
				if (neighbourNodes[i] != null)
				{
					Gizmos.color = Color.black;
					Gizmos.DrawLine(transform.position, neighbourNodes[i].transform.position);
					dir = neighbourNodes[i].transform.localPosition - transform.localPosition;
					angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
					quaternion = Quaternion.AngleAxis(angle, Vector3.forward) * Quaternion.Euler(0, -90, -270);
					Gizmos.color = Color.red;
					if (ArrowMesh)
						Gizmos.DrawMesh(ArrowMesh, position + dir.normalized * 0.15f, quaternion);
					if (textMeshPro == null)
					{
						GameObject gameObject = new GameObject("tetstst");
						gameObject.hideFlags = HideFlags.HideAndDontSave;
						gameObject.transform.SetParent(transform);
						textMeshPro = gameObject.AddComponent<TextMeshPro>();
						textMeshPro.alignment = TextAlignmentOptions.Center;
						textMeshPro.enableAutoSizing = true;
						textMeshPro.fontSizeMin = 0;
						textMeshPro.fontSizeMax = 1.5f;
						textMeshPro.color = Color.red;
					}
					if (textMeshPro != null)
					{
						textMeshPro.transform.SetParent(transform);
						textMeshPro.sortingOrder = 10000;
						textMeshPro.transform.localPosition = Vector3.zero;
						textMeshPro.text = transform.GetSiblingIndex().ToString();
					}
				}
			}
			DrarQueueGizmos();
		}

		private void DrarQueueGizmos()
		{
			if (nodeType != NodeType.QueueNode)
				return;
			Gizmos.color = Color.green;
			for (int i = 0; i < queuePoints.Length; i++)
			{
				Gizmos.DrawWireSphere(queuePoints[i].position, 0.1f);
			}
		}

#endif

		#endregion
	}

	public enum NodeType
	{
		Node,
		QueueNode
	}
}