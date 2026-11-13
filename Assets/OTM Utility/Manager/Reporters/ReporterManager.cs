using com.otm.utility;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.manager
{
	public class ReporterManager : Manager<ReporterManager>
	{
		#region private veriables

		[SerializeField] private bool showReporter;
		[SerializeField] private List<string> passwords = new List<string>();
		#endregion

		#region public static variables

		public static ActionSystem<bool> OnCheatValueChange = new ActionSystem<bool>();
		public static ActionSystem<bool> OnReporterValueChange = new ActionSystem<bool>();

		#endregion

		#region propertices
		public bool IsCheatEnable
		{
			get { return PlayerPrefsHelper.GetBool("IsCheatEnable", false); }
			set { PlayerPrefsHelper.SetBool("IsCheatEnable", value); }
		}

		#endregion


		#region overrided methods

		public override void Awake()
		{
			showReporter = false;
			base.Awake();
		}

		public override void OnDestroy()
		{
			OnCheatValueChange.ClearEvents();
			OnReporterValueChange.ClearEvents();
			base.OnDestroy();
		}

		#endregion


		#region public methods

		public void ShowCheat(string password)
		{
			string hash = SystemUtility.GetHash(password);
			Debug.LogError(password + " : " + hash);
			if (!passwords.Contains(hash))
				return;
			IsCheatEnable = true;
			OnCheatValueChange.Invoke(IsCheatEnable);
		}

		public void ShowCheat()
		{
			IsCheatEnable = true;
			OnCheatValueChange.Invoke(IsCheatEnable);
		}

		public void ShowLogs()
		{
			showReporter = true;
			Debug.unityLogger.filterLogType = LogType.Log;
			OnReporterValueChange.Invoke(showReporter);
		}

		#endregion


	}

}