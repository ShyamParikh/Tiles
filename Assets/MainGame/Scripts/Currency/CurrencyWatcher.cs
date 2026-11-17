using System;
using com.otm.currency;

// Calls one refresh method whenever a currency's amount changes or its infinite timer ticks.
// Create it after the currencies are initialised (Init wipes listeners) and Dispose it when done.
public class CurrencyWatcher : IDisposable
{
    private readonly Currency currency;
    private readonly Action refresh;

    public CurrencyWatcher(Currency currency, Action refresh)
    {
        this.currency = currency;
        this.refresh = refresh;

        currency.OnCurrencyChange.RegisterEvent(OnAmount);
        currency.OnInfiniteUpdate.RegisterEvent(OnTimer);
        currency.OnInfiniteOver.RegisterEvent(refresh);
    }

    private void OnAmount(int value) { refresh(); }
    private void OnTimer(TimeSpan left) { refresh(); }

    public void Dispose()
    {
        currency.OnCurrencyChange.DeRegisterEvent(OnAmount);
        currency.OnInfiniteUpdate.DeRegisterEvent(OnTimer);
        currency.OnInfiniteOver.DeRegisterEvent(refresh);
    }
}