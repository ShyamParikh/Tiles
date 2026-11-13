using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace com.otm.editor
{
	public class UtilityActivationWindow : OdinEditorWindow
	{
		#region private variables

		private List<string> symboles = new List<string>();

		private Dictionary<string, string> otmSymboles = new Dictionary<string, string>()
		{
			{"OTM_Firebase","Firebase"},
			{"OTM_IAP","In App Purchase"},
			{"UNITY_FMODE","F-Mode"},
			{"OTM_Addressable","Addressable"},
			{"MOBILE_NOTIFICATION","Local Notifications"},
			{"OTM_RATE_US","Rate us"},
			{"ANDROID_IN_APP_RATE_US","adroid in app Rate us"},
			{"UNITY_SPINE","Spine"},
			{"OTM_Offer","Offer"},
			{"OTM_A_Star","A* Path Finding"},
			{"BSF_Path_Finding","BSF Path Finding"},
			{"OTM_APPLOVIN","AppLovin Ads"},
			{"OTM_Analytics","Analytics"},
		};

		private Dictionary<string, bool> activationMapping = new Dictionary<string, bool>();

		#endregion

		[MenuItem("OTM TOOLS/UtilityActivationWindow")]
		public static void ShowWindow()
		{
			UtilityActivationWindow window = GetWindow<UtilityActivationWindow>("Utility Activation Window");
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			string smb = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
			symboles = smb.Split(';').ToList();
			foreach (var symbole in otmSymboles)
			{
				if (!activationMapping.ContainsKey(symbole.Key))
					activationMapping.Add(symbole.Key, symboles.Contains(symbole.Key));
				else
					activationMapping[symbole.Key] = symboles.Contains(symbole.Key);
			}
		}

		protected override void OnGUI()
		{
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.Space(5);
			GUIStyle titleStyle = new GUIStyle
			{
				alignment = TextAnchor.MiddleCenter,
				fontSize = 20,
				fontStyle = FontStyle.Bold
			};
			titleStyle.normal.textColor = Color.white;
			EditorGUILayout.LabelField("Activation Window", titleStyle);
			EditorGUILayout.Space(5);
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical("box");
			foreach (var symbole in otmSymboles)
			{
				activationMapping[symbole.Key] = EditorGUILayout.Toggle(symbole.Value, activationMapping[symbole.Key]);
			}
			if (GUILayout.Button("Apply Symboles"))
				ApplySettings();
			EditorGUILayout.EndVertical();
			base.OnGUI();
		}

		#region private methods

		private void ApplySettings()
		{
			List<string> newSymbols = new List<string>(symboles);
			foreach (var item in activationMapping)
			{
				if (item.Value && !newSymbols.Contains(item.Key))
					newSymbols.Add(item.Key);
				else if (!item.Value)
					newSymbols.Remove(item.Key);
			}
			string sm = "";
			for (int i = 0; i < newSymbols.Count; i++)
			{
				sm += newSymbols[i] + ";";
			}
			symboles = new List<string>(newSymbols);
			PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, sm);
		}

		#endregion
	}
}
