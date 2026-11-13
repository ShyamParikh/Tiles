using com.otm.utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.otm.editor
{
	public class ButtonMenu
	{
		[MenuItem("GameObject/OTM/CreateButton")]
		public static void CreateButton()
		{
			GameObject gameObject = Selection.activeObject as GameObject;
			if (gameObject == null)
				return;
			Undo.RegisterChildrenOrderUndo(gameObject, "Button Create");
			CustomButton customButtonPrefb = AssetDatabase.LoadAssetAtPath<CustomButton>("Assets/OTM Utility/Prefab/CustomeButton.prefab");
			CustomButton btn = Object.Instantiate(customButtonPrefb) as CustomButton;
			btn.transform.SetParent(gameObject.transform, false);
			btn.name = "Button";
			Selection.activeObject = btn;
		}

	}
}
