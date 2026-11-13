using Sirenix.OdinInspector;
using UnityEngine;

namespace com.otm.manager
{
    public class ManagerInstanceLoader : SerializedMonoBehaviour
    {
        #region public variables

        public LoadingType loadingType;
        public ManagerExtraLoading preExtraLoading;
        public ManagerExtraLoading postExtraLoading;

        [HideInInspector] public bool loaded;

        #endregion

        #region unity callback

        public void Awake()
        {
            if (loadingType == LoadingType.auto)
            {
                loaded = true;
            }
        }

        public void OnLoadingStart()
        {
            if (loadingType == LoadingType.Manual)
                loaded = false;
        }

        public void OnLoadingDone()
        {
            if (loadingType == LoadingType.Manual)
                loaded = true;
        }

        public bool IsLoaded()
        {
            if (postExtraLoading == null)
                return loaded;
            return loaded && postExtraLoading.isLoaded;
        }

        public bool CanLoadingStart()
        {
            if (preExtraLoading == null)
                return true;
            return preExtraLoading.isLoaded;
        }

        public void StartPreExtraLoading()
        {
            if (preExtraLoading == null)
                return;
            preExtraLoading.StartChecking();
        }

        #endregion

        #region  coroutine
        #endregion
    }

    public abstract class ManagerExtraLoading
    {
        public bool isLoaded;

        public virtual bool IsLoaded()
        {
            return isLoaded;
        }

        public abstract void StartChecking();

    }
}