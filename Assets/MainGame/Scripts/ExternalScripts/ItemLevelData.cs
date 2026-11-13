/*using UnityEngine;

[System.Serializable]
public struct ItemLevelData
{
    public Item itemPrefab;
    public bool isGoal;

    [NaughtyAttributes.ValidateInput("ValidateAmount", "Amount Spawned must be a multiple of 3")]
    [NaughtyAttributes.AllowNesting]
    [Range(0, 50)]
    public int amountSpawned;

    private bool ValidateAmount(int amount) => amount % 3 == 0;
}*/