using System;
using com.otm.currency;
using UnityEngine;

/// <summary>Pay a cost (or nothing, e.g. after a rewarded ad) and receive a reward.</summary>
[Serializable]
public class EconomyOffer
{
    [Tooltip("Unique id in lower_snake_case, e.g. offer_booster_shuffle_x3.")]
    public string offerId = "";
    [Tooltip("What the player pays with. Empty = free offer.")]
    public Currency costCurrency;
    public int costAmount = 0;
    public Currency rewardCurrency;
    public int rewardAmount = 1;
    [Tooltip("If above 0 the reward is 'infinite use' for this many seconds instead of an amount.")]
    public int rewardInfiniteSeconds = 0;
}