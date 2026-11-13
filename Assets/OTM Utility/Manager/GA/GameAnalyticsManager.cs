#if OTM_GA
using com.otm.manager;
using UnityEngine;

namespace com.otm.games
{
	public class GameAnalyticsManager : Manager<GameAnalyticsManager>
	{
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        #endregion

        #region Propertices

        #endregion


        #region IGameAnalyticsListerner_Implementation
        #endregion


        #region UNITY_CALLBACKS

		private void Start()
		{
			GameAnalytics.Initialize();
		}

        #endregion

        #region PUBLIC_FUNCTIONS

		public void LogDesignEvent(string name)
		{
			GameAnalytics.NewDesignEvent(name);
			if (AnalyticsManager.Instance != null && AnalyticsManager.Instance.LogEvent)
				Debug.Log("GA : " + name);
		}

        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        #endregion
	}
}
#endif