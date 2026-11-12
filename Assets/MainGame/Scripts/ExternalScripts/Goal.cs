/*using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class GoalManager : BaseManager
{
    [Header("Elements")]
    [SerializeField] private GoalCardUI goalCardPrefab;
    [SerializeField] private Transform goalCardParent;

    [Header("Data")]
    private ItemLevelData[] goals;
    private List<GoalCardUI> goalCards = new List<GoalCardUI>();

    protected override void SubscribeToEvents()
    {
        LevelManager.levelSpawned += OnLevelSpawned;
        ItemSpotsManager.itemPickedUp += OnItemPickedUp;
    }
    protected override void UnsubscribeFromEvents()
    {
        LevelManager.levelSpawned -= OnLevelSpawned;
        ItemSpotsManager.itemPickedUp -= OnItemPickedUp;
    }

    private void OnLevelSpawned(Level level)
    {
        goals = level.GetGoals();

        GenerateGoalCards();
    }

    private void GenerateGoalCards()
    {
        for (int i = 0; i < goals.Length; i++)
        {
            GenerateGoalCard(goals[i]);
        }
    }

    private void GenerateGoalCard(ItemLevelData goal)
    {
        GoalCardUI goalCard = Instantiate(goalCardPrefab, goalCardParent);
        goalCard.Configure(goal.amountSpawned, goal.itemPrefab.Icon);

        goalCards.Add(goalCard);
    }

    private void OnItemPickedUp(Item item)
    {
        for (int i = 0; i < goals.Length; i++)
        {
            if (!goals[i].itemPrefab.ItemId.Equals(item.ItemId)) continue;

            goals[i].amountSpawned--;

            if (goals[i].amountSpawned <= 0)
            {
                GoalsCompleted(i);
            }
            else
            {
                goalCards[i].SetAmount(goals[i].amountSpawned);
            }
        }
    }

    private void GoalsCompleted(int goalIndex)
    {
        goalCards[goalIndex].Complete();

        CheckForLevelCompleted();
    }

    private void CheckForLevelCompleted()
    {
        for (int i = 0; i < goals.Length; i++)
        {
            if (goals[i].amountSpawned > 0) return;
        }
        GameManager.instance.SetGameState(EGameState.LevelComplete);
    }
}*/