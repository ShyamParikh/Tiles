/*
 * EconomyOffer
 * ------------
 * A shop item that buys itself: pay a cost (a currency and an amount, or nothing for a free offer such as one
 * given after a rewarded ad) and receive a reward (an amount of a currency, or "infinite use" for some seconds).
 * Give it a unique offer id in lower_snake_case, e.g. offer_booster_shuffle_x3. TileEconomyManager keeps the
 * list of offers and Purchase(id) calls Buy on the matching one. All the money handling goes through CurrencyOps.
 */
using System;
using com.otm.currency;
using UnityEngine;

[Serializable]
public class EconomyOffer
{
    #region Public Variables

    [Tooltip("A unique id in lower_snake_case, e.g. offer_booster_shuffle_x3.")]
    public string offerId;
    [Tooltip("What the player pays with. Empty = a free offer.")]
    public Currency costCurrency;
    public int costAmount;
    public Currency rewardCurrency;
    public int rewardAmount = 1;
    [Tooltip("If above 0, the reward is 'infinite use' for this many seconds instead of an amount.")]
    public int rewardInfiniteSeconds;

    #endregion

    #region Methods

    // Takes the cost (if any) and gives the reward. Returns false if the player can't pay.
    public bool Buy()
    {
        if (rewardCurrency == null) return false;

        bool free = costCurrency == null || costAmount <= 0;
        if (!free && !CurrencyOps.Spend(costCurrency, costAmount)) return false;

        if (rewardInfiniteSeconds > 0) CurrencyOps.AddInfinite(rewardCurrency, rewardInfiniteSeconds);
        else CurrencyOps.Add(rewardCurrency, rewardAmount);
        return true;
    }

    #endregion
}