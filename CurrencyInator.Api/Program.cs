using CurrencyInator.Core.Services;

namespace CurrencyInator.Api;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddProblemDetails();

        builder.Services.AddAppOptions(builder.Configuration);
        builder.Services.AddNpbApiClient();
        builder.Services.AddServices();

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseExceptionHandler();

        app.MapGet("/ping", () => "pong");

        app.MapGet("/{currency}/{date}", async (CurrenciesService s, string currency, DateOnly date, CancellationToken ct) =>
        {
            return (await s.GetOrCreateCurrencyRate(currency, date, ct)).Match(p => Results.Ok(p), p => Results.NotFound("Requested currency does not have rate for given date"));
        });

        app.Run();
    }
}