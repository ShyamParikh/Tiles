using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEngine.Rendering;

namespace com.otm.editor
{
	public class SortingOrderWindow : OdinEditorWindow
	{
		#region public variables

		public bool incrementalSortingOrder = false;
		[ShowIf("$incrementalSortingOrder", false)]
		public int sortingOrderGap = 1;
		public int startSortingOrder = 0;
		public Transform parent;
		public static SortingOrderWindow instance;

		#endregion

		public static void ShowWindow(Transform parent)
		{
			if (parent == null)
				return;
			SortingOrderWindow window = GetWindow<SortingOrderWindow>("Sprite Rename");
			window.parent = parent;
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
				DoSortingOrder();
			}
		}

		[Button]
		private void DoSortingOrder()
		{
			int sortingOrder = startSortingOrder;
			for (int i = 0; i < parent.childCount; i++)
			{
				Transform child = parent.GetChild(i);
				SortingGroup group = child.GetComponent<SortingGroup>();
				if (group != null)
				{
					group.sortingOrder = sortingOrder;
					if (incrementalSortingOrder)
						sortingOrder += (sortingOrderGap <= 0) ? 1 : sortingOrderGap;
					continue;
				}
				SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
				if (spriteRenderer != null)
				{
					spriteRenderer.sortingOrder = sortingOrder;
					if (incrementalSortingOrder)
						sortingOrder += (sortingOrderGap <= 0) ? 1 : sortingOrderGap;
				}
			}
			instance.Close();
			instance = null;
		}
	}
}
