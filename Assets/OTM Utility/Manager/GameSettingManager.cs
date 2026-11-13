using com.otm.utility;
using UnityEngine;

namespace com.otm.manager
{
    public class GameSettingManager : Manager<GameSettingManager>
    {
        #region  private variables

        [SerializeField] private bool logEnable;
        [SerializeField] private int targetFPS;

        #endregion

        #region  overrided methods

        public override void Awake()
        {
            PlayerPrefsHelper.CanSaveData = true;
            base.Awake();
#if UNITY_EDITOR
            logEnable = true;
#endif
            if (!logEnable)
                Debug.unityLogger.filterLogType = LogType.Error;
            else
                Debug.unityLogger.filterLogType = LogType.Log;
#if !UNITY_EDITOR
			Application.targetFrameRate = targetFPS;
#endif
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        #endregion
    }
}
