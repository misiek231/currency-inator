using CurrencyInator.Core.Data.Models;
using CurrencyInator.Core.Data.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Moq;
using OneOf.Types;

namespace CurrencyInator.Core.Tests.Integration;

public class CurrenciesRepositoryTests : IClassFixture<DbFixture>
{
    private readonly Mock<ILogger<CurrenciesRepository>> loggerMock = new();
    private readonly Mock<IOptions<DbSettings>> dbSettingsMock = new();
    private readonly CurrenciesRepository sut;
    private readonly string dbConnectionString;

    public CurrenciesRepositoryTests(DbFixture dbFixture)
    {
        dbConnectionString = dbFixture.DbConnectionString;
        sut = new CurrenciesRepository(dbFixture.Context, loggerMock.Object);
    }

    [Fact]
    public async Task Create_ShouldCreateDocumentInDb_WhenTheresNotSuchCurrencyAndDate()
    {
        // arrange
        var model = new CurrencyRate()
        {
            Currency = "PLN",
            Date = new DateOnly(2024, 12, 12),
            BuyRate = 1.0M,
            SellRate = 1.0M,
        };

        // act
        var result = await sut.Create(model, CancellationToken.None);

        // assert
        var collection = GetCollection();
        Assert.NotNull(collection);

        var items = await collection.AsQueryable().Where(p => p.Currency == model.Currency && p.Date == model.Date && p.BuyRate == model.BuyRate && p.SellRate == model.SellRate).ToListAsync();

        Assert.Single(items);
        Assert.IsType<Success<CurrencyRate>>(result.Value);

        // cleanup
        await collection.DeleteOneAsync(p => p.Currency == "PLN");
    }

    [Fact]
    public async Task Create_ShouldNotCreateDocumentInDb_WhenSuchCurrencyAndDateAlreadyExists()
    {
        // arrange
        var model = new CurrencyRate()
        {
            Currency = "PLN",
            Date = new DateOnly(2024, 12, 12),
            BuyRate = 1.0M,
            SellRate = 1.0M,
        };

        await GetCollection().InsertOneAsync(model);

        // act
        var result = await sut.Create(model, CancellationToken.None);

        // assert
        var collection = GetCollection();
        Assert.NotNull(collection);

        var items = await collection.AsQueryable().Where(p => p.Currency == model.Currency && p.Date == model.Date && p.BuyRate == model.BuyRate && p.SellRate == model.SellRate).ToListAsync();
        Assert.Single(items);

        Assert.IsType<CurrencyRate>(result.Value);
        Assert.Equivalent(model, result.Value);

        // cleanup
        await collection.DeleteOneAsync(p => p.Currency == "PLN");
    }

    private IMongoCollection<CurrencyRate> GetCollection()
    {
        var _client = new MongoClient(dbConnectionString);
        var db = _client.GetDatabase("Test");
        return db.GetCollection<CurrencyRate>(nameof(CurrencyRate));
    }
}
