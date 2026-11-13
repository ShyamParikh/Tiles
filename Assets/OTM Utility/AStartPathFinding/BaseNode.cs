#if OTM_A_Star
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace com.otm.AStar
{
	public abstract class BaseNode : MonoBehaviour
	{
		#region private variables
		[SerializeField] private bool walkable;
		[SerializeField] private BaseNode[] neighbours;
		[SerializeField] private int rowId;
		[SerializeField] private int columnId;
		#endregion

		#region propertices
		public int FCost => GCost + HCost;
		public int RowId => rowId;
		public int ColumnId => columnId;
		public int GCost { get; set; }
		public int HCost { get; set; }
		public BaseNode Parent { get; set; }
		public BaseNode[] Neighbours => neighbours;

		#endregion

		#region virtual methods

		public virtual void Init(Vector3 position, int row, int column)
		{
			transform.localPosition = position;
			rowId = row;
			columnId = column;
			gameObject.SetActive(true);
		}

		public virtual void SetNeighbours(List<BaseNode> nodes)
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