using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.path
{
	public class BasePath<T> : SerializedMonoBehaviour where T : BaseNode
	{
		#region protected variables
		[SerializeField] protected T[] nodes = new T[0];
		#endregion

		#region propertices

		public T[] Nodes { get { return nodes; } set { nodes = value; } }

		public bool IsPathInitlized { get; set; }

		public bool CanClick { get; set; }

		#endregion
#if UNITY_EDITOR
		public bool canShowPathEditor;
#endif

		#region virtual methods
		protected virtual void Start()
		{
			IsPathInitlized = false;
			Init();
			IsPathInitlized = true;
		}

		protected virtual void OnDestroy()
		{
			IsPathInitlized = false;
		}

		public virtual void Init()
		{
			CanClick = true;
			InitNodes();
		}

		public virtual List<T> GetPath(T start, T end)
		{
			List<T> result = new List<T>();
			if (start == end)
			{
				result.Add(start);
			}
			else
			{
				T node = GetShortestPath(start, end);
				while (node)
				{
					result.Add(node);
					node = node.ParentNode as T;
				}

				result.Reverse();
			}

			return result;
		}

		public virtual T GetNode(int nodeId)
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				if (nodes[i].NodeId == nodeId)
					return nodes[i];
			}

			return null;
		}

		#endregion

		#region private methods
		private void InitNodes()
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				nodes[i].Init();
			}
		}
		private T GetShortestPath(T start, T end)
		{
			List<T> unexplored = new List<T>();
			for (int i = 0; i < nodes.Length; i++)
			{
				nodes[i].ResetWeightAndParent();
				unexplored.Add(nodes[i]);
			}

			T startNode = start;
			startNode.Weight = 0;

			while (unexplored.Count > 0)
			{
				unexplored.Sort((x, y) => x.Weight.CompareTo(y.Weight));

				T current = unexplored[0];

				unexplored.Remove(current);

				T currentNode = current;
				List<BaseNode> neighbours = currentNode.NeighbourNodes;
				foreach (T neighNode in neighbours)
				{
					if (unexplored.Contains(neighNode))
					{
						float distance = Vector3.Distance(neighNode.transform.position, current.transform.position);
						distance = (currentNode.Weight) + distance;

						if (distance < neighNode.Weight)
						{
							neighNode.Weight = distance;
							neighNode.ParentNode = current;
						}
					}
				}
			}
			return end;
		}

		#endregion
	}
}
