/*using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using static UnityEditor.Progress;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class ItemSpawner : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private List<ItemLevelData> itemData;

    [Header("Settings")]
    [SerializeField] private BoxCollider spawnZone;
    [SerializeField] private int seed;

    public ItemLevelData[] GetGoals()
    {
        List<ItemLevelData> goals = new List<ItemLevelData>();
        foreach (ItemLevelData data in itemData)
        {
            if (data.isGoal)
            {
                goals.Add(data);
            }
        }
        return goals.ToArray();
    }

#if UNITY_EDITOR
    [Button]
    private void SpawnItems()
    {
        while (transform.childCount > 0)
        {
            Transform t = transform.GetChild(0);
            t.SetParent(null);
            DestroyImmediate(t.gameObject);
        }

        Random.InitState(seed);

        for (int i = 0; i < itemData.Count; i++)
        {
            ItemLevelData data = itemData[i];

            for (int j = 0; j < data.amountSpawned; j++)
            {
                Vector3 spawnPos = GetSpawnPosition();

                Item itemInstance = PrefabUtility.InstantiatePrefab(data.itemPrefab, transform) as Item;
                itemInstance.transform.position = spawnPos;
                itemInstance.transform.rotation = Quaternion.Euler(Random.onUnitSphere * 360);
            }
        }
    }

    private Vector3 GetSpawnPosition()
    {
        float x = Random.Range(-(spawnZone.size.x / 3), spawnZone.size.x / 3);
        float y = Random.Range(-(spawnZone.size.y / 3), spawnZone.size.y / 3);
        float z = Random.Range(-(spawnZone.size.z / 3), spawnZone.size.z / 3);

        Vector3 localPos = spawnZone.center + new Vector3(x, y, z);
        Vector3 spawnPos = transform.TransformPoint(localPos);

        return spawnPos;
    }
#endif
}*/