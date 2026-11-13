using com.otm.manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.utility
{
	public class CheatHandler : MonoBehaviour
	{
		#region private variables

		[SerializeField] private GameObject cheatObject;

		#endregion

		#region unity callback

		private void OnEnable()
		{
			cheatObject.SetActive(ReporterManager.Instance.IsCheatEnable);
			ReporterManager.OnCheatValueChange.RegisterEvent(OnCheatValueChange);
		}

		private void OnDisable()
		{
			ReporterManager.OnCheatValueChange.DeRegisterEvent(OnCheatValueChange);
		}

		#endregion

		#region private methods

		private void OnCheatValueChange(bool active)
		{
			cheatObject.SetActive(active);
		}

		#endregion
	}
}
