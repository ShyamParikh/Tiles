using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace com.otm.editor
{
	public class RenameWindow : OdinEditorWindow
	{
		#region public variables

		public string mainName;
		public string prefix = "";
		public string suffix = "";
		public string separator = "-";
		public int startNumber;

		public Object[] objects;
		public string[] folders;
		public string[] extarNames;

		public static RenameWindow instance;

		#endregion

		public static void ShowWindow(string name, Object[] objects)
		{
			if (objects == null || objects.Length <= 0)
				return;
			RenameWindow window = GetWindow<RenameWindow>("Rename Window");
			window.mainName = name;
			window.objects = objects;
			window.extarNames = new string[objects.Length];
			instance = window;
		}

		public static void ShowWindow(string name, string[] folders)
		{
			if (folders == null || folders.Length <= 0)
				return;
			RenameWindow window = GetWindow<RenameWindow>("Rename Window");
			window.folders = folders;
			window.extarNames = new string[folders.Length];
			window.mainName = name;
			instance = window;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			instance = null;
		}

		protected override void OnGUI()
		{
			base.OnGUI();
			Event e = Event.current;
			if (e == null)
				return;
			if (e.keyCode == KeyCode.Escape)
			{
				instance.Close();
				instance = null;
			}
			if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
			{
				Rename();
			}

		}

		[Button]
		private void Rename()
		{
			if (objects == null)
				return;
			if (string.IsNullOrEmpty(mainName))
			{
				Debug.LogError("Main name Must empty");
				return;
			}

			for (int i = 0; i < objects.Length; i++)
			{
				string path = AssetDatabase.GetAssetPath(objects[i]);
				if (extarNames.Length > i && !string.IsNullOrEmpty(extarNames[i]))
					AssetDatabase.RenameAsset(path, prefix + mainName + suffix + separator + extarNames[i]);
				else
				{
					AssetDatabase.RenameAsset(path, prefix + mainName + suffix + separator + startNumber);
					startNumber++;
				}
			}

			for (int i = 0; i < folders.Length; i++)
			{
				if (extarNames.Length > i && !string.IsNullOrEmpty(extarNames[i]))
					AssetDatabase.RenameAsset(folders[i], prefix + mainName + suffix + separator + extarNames[i]);
				else
				{
					AssetDatabase.RenameAsset(folders[i], prefix + mainName + suffix + separator + i);
					startNumber++;
				}
			}
			AssetDatabase.Refresh();
			instance.Close();
			instance = null;
		}


	}
}
