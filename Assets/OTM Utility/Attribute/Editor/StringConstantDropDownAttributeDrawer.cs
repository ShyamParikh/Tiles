using com.otm.utility;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace com.otm.editor
{
	public class StringConstantDropDownAttributeDrawer : OdinAttributeDrawer<StringConstantDropDownAttribute, string>
	{
		#region private veriables
		private string[] values;

		#endregion

		#region overided methods
		protected override void Initialize()
		{
			base.Initialize();
			FillValues();
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			EditorGUILayout.BeginHorizontal();
			if (values != null && values.Length > 0)
				ValueEntry.SmartValue = SirenixEditorFields.Dropdown(label, ValueEntry.SmartValue, values, values);
			else
				SirenixEditorGUI.ErrorMessageBox("NO CONSTNT VARIABLE FOUND");
			EditorGUILayout.EndHorizontal();
		}

		#endregion

		#region private methods

		private void FillValues()
		{
			FieldInfo[] fieldInfos = Attribute.TargetType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string)).ToArray();
			if (fieldInfos.Length <= 0)
				return;
			values = new string[fieldInfos.Length + 2];
			for (int i = 0; i < fieldInfos.Length; i++)
			{
				values[i] = (string)fieldInfos[i].GetValue(null);
			}
			if (string.IsNullOrEmpty(ValueEntry.SmartValue))
				ValueEntry.SmartValue = values[0];
		}

		#endregion

	}
}
