using UnityEditor;
using UnityEngine;
using com.otm.currency;

[CustomEditor(typeof(TileEconomyManager))]
public class TileEconomyManagerEditor : Editor
{
    private int customAmount = 5;
    private int infiniteSeconds = 60;

    public override bool RequiresConstantRepaint()
    {
        return Application.isPlaying;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TileEconomyManager manager = (TileEconomyManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Runtime Debugger", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to see and change currency values.", MessageType.Info);
            return;
        }

        manager.EnsureInitialized();

        customAmount = EditorGUILayout.IntField("Custom Amount", customAmount);
        infiniteSeconds = EditorGUILayout.IntField("Infinite Seconds", infiniteSeconds);

        EditorGUILayout.Space(4);

        foreach (Currency currency in manager.Currencies)
        {
            if (currency == null) continue;

            string valueText = currency.IsInfiniteOnGoing
                ? "INF " + currency.InfiniteTime.ToString(@"hh\:mm\:ss")
                : currency.Value.ToString();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(currency.name, GUILayout.MinWidth(130f));
            EditorGUILayout.LabelField(valueText, GUILayout.Width(90f));

            if (GUILayout.Button("-1", GUILayout.Width(32f))) manager.TrySpend(currency, 1);
            if (GUILayout.Button("+1", GUILayout.Width(32f))) manager.Grant(currency, 1);
            if (GUILayout.Button("+N", GUILayout.Width(36f))) manager.Grant(currency, customAmount);
            if (GUILayout.Button("-N", GUILayout.Width(36f))) manager.TrySpend(currency, customAmount);
            if (GUILayout.Button("INF", GUILayout.Width(36f))) manager.GrantInfinite(currency, infiniteSeconds);
            if (GUILayout.Button("Reset", GUILayout.Width(48f))) manager.ResetCurrency(currency);
            EditorGUILayout.EndHorizontal();
        }

        if (manager.Offers.Count > 0)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Offers", EditorStyles.boldLabel);

            foreach (EconomyOffer offer in manager.Offers)
            {
                if (offer == null) continue;

                string cost = offer.costCurrency != null && offer.costAmount > 0
                    ? $"{offer.costAmount} {offer.costCurrency.name}"
                    : "free";
                string reward = offer.rewardCurrency != null ? offer.rewardCurrency.name : "?";

                if (GUILayout.Button($"{offer.offerId}   ({cost} -> {reward})"))
                    manager.TryPurchase(offer);
            }
        }

        EditorGUILayout.Space(6);
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("Reset ALL Currencies", GUILayout.Height(26f)))
            manager.ResetAll();
        GUI.backgroundColor = Color.white;
    }
}

