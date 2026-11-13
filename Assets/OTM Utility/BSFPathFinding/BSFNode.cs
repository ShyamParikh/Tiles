#if BSF_Path_Finding
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.BSFPathfinding
{
	public abstract class BSFNode : MonoBehaviour
	{
		#region private variables
		[SerializeField] private bool walkable;
		[SerializeField] private BSFNode[] neighbours;
		[SerializeField] private int rowId;
		[SerializeField] private int columnId;
		#endregion

		#region propertices
		public int RowId => rowId;
		public int ColumnId => columnId;
		public BSFNode Parent { get; set; }
		public BSFNode[] Neighbours => neighbours;

		#endregion

		#region virtual methods

		public virtual void Init(Vector3 position, int row, int column)
		{
			transform.localPosition = position;
			rowId = row;
			columnId = column;
			gameObject.SetActive(true);
		}

		public virtual void SetNeighbours(List<BSFNode> nodes)
		{
			neighbours = nodes.ToArray();
		}

		public virtual bool IsWalkable()
		{
			return walkable;
		}

		#endregion
	}
}
#endif