using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using UnityEditor;
using UnityEngine;

namespace com.otm.editor
{
	public abstract class ChildrenComponentReferenceAttributeDrawer<T, V> : OdinAttributeDrawer<T, V> where T : Attribute where V : Component
	{
		#region private veriables

		private V[] values;
		private string[] names;
		private V selectedValue;
		private int selectedIndex;

		#endregion

		#region overrided methods

		protected override void Initialize()
		{
			base.Initialize();
			selectedValue = ValueEntry.SmartValue;
			FillImages();
		}
		protected override void DrawPropertyLayout(GUIContent label)
		{
			ValueEntry.SmartValue = (V)SirenixEditorFields.UnityObjectField(label, ValueEntry.SmartValue, typeof(V), true);
			if (values == null || values.Length <= 0)
			{
				SirenixEditorGUI.ErrorMessageBox("NO Component Found : " + (typeof(V)));
				return;
			}
			if (names.Length > 0)
			{
				selectedIndex = SirenixEditorFields.Dropdown(selectedIndex, names);
				selectedValue = values[selectedIndex];
				if (selectedValue != ValueEntry.SmartValue)
					ValueEntry.SmartValue = selectedValue;
			}
		}

		#endregion

		#region private methods

		private void FillImages()
		{
			values = new V[0];
			names = new string[0];
			if (Selection.activeGameObject == null)
				return;
			values = Selection.activeGameObject.GetComponentsInChildren<V>();
			if (values.Length > 0 && selectedValue == null && ValueEntry.SmartValue == null)
			{
				ValueEntry.SmartValue = values[0];
				selectedValue = ValueEntry.SmartValue;
			}
			names = new string[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				names[i] = values[i].name;
				if (values[i] == selectedValue)
					selectedIndex = i;
			}
		}

		#endregion
	}
}
