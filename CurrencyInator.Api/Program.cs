using CurrencyInator.Api;
using CurrencyInator.Api.Settings;
using CurrencyInator.Core.Data;
using CurrencyInator.Core.Data.Models;
using CurrencyInator.Core.Data.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<DbSettings>()
    .Bind(builder.Configuration.GetSection("Database"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<NbpApiSettings>()
  .Bind(builder.Configuration.GetSection("NbpApi"))
  .ValidateDataAnnotations()
  .ValidateOnStart();

builder.Services.AddSingleton<IMongoDbContext, MongoDbContext>();
builder.Services.AddSingleton<CurrenciesRepository>();
builder.Services.RegisterNpbApiClient();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/ping", () => "pong");

app.Run();
