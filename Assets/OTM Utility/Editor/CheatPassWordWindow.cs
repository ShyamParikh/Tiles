using com.otm.utility;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace com.otm.editor
{
	public class CheatPassWordWindow : OdinEditorWindow
	{
		[DictionaryDrawerSettings(KeyLabel = "Password", ValueLabel = "Hash")]
		public Dictionary<string, string> passwords = new Dictionary<string, string>();

		[MenuItem("GameTool/System/CheatPasswordWindow")]
		private static void OpenWindow()
		{
			GetWindow<CheatPassWordWindow>().Show();
		}
		protected override void OnEnable()
		{
			base.OnEnable();
			GetPassWordFromFile();
		}

		[Button]
		public void SavePassword()
		{
			if (passwords == null)
				return;
			SavePasswordToFile();
		}

		[Button]
		public void CalculateHash()
		{
			Dictionary<string, string> calculatedPassword = new Dictionary<string, string>();
			foreach (var item in passwords)
			{
				calculatedPassword.Add(item.Key, SystemUtility.GetHash(item.Key));
			}
			passwords = calculatedPassword;
		}

		private void SavePasswordToFile()
		{
			FileInfo fileInfo = GetPasswordFile();
			FileStream stream = fileInfo.Open(FileMode.Open);
			byte[] datas = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(passwords));
			stream.Write(datas, 0, datas.Length);
			stream.Close();
			AssetDatabase.Refresh();
		}

		private FileInfo GetPasswordFile()
		{
			string folderName = "CheatPassword";
			string filePath = Application.dataPath + "/" + folderName + "/ cheatPassword.txt";
			if (!AssetDatabase.IsValidFolder("Assets/" + folderName))
				AssetDatabase.CreateFolder("Assets", folderName);
			FileInfo fileInfo = new FileInfo(filePath);
			if (!fileInfo.Exists)
			{
				FileStream fileStream = fileInfo.Create();
				fileStream.Close();
			}
			return fileInfo;
		}

		private void GetPassWordFromFile()
		{
			FileInfo fileInfo = GetPasswordFile();
			byte[] bytes = File.ReadAllBytes(fileInfo.FullName);
			string data = Encoding.ASCII.GetString(bytes);
			if (string.IsNullOrEmpty(data))
				passwords = new Dictionary<string, string>();
			else
				passwords = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
		}
	}
}
