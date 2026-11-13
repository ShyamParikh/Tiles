using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace com.otm.utility
{
	public static class PlayerPrefsHelper
	{
		#region private variables

		private static Dictionary<string, int> intPrefb = new Dictionary<string, int>();
		private static Dictionary<string, string> stringPrefb = new Dictionary<string, string>();
		private static Dictionary<string, float> floatPrefb = new Dictionary<string, float>();
		private static Dictionary<string, List<Action>> onValueChange = new Dictionary<string, List<Action>>();

		#endregion

		#region propertice

		public static bool CanSaveData { get; set; }

		#endregion


		#region public methods

		public static int GetInt(string key, int defaultValue = 0)
		{
			if (intPrefb == null || string.IsNullOrEmpty(key))
				return defaultValue;
			if (!intPrefb.ContainsKey(key))
			{
				intPrefb.Add(key, PlayerPrefs.GetInt(key, defaultValue));
			}

			return intPrefb[key];
		}

		public static void SetInt(string key, int value)
		{
			if (intPrefb == null)
				intPrefb = new Dictionary<string, int>();

			if (!intPrefb.ContainsKey(key))
				intPrefb.Add(key, value);
			else
				intPrefb[key] = value;
			if (CanSaveData)
			{
				PlayerPrefs.SetInt(key, value);
				PlayerPrefs.Save();
			}
			OnValueChange(key);
		}

		public static string GetString(string key, string defaultValue = "")
		{
			if (stringPrefb == null || string.IsNullOrEmpty(key))
				return defaultValue;
			if (!stringPrefb.ContainsKey(key))
			{
				stringPrefb.Add(key, PlayerPrefs.GetString(key, defaultValue));
			}

			return stringPrefb[key];
		}

		public static void SetString(string key, string value)
		{
			if (stringPrefb == null)
				stringPrefb = new Dictionary<string, string>();
			if (!stringPrefb.ContainsKey(key))
				stringPrefb.Add(key, value);
			else
				stringPrefb[key] = value;
			if (CanSaveData)
			{
				PlayerPrefs.SetString(key, value);
				PlayerPrefs.Save();
			}
			OnValueChange(key);
		}


		public static float GetFloat(string key, float defaultValue = 0)
		{
			if (floatPrefb == null || string.IsNullOrEmpty(key))
				return defaultValue;
			if (!floatPrefb.ContainsKey(key))
			{
				floatPrefb.Add(key, PlayerPrefs.GetFloat(key, defaultValue));
			}

			return floatPrefb[key];
		}

		public static void SetFloat(string key, float value)
		{
			if (floatPrefb == null)
				floatPrefb = new Dictionary<string, float>();
			if (!floatPrefb.ContainsKey(key))
				floatPrefb.Add(key, value);
			else
				floatPrefb[key] = value;
			if (CanSaveData)
			{
				PlayerPrefs.SetFloat(key, value);
				PlayerPrefs.Save();
			}
			OnValueChange(key);
		}

		public static void SetBool(string key, bool value)
		{
			SetInt(key + "_bool", value ? 1 : 0);
		}

		public static bool GetBool(string key, bool defaultValue = false)
		{
			return GetInt(key + "_bool", defaultValue ? 1 : 0) == 1;
		}

		public static void SaveData()
		{
			if (intPrefb != null)
			{
				foreach (var v in intPrefb)
					PlayerPrefs.SetInt(v.Key, v.Value);
			}

			if (stringPrefb != null)
			{
				foreach (var v in stringPrefb)
					PlayerPrefs.SetString(v.Key, v.Value);
			}

			if (floatPrefb != null)
			{
				foreach (var v in floatPrefb)
					PlayerPrefs.SetFloat(v.Key, v.Value);
			}

			PlayerPrefs.Save();
		}

		public static void DeleteKey(string key)
		{
			PlayerPrefs.DeleteKey(key);
			if (intPrefb != null && intPrefb.ContainsKey(key))
				intPrefb.Remove(key);
			if (stringPrefb != null && stringPrefb.ContainsKey(key))
				stringPrefb.Remove(key);
			if (floatPrefb != null && floatPrefb.ContainsKey(key))
				floatPrefb.Remove(key);
		}

		public static void DeleteAllkey()
		{
			PlayerPrefs.DeleteAll();
			intPrefb?.Clear();
			stringPrefb?.Clear();
			floatPrefb?.Clear();
		}

		public static bool HasKey(string key)
		{
			return PlayerPrefs.HasKey(key) || PlayerPrefs.HasKey(key + "_bool");
		}

		public static void RegisterEvent(string key, Action action)
		{
			if (!onValueChange.ContainsKey(key))
				onValueChange.Add(key, new List<Action>());
			onValueChange[key].Add(action);
		}

		public static void RemoveEvent(string key, Action action)
		{
			if (onValueChange.ContainsKey(key))
				onValueChange[key].Remove(action);
		}

		public static string GetJson()
		{
			DataWapper dataWapper = new DataWapper();
			dataWapper.intValues = JsonConvert.SerializeObject(intPrefb);
			dataWapper.floatValues = JsonConvert.SerializeObject(floatPrefb);
			dataWapper.stringValues = JsonConvert.SerializeObject(stringPrefb);

			return JsonConvert.SerializeObject(dataWapper);
		}

		public static void Save(DataWapper dataWapper)
		{
			intPrefb = JsonConvert.DeserializeObject<Dictionary<string, int>>(dataWapper.intValues);
			floatPrefb = JsonConvert.DeserializeObject<Dictionary<string, float>>(dataWapper.floatValues);
			stringPrefb = JsonConvert.DeserializeObject<Dictionary<string, string>>(dataWapper.stringValues);
			SaveData();
		}


#if UNITY_EDITOR
		[UnityEditor.MenuItem("Player Prefs/ClearPlayerPrefs %&#P", priority = 0)]
		public static void ClearPlayerPrefs()
		{
			PlayerPrefs.DeleteAll();
		}

		[UnityEditor.MenuItem("Player Prefs/Save Data to file", priority = 1)]
		public static void SavePrefabToFile()
		{
			string folderName = "EditorPlayerPrefabData";
			string fileName = Environment.MachineName + "-" + DateTime.Now;
			fileName = fileName.Replace(" ", "_");
			fileName = fileName.Replace("-", "_");
			fileName = fileName.Replace(":", "_");
			string filePath = Application.dataPath + "/" + folderName + "/" + fileName + ".txt";
			if (!UnityEditor.AssetDatabase.AssetPathExists("Assets/" + folderName))
				UnityEditor.AssetDatabase.CreateFolder("Assets", folderName);
			FileInfo fileInfo = new FileInfo(filePath);
			if (!fileInfo.Exists)
			{
				FileStream fileStream = fileInfo.Create();
				fileStream.Close();
			}

			FileStream stream = fileInfo.Open(FileMode.Open);
			byte[] datas = Encoding.ASCII.GetBytes(GetJson());
			stream.Write(datas, 0, datas.Length);
			stream.Close();
			UnityEditor.AssetDatabase.Refresh();
		}

		[UnityEditor.MenuItem("Player Prefs/Load Prefab From File", priority = 2)]
		public static void LoadPrefabFormFile()
		{
			string path = UnityEditor.EditorUtility.OpenFilePanel("PLAYER PREFAB", Application.dataPath + "/EditorPlayerPrefabData", "txt");
			if (string.IsNullOrEmpty(path))
				return;
			byte[] bytes = File.ReadAllBytes(path);
			string data = Encoding.ASCII.GetString(bytes);
			DataWapper dataWapper = JsonConvert.DeserializeObject<DataWapper>(data);
			DeleteAllkey();
			Save(dataWapper);
		}
#endif

		#endregion

		#region private methods

		private static void OnValueChange(string key)
		{
			if (onValueChange.ContainsKey(key))
			{
				for (int i = 0; i < onValueChange[key].Count; i++)
				{
					if (onValueChange[key][i] != null)
						onValueChange[key][i].Invoke();
				}
			}
		}

		#endregion

		public class DataWapper
		{
			public string intValues;
			public string floatValues;
			public string stringValues;
		}
	}
}