using CurrencyInator.Core.Services;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

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
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.MapOpenApi();
        app.MapScalarApiReference(p =>
        {
            p.WithTitle("Currency-Inator API")
            .WithTheme(ScalarTheme.BluePlanet)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

            p.Servers = Array.Empty<ScalarServer>();
        });

        app.UseHttpsRedirection();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler();
        }

        app.MapGet("/", () => Results.Redirect("scalar/v1")).ExcludeFromDescription();

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