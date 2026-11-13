#if BSF_Path_Finding
using com.otm.manager;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.BSFPathfinding
{
	public abstract class BSFPathFinding<T, K> : SerializedManager<T> where T : SerializedManager<T> where K : BSFNode
	{
		#region protected variables

		[SerializeField] protected int row;
		[SerializeField] protected int column;
		[SerializeField] protected Vector2 nodeSize;
		[SerializeField] protected Vector2 space;
		[SerializeField] protected K nodePrefab;
		[SerializeField, OnValueChanged("SetAxis")] protected Axis axis;
		[SerializeField] protected K[,] grid;

		#endregion

		#region propertices

		public K[,] Grid => grid;

		#endregion

		#region public methods

		public K GetNodeFromWorldPoint(Vector3 worldPosition)
		{
			int r = 0;
			int c = 0;
			float distance = float.MaxValue;
			float d;
			for (int i = 0; i < grid.GetLength(0); i++)
			{
				d = (axis == Axis.XY) ? Mathf.Abs(worldPosition.y - grid[i, 0].transform.position.y) : Mathf.Abs(worldPosition.z - grid[i, 0].transform.position.z);
				if (distance > d)
				{
					distance = d; r = i;
				}
			}
			distance = float.MaxValue;
			for (int i = 0; i < grid.GetLength(1); i++)
			{
				d = Mathf.Abs(worldPosition.x - grid[r, i].transform.position.x);
				if (distance > d)
				{
					distance = d; c = i;
				}
			}
			return grid[r, c];
		}

		public List<K> FindPath(Vector3 startPos, Vector3 targetPos)
		{
			K startNode = GetNodeFromWorldPoint(startPos);
			K targetNode = GetNodeFromWorldPoint(targetPos);
			return FindPath(startNode, targetNode);
		}

		public List<K> FindPath(K startNode, K targetNode)
		{
			Queue<K> queue = new Queue<K>();
			HashSet<K> visited = new HashSet<K>();

			queue.Enqueue(startNode);
			visited.Add(startNode);

			while (queue.Count > 0)
			{
				K current = queue.Dequeue();

				if (current == targetNode)
				{
					return RetracePath(startNode, targetNode);
				}

				foreach (K neighbour in current.Neighbours)
				{
					if (!visited.Contains(neighbour) && neighbour.IsWalkable())
					{
						neighbour.Parent = current;
						queue.Enqueue(neighbour);
						visited.Add(neighbour);
					}
				}
			}
			return null;
		}

		[Button]
		public void CreateGrid()
		{
			CreateGrid(row, column, nodeSize, space);
		}

		public void CreateGrid(int row, int column)
		{
			CreateGrid(row, column, nodeSize, space);
		}

		public void CreateGrid(int row, int column, Vector2 nodeSize)
		{
			CreateGrid(row, column, nodeSize, space);
		}
		public void CreateGrid(int row, int column, Vector2 nodeSize, Vector2 space)
		{
			this.row = row;
			this.column = column;
			this.space = space;
			this.nodeSize = nodeSize;
			grid = new K[row, column];
			Vector2 rowCol = new Vector2(column, row);
			Vector2 topLeft = ((nodeSize * rowCol + (space * rowCol)) * 0.5f);
			topLeft.x *= -1;
			topLeft.x += (nodeSize.x + space.x) * 0.5f;
			topLeft.y -= (nodeSize.y + space.y) * 0.5f;
			Vector2 pos = topLeft;
			for (int i = 0; i < row; i++)
			{
				for (int j = 0; j < column; j++)
				{
					grid[i, j] = Instantiate(nodePrefab, transform);
					grid[i, j].Init(pos, i, j);
					grid[i, j].transform.localScale = new Vector3(nodeSize.x, nodeSize.y, 1);
					grid[i, j].name = "node-[" + i + "," + j + "]";
					pos.x += nodeSize.x + space.x;
				}
				pos.x = topLeft.x;
				pos.y -= nodeSize.y + space.y;
			}
			SetNeighbours();
			SetAxis();
		}

		public void ClearGrid()
		{
			if (grid == null)
				return;
			for (int i = 0; i < grid.GetLength(0); i++)
			{
				for (int j = 0; j < grid.GetLength(1); j++)
				{
					Destroy(grid[i, j].gameObject);
				}
			}
			grid = new K[0, 0];
		}

		public List<K> GetRow(int rowId)
		{
			List<K> list = new List<K>();
			if (grid.GetLength(0) < rowId || rowId >= 0)
			{
				for (int i = 0; i < grid.GetLength(1); i++)
				{
					list.Add(grid[rowId, i]);
				}
			}
			return list;
		}

		public List<K> GetColumn(int colmnId)
		{
			List<K> list = new List<K>();
			if (grid.GetLength(1) < colmnId || colmnId >= 0)
			{
				for (int i = 0; i < grid.GetLength(0); i++)
				{
					list.Add(grid[i, colmnId]);
				}
			}
			return list;
		}
		public K GetNode(int row, int column)
		{
			if (row >= 0 && row < grid.GetLength(0) && column >= 0 && column < grid.GetLength(1))
				return grid[row, column];
			return null;
		}

		#endregion

		#region private variables

		private List<K> RetracePath(K startNode, K endNode)
		{
			List<K> path = new List<K>();
			BSFNode current = endNode;

			while (current != startNode)
			{
				path.Add(current as K);
				current = current.Parent;
			}
			path.Add(startNode);
			path.Reverse();
			return path;
		}

		private void SetNeighbours()
		{
			List<BSFNode> neighbours = new List<BSFNode>();
			BSFNode node;
			for (int i = 0; i < row; i++)
			{
				for (int j = 0; j < column; j++)
				{
					node = GetNode(i + 1, j);
					if (node != null)
						neighbours.Add(node);
					node = GetNode(i, j + 1);
					if (node != null)
						neighbours.Add(node);
					node = GetNode(i, j - 1);
					if (node != null)
						neighbours.Add(node);
					node = GetNode(i - 1, j);
					if (node != null)
						neighbours.Add(node);
					grid[i, j].SetNeighbours(neighbours);
					neighbours.Clear();
				}
			}
		}

		#endregion

		#region editor methods

		private void SetAxis()
		{
			if (axis == Axis.XY)
				ConvertToXY();
			else
				ConvertToXZ();
		}

		private void ConvertToXY()
		{
			Vector2 rowCol = new Vector2(column, row);
			Vector2 topLeft = ((nodeSize * rowCol + (space * rowCol)) * 0.5f);
			topLeft.x *= -1;
			topLeft.x += (nodeSize.x + space.x) * 0.5f;
			topLeft.y -= (nodeSize.y + space.y) * 0.5f;
			Vector2 pos = topLeft;
			for (int i = 0; i < row; i++)
			{
				for (int j = 0; j < column; j++)
				{
					grid[i, j].Init(pos, i, j);
					grid[i, j].transform.eulerAngles = Vector3.zero;
					grid[i, j].transform.localScale = new Vector3(nodeSize.x, nodeSize.y, 1);
					pos.x += nodeSize.x + space.x;
				}
				pos.x = topLeft.x;
				pos.y -= nodeSize.y + space.y;
			}
		}

		private void ConvertToXZ()
		{
			Vector2 rowCol = new Vector2(column, row);
			Vector2 topLeft = ((nodeSize * rowCol + (space * rowCol)) * 0.5f);
			topLeft.x *= -1;
			topLeft.x += (nodeSize.x + space.x) * 0.5f;
			topLeft.y -= (nodeSize.y + space.y) * 0.5f;
			Vector2 pos = topLeft;
			for (int i = 0; i < row; i++)
			{
				for (int j = 0; j < column; j++)
				{
					grid[i, j].Init(new Vector3(pos.x, 0, pos.y), i, j);
					grid[i, j].transform.eulerAngles = Vector3.left * 90;
					grid[i, j].transform.localScale = new Vector3(nodeSize.x, nodeSize.y, 1);
					pos.x += nodeSize.x + space.x;
				}
				pos.x = topLeft.x;
				pos.y -= nodeSize.y + space.y;
			}
		}

		[Button]
		public void DeleteGrid()
		{
			if (grid == null)
				return;
			for (int i = 0; i < grid.GetLength(0); i++)
			{
				for (int j = 0; j < grid.GetLength(1); j++)
				{
					DestroyImmediate(grid[i, j].gameObject);
				}
			}
			grid = new K[0, 0];
		}

		#endregion

		#region enum

		public enum Axis
		{
			XY,
			XZ
		}

		#endregion
	}
}
#endif