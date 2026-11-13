using com.otm.manager;
using com.otm.path;
using com.otm.utility;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.manager
{
	public class QueueManager : NoneMonoManager<QueueManager>
	{

		#region public static variables

		public static ActionSystem<BaseNode> OnQueueUpdate = new ActionSystem<BaseNode>();

		#endregion

		#region private variables

		private Dictionary<BaseNode, List<Transform>> queue = new Dictionary<BaseNode, List<Transform>>();

		#endregion

		#region public methods

		public void AddInQueue(BaseNode baseNode, Transform transform)
		{
			if (!queue.ContainsKey(baseNode))
				queue.Add(baseNode, new List<Transform>());
			if (!queue[baseNode].Contains(transform))
			{
				queue[baseNode].Add(transform);
				OnQueueUpdate.Invoke(baseNode);
			}
		}

		public void RemoveFromQueue(Transform transform)
		{
			foreach (var q in queue)
			{
				if (q.Value.Contains(transform))
				{
					q.Value.Remove(transform);
					OnQueueUpdate.Invoke(q.Key);
				}
			}
		}

		public void RemoveFromQueue(BaseNode baseNode, Transform transform, bool raiseEvent = true)
		{
			if (!queue.ContainsKey(baseNode))
				return;
			if (queue[baseNode].Contains(transform))
			{
				queue[baseNode].Remove(transform);
				if (raiseEvent)
					OnQueueUpdate.Invoke(baseNode);
			}
		}

		public void ClearQueue(BaseNode baseNode)
		{
			if (!queue.ContainsKey(baseNode))
				return;
			queue[baseNode].Clear();
		}

		public int GetQueuePoint(BaseNode baseNode, Transform transform)
		{
			if (queue.ContainsKey(baseNode))
			{
				if (queue[baseNode].Contains(transform))
					return queue[baseNode].IndexOf(transform);
				return queue[baseNode].Count;
			}
			return 0;
		}

		public bool IsInQueue(BaseNode baseNode, Transform transform)
		{
			if (!queue.ContainsKey(baseNode))
				return false;
			return queue[baseNode].Contains(transform);
		}

		public int GetQueueCount(BaseNode baseNode)
		{
			if (queue.ContainsKey(baseNode))
				return queue[baseNode].Count;
			return 0;
		}

		#endregion

	}
}
