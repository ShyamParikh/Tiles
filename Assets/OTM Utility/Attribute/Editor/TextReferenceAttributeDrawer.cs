//using com.otm.utility;
//using Sirenix.OdinInspector.Editor;
//using Sirenix.Utilities.Editor;
//using TMPro;
//using UnityEditor;
//using UnityEngine;

//namespace com.otm.editor
//{
//	[DrawerPriority(10000000, 0, 0)]
//	public class TextReferenceAttributeDrawer : OdinAttributeDrawer<TextReferenceAttribute, TMP_Text>
//	{
//		#region private veriables

//		private TMP_Text[] values;
//		private string[] names;
//		private TMP_Text selectedValue;
//		private int selectedIndex;

//		#endregion

//		#region overrided methods

//		protected override void Initialize()
//		{
//			this.SkipWhenDrawing = true;
//			base.Initialize();
//			selectedValue = ValueEntry.SmartValue;
//			FillImages();
//		}
//		protected override void DrawPropertyLayout(GUIContent label)
//		{
//			ValueEntry.SmartValue = (TMP_Text)SirenixEditorFields.UnityObjectField(label, ValueEntry.SmartValue, typeof(TMP_Text), true);
//			if (values == null || values.Length <= 0)
//			{
//				SirenixEditorGUI.ErrorMessageBox("NO Component Found : " + (typeof(TMP_Text)));
//				return;
//			}
//			if (names.Length > 0)
//			{
//				selectedIndex = SirenixEditorFields.Dropdown(selectedIndex, names);
//				Debug.Log(selectedIndex);
//				selectedValue = values[selectedIndex];
//				if (selectedValue != ValueEntry.SmartValue)
//					ValueEntry.SmartValue = selectedValue;
//			}
//			else
//			{
//				Debug.Log(names.Length);
//			}
//		}

//		#endregion

//		#region private methods

//		private void FillImages()
//		{
//			values = new TMP_Text[0];
//			names = new string[0];
//			if (Selection.activeGameObject == null)
//				return;
//			values = Selection.activeGameObject.GetComponentsInChildren<TMP_Text>();
//			if (values.Length > 0 && selectedValue == null && ValueEntry.SmartValue == null)
//			{
//				ValueEntry.SmartValue = values[0];
//				selectedValue = ValueEntry.SmartValue;
//			}
//			names = new string[values.Length];
//			for (int i = 0; i < values.Length; i++)
//			{
//				names[i] = values[i].name;
//				if (values[i] == selectedValue)
//					selectedIndex = i;
//			}
//		}

//		#endregion
//	}
//}
