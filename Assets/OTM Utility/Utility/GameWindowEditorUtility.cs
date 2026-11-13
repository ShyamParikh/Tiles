#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace com.otm.editor
{
	public class GameWindowEditorUtility
	{
		static readonly Type GameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
		static readonly PropertyInfo ShowToolbarProperty = GameViewType.GetProperty("showToolbar", BindingFlags.Instance | BindingFlags.NonPublic);
		static readonly object False = false; // Only box once. This is a matter of principle.

		static EditorWindow instance;

		[MenuItem("Window/General/Game (Fullscreen) %#&2", priority = 2)]
		public static void MakeFullScreen()
		{
			if (GameViewType == null)
			{
				Debug.LogError("GameView type not found.");
				return;
			}

			if (ShowToolbarProperty == null)
			{
				Debug.LogWarning("GameView.showToolbar property not found.");
			}

			if (instance != null)
			{
				instance.Close();
				instance = null;
			}
			else
			{
				instance = (EditorWindow)ScriptableObject.CreateInstance(GameViewType);

				ShowToolbarProperty?.SetValue(instance, False);

				var desktopResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
				var fullscreenRect = new Rect(Vector2.zero, desktopResolution);
				instance.ShowPopup();
				instance.position = fullscreenRect;
				instance.Focus();
			}
		}
	}
}
#endif