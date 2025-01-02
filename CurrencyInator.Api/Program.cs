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

        app.MapGet("/{currency}/{date}", async (CurrenciesService s, string currency, string date, CancellationToken ct) =>
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
            {
                return Results.BadRequest(new { error = "Invalid date format. Provide date in format: yyyy-MM-dd" });
            }

            return (await s.GetOrCreateCurrencyRate(currency, parsedDate, ct)).Match(p => Results.Ok(p), p => Results.NotFound(new { error = "Requested currency does not have rate for given date" }));
        });

        app.Run();
    }
}