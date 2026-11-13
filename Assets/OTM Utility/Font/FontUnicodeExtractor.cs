using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class FontUnicodeExtractor : SerializedMonoBehaviour
{
	public Font font;
	public char c;


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
		string filePath = Application.dataPath + "/FontTest/" + fontName + ".txt";
		Debug.Log(filePath);
		FileInfo fileInfo = new FileInfo(filePath);
		if (!fileInfo.Exists)
		{
			FileStream fileStream = fileInfo.Create();
			fileStream.Close();
		}

		FileStream stream = fileInfo.Open(FileMode.Open);
		byte[] datas = Encoding.ASCII.GetBytes(json);
		stream.Write(datas, 0, datas.Length);
		stream.Close();
#if UNITY_EDITOR
		UnityEditor.AssetDatabase.Refresh();
#endif
	}
}