/*
using UnityEngine;
using Sirenix.OdinInspector;
using com.otm.currency;
using com.otm.utility;

namespace com.otm.core
{
    public class EconomyDebugger : MonoBehaviour
    {
        [Header("Economy Assets")]
        public Currency coinEngine;
        public RefillCurrency lifeEngine;

        [Header("Booster Assets")]
        public Currency hammerEngine;
        public Currency magnetEngine;
        public Currency popEngine;
        public Currency wandEngine;

        private void Awake()
        {
            PlayerPrefsHelper.CanSaveData = true;
        }

        [Title("Hard Drive Diagnostics")]
        [Button(ButtonSizes.Large), GUIColor(1f, 1f, 0f)]
        public void PrintRawHardDriveData()
        {
            PlayerPrefsHelper.CanSaveData = true;
            Debug.Log("<color=#FFFF00>--- RAW HARD DRIVE SCAN ---</color>");

            LogCurrencyData(coinEngine, "Coins");
            LogCurrencyData(lifeEngine, "Lives");
            LogCurrencyData(hammerEngine, "Hammers");
            LogCurrencyData(magnetEngine, "Magnets");
            LogCurrencyData(popEngine, "Pops");
            LogCurrencyData(wandEngine, "Wands");

            Debug.Log("<color=#FFFF00>---------------------------</color>");
        }

        private void LogCurrencyData(Currency engine, string name)
        {
            if (engine != null)
            {
                string key = string.IsNullOrEmpty(engine.InfiniteEndDateKey) ? "" : engine.InfiniteEndDateKey.Replace("InfiniteEndDate", "");
                int savedVal = PlayerPrefsHelper.GetInt(key, -999);
                Debug.Log($"Raw {name} saved in OS: {savedVal}");
            }
        }

        [Title("Coin Controls")]
        [Button(ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
        public void AddCoins(int amount = 500)
        {
            PlayerPrefsHelper.CanSaveData = true;
            if (coinEngine != null)
            {
                coinEngine.Add(amount);
                PlayerPrefsHelper.SaveData();
            }
        }

        [Button(ButtonSizes.Medium), GUIColor(1f, 0.4f, 0.4f)]
        public void RemoveCoins(int amount = 100)
        {
            PlayerPrefsHelper.CanSaveData = true;
            if (coinEngine != null)
            {
                coinEngine.Add(-amount);
                PlayerPrefsHelper.SaveData();
            }
        }

        [Title("Life Controls")]
        [Button(ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
        public void AddLife()
        {
            PlayerPrefsHelper.CanSaveData = true;
            if (lifeEngine != null)
            {
                lifeEngine.Add(1);
                PlayerPrefsHelper.SaveData();
            }
        }

        [Button(ButtonSizes.Medium), GUIColor(1f, 0.4f, 0.4f)]
        public void RemoveLife()
        {
            PlayerPrefsHelper.CanSaveData = true;
            if (lifeEngine == null) return;
            if (lifeEngine.IsInfiniteOnGoing) return;
            if (lifeEngine.Value <= 0) return;

            lifeEngine.Add(-1);
            PlayerPrefsHelper.SaveData();
        }

        [Title("Booster Controls")]
        [Button(ButtonSizes.Medium), GUIColor(0.2f, 1f, 0.4f)]
        public void AddBooster(BoosterType booster, int amount = 1)
        {
            PlayerPrefsHelper.CanSaveData = true;
            Currency targetEngine = GetBoosterEngine(booster);
            if (targetEngine != null)
            {
                targetEngine.Add(amount);
                PlayerPrefsHelper.SaveData();
                Debug.Log($"<color=#32CD32>[ECONOMY] Added {amount} to {booster}!</color>");
            }
        }

        [Button(ButtonSizes.Medium), GUIColor(1f, 0.4f, 0.4f)]
        public void RemoveBooster(BoosterType booster, int amount = 1)
        {
            PlayerPrefsHelper.CanSaveData = true;
            Currency targetEngine = GetBoosterEngine(booster);
            if (targetEngine != null)
            {
                targetEngine.Add(-amount);
                PlayerPrefsHelper.SaveData();
                Debug.Log($"<color=#FF4500>[ECONOMY] Removed {amount} from {booster}!</color>");
            }
        }

        private Currency GetBoosterEngine(BoosterType type)
        {
            switch (type)
            {
                case BoosterType.Hammer: return hammerEngine;
                case BoosterType.Magnet: return magnetEngine;
                case BoosterType.Pop: return popEngine;
                case BoosterType.Wand: return wandEngine;
                default: return null;
            }
        }

        [Title("Infinite Mode Controls")]
        [Button(ButtonSizes.Large), GUIColor(0.8f, 0.2f, 1f)]
        public void GrantInfiniteLivesForDuration(int infiniteTimer = 3600)
        {
            PlayerPrefsHelper.CanSaveData = true;
            if (lifeEngine != null)
            {
                lifeEngine.AddInfinite(infiniteTimer);

                int missingLives = lifeEngine.DefaultValue - lifeEngine.Value;
                if (missingLives > 0)
                {
                    lifeEngine.Add(missingLives);
                }

                PlayerPrefsHelper.SaveData();
                lifeEngine.OnCurrencyChange?.Invoke(lifeEngine.Value);

                Debug.Log("<color=#DDA0DD>[ECONOMY] Granted " + infiniteTimer / 60 + " minutes of Infinite Lives & Reset Lives to Full!</color>");
            }
        }

        [Title("Special Controls")]
        [Button(ButtonSizes.Large), GUIColor(1f, 0f, 0f)]
        public void ResetAllData()
        {
            PlayerPrefsHelper.CanSaveData = true;

            if (lifeEngine != null)
            {
                PlayerPrefs.DeleteKey(lifeEngine.InfiniteEndDateKey);
                lifeEngine.Delete();
            }
            if (coinEngine != null) coinEngine.Delete();
            if (hammerEngine != null) hammerEngine.Delete();
            if (magnetEngine != null) magnetEngine.Delete();
            if (popEngine != null) popEngine.Delete();
            if (wandEngine != null) wandEngine.Delete();

            PlayerPrefsHelper.DeleteAllkey();
            PlayerPrefsHelper.SaveData();
            PlayerPrefs.Save();

            if (coinEngine != null) { coinEngine.Init(); coinEngine.OnCurrencyChange?.Invoke(coinEngine.Value); }
            if (hammerEngine != null) { hammerEngine.Init(); hammerEngine.OnCurrencyChange?.Invoke(hammerEngine.Value); }
            if (magnetEngine != null) { magnetEngine.Init(); magnetEngine.OnCurrencyChange?.Invoke(magnetEngine.Value); }
            if (popEngine != null) { popEngine.Init(); popEngine.OnCurrencyChange?.Invoke(popEngine.Value); }
            if (wandEngine != null) { wandEngine.Init(); wandEngine.OnCurrencyChange?.Invoke(wandEngine.Value); }

            if (lifeEngine != null)
            {
                lifeEngine.Init();
                lifeEngine.OnCurrencyChange?.Invoke(lifeEngine.Value);
                lifeEngine.OnInfiniteOver?.Invoke();
            }

            Debug.Log("<color=#FF0000>[ECONOMY] ALL SAVED DATA WIPED AND RUNTIME RESET!</color>");
        }
    }
}
*/