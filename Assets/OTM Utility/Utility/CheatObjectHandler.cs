using com.otm.manager;
using UnityEngine;

namespace com.otm.utility
{
	public class CheatObjectHandler : MonoBehaviour
	{
		#region private veriables

		[SerializeField] private GameObject[] objects = new GameObject[0];

		#endregion

		#region unity callback

		private void OnEnable()
		{
			for (int i = 0; i < objects.Length; i++)
			{
				objects[i].SetActive(ReporterManager.Instance.IsCheatEnable);
			}
		}



		#endregion
	}
}
