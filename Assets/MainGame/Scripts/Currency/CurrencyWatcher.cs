/*
 * CurrencyWatcher
 * ---------------
 * Calls one refresh method whenever a currency's amount changes or its infinite timer ticks or ends.
 * Labels should never poll, so they create a watcher and get told instead. Boosters and the coin shower use it.
 * Create it AFTER the currencies are initialised (Currency.Init wipes listeners) and call Dispose when the
 * object is destroyed, so no listener is left behind on the currency asset.
 */
using System;
using com.otm.currency;

public class CurrencyWatcher : IDisposable
{
    #region Private Variables

    private readonly Currency currency;
    private readonly Action refresh;

    #endregion

    #region Setup And Cleanup

    // Starts listening to the currency.
    public CurrencyWatcher(Currency currency, Action refresh)
    {
        this.currency = currency;
        this.refresh = refresh;

        currency.OnCurrencyChange.RegisterEvent(OnAmount);
        currency.OnInfiniteUpdate.RegisterEvent(OnTimer);
        currency.OnInfiniteOver.RegisterEvent(refresh);
    }

    // Stops listening.
    public void Dispose()
    {
        currency.OnCurrencyChange.DeRegisterEvent(OnAmount);
        currency.OnInfiniteUpdate.DeRegisterEvent(OnTimer);
        currency.OnInfiniteOver.DeRegisterEvent(refresh);
    }

    #endregion

    #region Currency Events

    // The amount changed.
    private void OnAmount(int value) { refresh(); }

    // The infinite timer ticked.
    private void OnTimer(TimeSpan left) { refresh(); }

    #endregion
}