using CurrencyInator.Api.Settings;
using CurrencyInator.Core.Data;
using CurrencyInator.Core.Data.Models;
using CurrencyInator.Core.Data.Repository;
using CurrencyInator.Core.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace CurrencyInator.Api;

public static class StartupExtensions
{
    public static IServiceCollection AddNpbApiClient(this IServiceCollection services)
    {
        services.AddHttpClient(CurrenciesHttpService.NBP_CLIENT_NAME, (services, client) =>
        {
            var clientConfig = services.GetRequiredService<IOptions<NbpApiSettings>>().Value;

            client.BaseAddress = new Uri(clientConfig.Url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        return services;
    }

    public static IServiceCollection AddAppOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DbSettings>()
            .Bind(configuration.GetSection("Database"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<NbpApiSettings>()
          .Bind(configuration.GetSection("NbpApi"))
          .ValidateDataAnnotations()
          .ValidateOnStart();

        return services;
    }
    
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IMongoDbContext, MongoDbContext>();
        services.AddSingleton<ICurrenciesRepository, CurrenciesRepository>();
        services.AddScoped<ICurrenciesHttpService, CurrenciesHttpService>();
        services.AddScoped<CurrenciesService>();
        
        return services;
    }
}
