using com.otm.manager;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.manager
{
	public class GlobalIconHelper : SerializedManager<GlobalIconHelper>
	{
		#region private variables

		[SerializeField] private Dictionary<string, Sprite> icons = new Dictionary<string, Sprite>();

		#endregion


		#region static public methods

		public static Sprite GetIcon(string key)
		{
			if (Instance == null)
				return null;
			if (Instance.icons.ContainsKey(key))
				return Instance.icons[key];
			return null;
		}

		#endregion
	}
}
