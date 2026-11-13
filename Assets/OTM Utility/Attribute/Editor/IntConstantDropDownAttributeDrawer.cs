using com.otm.utility;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace com.otm.editor
{
	public class IntConstantDropDownAttributeDrawer : OdinAttributeDrawer<IntConstanrDropDownAttribute, int>
	{
		#region private veriables
		private int[] values;
		private string[] lables;

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
				ValueEntry.SmartValue = SirenixEditorFields.Dropdown(label, ValueEntry.SmartValue, lables);
			else
				SirenixEditorGUI.ErrorMessageBox("NO CONSTNT VARIABLE FOUND");
			EditorGUILayout.EndHorizontal();
		}

		#endregion

		#region private methods

		private void FillValues()
		{
			FieldInfo[] fieldInfos = Attribute.TargetType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(int)).ToArray();
			if (fieldInfos.Length <= 0)
				return;
			values = new int[fieldInfos.Length];
			lables = new string[fieldInfos.Length];
			for (int i = 0; i < fieldInfos.Length; i++)
			{
				values[i] = (int)fieldInfos[i].GetValue(null);
				lables[i] = fieldInfos[i].Name;
			}
		}

		#endregion
	}
}
