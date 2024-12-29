using CurrencyInator.Core.Data.Models;
using CurrencyInator.Core.Data.Repository;
using CurrencyInator.Core.Models;
using OneOf;
using OneOf.Types;

namespace CurrencyInator.Core.Services;

public class CurrenciesService
{
    private readonly ICurrenciesHttpService currenciesHttpService;
    private readonly ICurrenciesRepository currenciesRepository;

    public CurrenciesService(ICurrenciesHttpService currenciesHttpService, ICurrenciesRepository currenciesRepository)
    {
        this.currenciesHttpService = currenciesHttpService;
        this.currenciesRepository = currenciesRepository;
    }

    public async Task<OneOf<CurrencyRateResult, NotFound>> GetOrCreateCurrencyRate(string currency, DateOnly date, CancellationToken ct)
    {
        var result = await currenciesRepository.Find(currency, date, ct);

        if (result.Value is CurrencyRate r) return CurrencyRateResult.From(r);

        var newRate = await currenciesHttpService.GetCurrencyRate(currency, date, ct);

        if (newRate.Value is NotFound f) return f;

        var model = new CurrencyRate()
        {
            Currency = currency,
            Date = date,
            BuyRate = newRate.AsT0.Bid,
            SellRate = newRate.AsT0.Ask
        };

        await currenciesRepository.Create(model, ct);

        return CurrencyRateResult.From(model);
    }
}
