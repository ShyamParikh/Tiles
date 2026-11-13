#if OTM_A_Star
using com.otm.manager;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.AStar
{
	public abstract class AStarPathFinding<T, K> : SerializedManager<T> where T : SerializedManager<T> where K : BaseNode
	{
		#region protected variables

		[SerializeField] protected int row;
		[SerializeField] protected int column;
		[SerializeField] protected Vector2 nodeSize;
		[SerializeField] protected Vector2 space;
		[SerializeField] protected K nodePrefab;
		[SerializeField] protected K[,] grid;

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
				d = Mathf.Abs(worldPosition.y - grid[i, 0].transform.position.y);
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
			List<BaseNode> openSet = new List<BaseNode>();
			HashSet<BaseNode> closedSet = new HashSet<BaseNode>();
			openSet.Add(startNode);

			while (openSet.Count > 0)
			{
				BaseNode currentNode = openSet[0];
				for (int i = 1; i < openSet.Count; i++)
				{
					if (openSet[i].FCost < currentNode.FCost || (openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost))
					{
						currentNode = openSet[i];
					}
				}

				openSet.Remove(currentNode);
				closedSet.Add(currentNode);

				if (currentNode == targetNode)
				{
					return RetracePath(startNode, targetNode);
				}

				foreach (BaseNode neighbour in currentNode.Neighbours)
				{
					if (!neighbour.IsWalkable() || closedSet.Contains(neighbour)) continue;

					int newCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour);
					if (newCostToNeighbour < neighbour.GCost || !openSet.Contains(neighbour))
					{
						neighbour.GCost = newCostToNeighbour;
						neighbour.HCost = GetDistance(neighbour, targetNode);
						neighbour.Parent = currentNode;

						if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
					}
				}
			}
			return null;
		}

		[Button]
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
					grid[i, j].name = "node-[" + i + "," + j + "]";
					pos.x += nodeSize.x + space.x;
				}
				pos.x = topLeft.x;
				pos.y -= nodeSize.y + space.y;
			}
			SetNeighbours();
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

		#endregion

		#region private variables

		private List<K> RetracePath(K startNode, K endNode)
		{
			List<K> path = new List<K>();
			BaseNode currentNode = endNode;

			while (currentNode != null && currentNode != startNode)
			{
				path.Add(currentNode as K);
				currentNode = currentNode.Parent;
			}
			path.Add(startNode);
			path.Reverse();
			return path;
		}

		private int GetDistance(BaseNode a, BaseNode b)
		{
			int dstX = Mathf.Abs(a.RowId - b.RowId);
			int dstY = Mathf.Abs(a.ColumnId - b.ColumnId);

			if (dstX > dstY)
				return 14 * dstY + 10 * (dstX - dstY);
			return 14 * dstX + 10 * (dstY - dstX);
		}

		private void SetNeighbours()
		{
			List<BaseNode> neighbours = new List<BaseNode>();
			BaseNode node;
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

		private K GetNode(int row, int column)
		{
			if (row >= 0 && row < grid.GetLength(0) && column >= 0 && column < grid.GetLength(1))
				return grid[row, column];
			return null;
		}

		#endregion
	}
}
#endif