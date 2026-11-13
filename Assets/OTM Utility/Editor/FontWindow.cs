using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.otm.editor
{
	public class FontWindow : OdinEditorWindow
	{
		#region public variables

		public Font font;
		public char c;
		[Multiline] public string unicodes;

		#endregion
		[MenuItem("OTM TOOLS/FontWindow")]
		public static void ShowWindow()
		{
			FontWindow window = GetWindow<FontWindow>("FontWindow");
		}

		[Button]
		private void FontButton()
		{
			CharacterInfo characterInfo;
			font.GetCharacterInfo(c, out characterInfo);
			List<int> fontIndex = new List<int>();
			for (int i = 0; i < font.characterInfo.Length; i++)
			{
				fontIndex.Add(font.characterInfo[i].index);
			}
			Debug.Log(JsonConvert.SerializeObject(fontIndex));
		}


		[Button]
		private void GetCharacterUnicode()
		{
			CharacterInfo[] characters = font.characterInfo;
			List<string> hexa = new List<string>();
			for (int i = 0; i < characters.Length; i++)
			{
				string uniCodeHex = characters[i].index.ToString("X4");
				hexa.Add(uniCodeHex);
			}
			SaveJsonDataToFile(hexa, font.name);
		}


		private void SaveJsonDataToFile(List<string> data, string fontName)
		{
			string json = JsonConvert.SerializeObject(data);
			json = json.Replace("\"", "");
			json = json.Replace("[", "");
			json = json.Replace("]", "");
			unicodes = json;
		}
	}
}
