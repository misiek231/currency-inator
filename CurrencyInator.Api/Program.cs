using CurrencyInator.Core.Data;
using CurrencyInator.Core.Data.Models;
using CurrencyInator.Core.Data.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DbSettings>(builder.Configuration.GetSection("Database"));

builder.Services.AddSingleton<IMongoDbContext, MongoDbContext>();
builder.Services.AddSingleton<CurrenciesRepository>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/ping", () => "pong");

app.Run();
