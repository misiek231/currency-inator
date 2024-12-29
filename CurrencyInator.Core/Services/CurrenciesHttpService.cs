using CurrencyInator.Core.Models;
using OneOf;
using OneOf.Types;
using System.Net.Http.Json;

namespace CurrencyInator.Core.Services;

public interface ICurrenciesHttpService
{
    Task<OneOf<decimal, NotFound>> GetCurrencyRate(string currency, DateOnly date, CancellationToken ct);
}

public class CurrenciesHttpService : ICurrenciesHttpService
{
    private readonly IHttpClientFactory httpClientFactory;
    public const string NBP_CLIENT_NAME = "NPB";

    public CurrenciesHttpService(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<OneOf<decimal, NotFound>> GetCurrencyRate(string currency, DateOnly date, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient(NBP_CLIENT_NAME);

        var response = await client.GetAsync($"{currency}/{date:yyyy-MM-dd}", ct);

        if (!response.IsSuccessStatusCode) return new NotFound();

        var result = await response.Content.ReadFromJsonAsync<NbpCurrencyRate>(ct);

        if (result == null || result.Rates.Count != 1) return new NotFound();

        return result.Rates[0].Mid;
    }
}
