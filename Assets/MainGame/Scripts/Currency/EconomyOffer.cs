using System;
using com.otm.currency;
using UnityEngine;

// A shop item that buys itself: pay a cost (or nothing, e.g. after a rewarded ad), get a reward.
[Serializable]
public class EconomyOffer
{
    [Tooltip("A unique id in lower_snake_case, e.g. offer_booster_shuffle_x3.")]
    public string offerId;
    [Tooltip("What the player pays with. Empty = a free offer.")]
    public Currency costCurrency;
    public int costAmount;
    public Currency rewardCurrency;
    public int rewardAmount = 1;
    [Tooltip("If above 0, the reward is 'infinite use' for this many seconds instead of an amount.")]
    public int rewardInfiniteSeconds;

    public bool Buy()
    {
        if (rewardCurrency == null) return false;

        bool free = costCurrency == null || costAmount <= 0;
        if (!free && !CurrencyOps.Spend(costCurrency, costAmount)) return false;

        if (rewardInfiniteSeconds > 0) CurrencyOps.AddInfinite(rewardCurrency, rewardInfiniteSeconds);
        else CurrencyOps.Add(rewardCurrency, rewardAmount);
        return true;
    }
}