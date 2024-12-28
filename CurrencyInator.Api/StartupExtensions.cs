using CurrencyInator.Api.Settings;
using CurrencyInator.Core.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace CurrencyInator.Api;

public static class StartupExtensions
{
    public static IServiceCollection RegisterNpbApiClient(this IServiceCollection services)
    {
        services.AddHttpClient(CurrenciesHttpService.NBP_CLIENT_NAME, (services, client) =>
        {
            var clientConfig = services.GetRequiredService<IOptions<NbpApiSettings>>().Value;

            client.BaseAddress = new Uri(clientConfig.Url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        return services;
    }
}
