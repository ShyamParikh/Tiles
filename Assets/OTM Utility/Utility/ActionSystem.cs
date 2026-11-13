using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.otm.utility
{
	[HideInInspector]
	public class ActionSystem
	{
		#region private variables

		private HashSet<Action> actions = new HashSet<Action>();
		private readonly Dictionary<string, Action> actionMapping = new Dictionary<string, Action>();
		private Action[] cachedActions;
		private bool isDirty = true;

		#endregion

		#region public methods

		public void RegisterEvent(Action action)
		{
			if (actions == null)
				actions = new HashSet<Action>();
			if (actions.Add(action))
			{
				isDirty = true;
			}
		}

		public void DeRegisterEvent(Action action)
		{
			if (actions == null)
				return;
			if (actions.Remove(action))
			{
				isDirty = true;
			}
		}

		public void RegisterEvent(string id, Action action)
		{
			actionMapping[id] = action;
		}

		public void DeRegisterEvent(string id)
		{
			actionMapping.Remove(id);
		}

		public void Invoke()
		{
			if ((isDirty || cachedActions == null) && actions != null)
			{
				cachedActions = actions.ToArray();
				isDirty = false;
			}

			if (cachedActions != null)
			{
				for (int i = 0; i < cachedActions.Length; i++)
				{
					cachedActions[i]?.Invoke();
				}
			}

			if (actionMapping != null)
			{
				foreach (var action in actionMapping)
				{
					action.Value?.Invoke();
				}
			}
		}

		public void ClearEvents()
		{
			actions?.Clear();
			actionMapping?.Clear();
			cachedActions = null;
			isDirty = true;
		}

		#endregion
	}

	[HideInInspector]
	public class ActionSystem<T>
	{
		#region private variables

		private HashSet<Action<T>> actions = new HashSet<Action<T>>();
		private readonly Dictionary<string, Action<T>> actionMapping = new Dictionary<string, Action<T>>();
		private Action<T>[] cachedActions;
		private bool isDirty = true;

		#endregion

		#region public methods

		public void RegisterEvent(Action<T> action)
		{
			if (actions == null)
				actions = new HashSet<Action<T>>();
			if (actions.Add(action))
			{
				isDirty = true;
			}
		}

		public void DeRegisterEvent(Action<T> action)
		{
			if (actions == null)
				return;
			if (actions.Remove(action))
			{
				isDirty = true;
			}
		}

		public void RegisterEvent(string id, Action<T> action)
		{
			actionMapping[id] = action;
		}

		public void DeRegisterEvent(string id)
		{
			actionMapping.Remove(id);
		}

		public void Invoke(T value)
		{
			if ((isDirty || cachedActions == null) && actions != null)
			{
				cachedActions = actions.ToArray();
				isDirty = false;
			}

			if (cachedActions != null)
			{
				for (int i = 0; i < cachedActions.Length; i++)
				{
					cachedActions[i]?.Invoke(value);
				}
			}

			if (actionMapping != null)
			{
				foreach (var action in actionMapping)
				{
					action.Value?.Invoke(value);
				}
			}
		}

		public void ClearEvents()
		{
			actions?.Clear();
			actionMapping?.Clear();
			cachedActions = null;
			isDirty = true;
		}

		#endregion
	}

	[HideInInspector]
	public class ActionSystem<T, K>
	{
		#region private variables

		private HashSet<Action<T, K>> actions = new HashSet<Action<T, K>>();
		private readonly Dictionary<string, Action<T, K>> actionMapping = new Dictionary<string, Action<T, K>>();
		private Action<T, K>[] cachedActions;
		private bool isDirty = true;

		#endregion

		#region public methods

		public void RegisterEvent(Action<T, K> action)
		{
			if (actions == null)
				actions = new HashSet<Action<T, K>>();

			if (actions.Add(action))
			{
				isDirty = true;
			}
		}

		public void DeRegisterEvent(Action<T, K> action)
		{
			if (actions == null)
				return;
			if (actions.Remove(action))
			{
				isDirty = true;
			}
		}

		public void RegisterEvent(string id, Action<T, K> action)
		{
			actionMapping[id] = action;
		}

		public void DeRegisterEvent(string id)
		{
			actionMapping.Remove(id);
		}

		public void Invoke(T value, K value2)
		{
			if ((isDirty || cachedActions == null) && actions != null)
			{
				cachedActions = actions.ToArray();
				isDirty = false;
			}

			if (cachedActions != null)
			{
				for (int i = 0; i < cachedActions.Length; i++)
				{
					cachedActions[i]?.Invoke(value, value2);
				}
			}

			if (actionMapping != null)
			{
				foreach (var action in actionMapping)
				{
					action.Value?.Invoke(value, value2);
				}
			}
		}

		public void ClearEvents()
		{
			actions?.Clear();
			actionMapping?.Clear();
			cachedActions = null;
			isDirty = true;
		}

		#endregion
	}

	[HideInInspector]
	public class ActionSystem<T, K, U>
	{
		#region private variables

		private HashSet<Action<T, K, U>> actions = new HashSet<Action<T, K, U>>();
		private readonly Dictionary<string, Action<T, K, U>> actionMapping = new Dictionary<string, Action<T, K, U>>();
		private Action<T, K, U>[] cachedActions;
		private bool isDirty = true;

		#endregion

		#region public methods

		public void RegisterEvent(Action<T, K, U> action)
		{
			if (actions == null)
				actions = new HashSet<Action<T, K, U>>();
			if (actions.Add(action))
			{
				isDirty = true;
			}
		}

		public void DeRegisterEvent(Action<T, K, U> action)
		{
			if (actions == null)
				return;
			if (actions.Remove(action))
			{
				isDirty = true;
			}
		}

		public void RegisterEvent(string id, Action<T, K, U> action)
		{
			actionMapping[id] = action;
		}

		public void DeRegisterEvent(string id)
		{
			actionMapping.Remove(id);
		}

		public void Invoke(T value, K value2, U value3)
		{
			if ((isDirty || cachedActions == null) && actions != null)
			{
				cachedActions = actions.ToArray();
				isDirty = false;
			}

			if (cachedActions != null)
			{
				for (int i = 0; i < cachedActions.Length; i++)
				{
					cachedActions[i]?.Invoke(value, value2, value3);
				}
			}

			if (actionMapping != null)
			{
				foreach (var action in actionMapping)
				{
					action.Value?.Invoke(value, value2, value3);
				}
			}
		}

		public void ClearEvents()
		{
			actions?.Clear();
			actionMapping?.Clear();
			cachedActions = null;
			isDirty = true;
		}

		#endregion
	}
}