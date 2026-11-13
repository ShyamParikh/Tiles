using com.otm.utility;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.manager
{
	public class LevelIndexHelper : RunTimeManager<LevelIndexHelper>
	{
		#region private variables

		[ShowInInspector] private Dictionary<string, Dictionary<string, int>> levels = new Dictionary<string, Dictionary<string, int>>() { { "Common", new Dictionary<string, int>() } };

		private const string LEVEL_HELPER_DATA = "LevelHelperData";
		private const string COMMON_FILTER = "Common";
		#endregion

		#region unity callback

		public override void Awake()
		{
			SetData();
			base.Awake();
		}

		#endregion

		#region public methods

		public void SetLevelIndex(string key, int index, string filter = "Common")
		{
			index = Mathf.Clamp(index, 0, index);
			if (!levels.ContainsKey(filter))
				levels.Add(filter, new Dictionary<string, int>());
			if (!levels[filter].ContainsKey(key))
				levels[filter].Add(key, index);
			else
				levels[filter][key] = index;
			SaveData();
		}

		public void SetLevelNumber(string key, int number, string filter = "Common")
		{
			SetLevelIndex(key, number - 1, filter);
		}

		public int GetLevelIndex(string key, string filter = "Common")
		{
			if (!levels.ContainsKey(filter))
				return 0;
			if (levels[filter].ContainsKey(key))
				return levels[filter][key];
			return 0;
		}

		public int GetLevelNumber(string key, string filter = "Common")
		{
			return (GetLevelIndex(key, filter) + 1);
		}

		public int GetTotalLevelIndex(string filter = "Common")
		{
			if (!levels.ContainsKey(filter))
				return 0;
			int total = 0;
			foreach (int item in levels[filter].Values)
			{
				total += item;
			}

			return total;
		}

		public int GetTotalLevelNumber(string filter = "Common")
		{
			if (!levels.ContainsKey(filter))
				return 1;
			int total = 0;
			foreach (int item in levels[filter].Values)
			{
				total += (item + 1);
			}

			return (total > 0) ? total : 1;
		}

		#endregion

		#region private methods

		private void SetData()
		{
			string data = PlayerPrefsHelper.GetString(LEVEL_HELPER_DATA, "");
			if (!string.IsNullOrEmpty(data))
			{
				levels = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(data);
			}
		}

		private void SaveData()
		{
			if (levels == null)
				return;
			string jsonData = JsonConvert.SerializeObject(levels);
			PlayerPrefsHelper.SetString(LEVEL_HELPER_DATA, jsonData);
		}

		#endregion
	}
}
