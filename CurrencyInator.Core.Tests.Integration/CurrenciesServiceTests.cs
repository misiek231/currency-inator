using CurrencyInator.Core.Data.Models;
using CurrencyInator.Core.Data.Repository;
using CurrencyInator.Core.Models;
using CurrencyInator.Core.Services;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Moq;

namespace CurrencyInator.Core.Tests.Integration;

public class CurrenciesServiceTests : IClassFixture<DbFixture>
{
    private readonly CurrenciesService sut;
    private readonly Mock<ILogger<CurrenciesRepository>> loggerMock = new();
    private readonly Mock<IHttpClientFactory> clientFactoryMock = new();
    private readonly string baseAddress = "https://api.nbp.pl/api/exchangerates/rates/C/";
    private readonly string dbConnectionString;

    public CurrenciesServiceTests(DbFixture dbFixture)
    {
        clientFactoryMock.Setup(p => p.CreateClient(It.IsAny<string>()))
            .Throws(new Exception("Invalid client name was provided"));

        clientFactoryMock.Setup(p => p.CreateClient(CurrenciesHttpService.NBP_CLIENT_NAME))
            .Returns(new HttpClient { BaseAddress = new Uri(baseAddress) });

        var currenciesHttpService = new CurrenciesHttpService(clientFactoryMock.Object);

        dbConnectionString = dbFixture.DbConnectionString;
        var currenciesRepository = new CurrenciesRepository(dbFixture.Context, loggerMock.Object);

        sut = new(currenciesHttpService, currenciesRepository);
    }

    [Fact]
    public async Task GetOrCreateCurrencyRate_ShouldReturnNbpCurrency_WhenExistsInDb()
    {
        // arrange
        var data = new CurrencyRate
        {
            Currency = "USD",
            Date = new DateOnly(2024, 12, 12),
            BuyRate = 1.0M,
            SellRate = 1.0M
        };

        await GetCollection().InsertOneAsync(data);

        // act
        var result = await sut.GetOrCreateCurrencyRate("USD", new DateOnly(2024, 12, 12), CancellationToken.None);

        // assert
        Assert.IsType<CurrencyRateResult>(result.Value);
        Assert.Equal(data.BuyRate, result.AsT0.BuyRate);
        Assert.Equal(data.SellRate, result.AsT0.SellRate);

        // cleanup
        await GetCollection().DeleteOneAsync(p => p.Currency == "USD");
    }

    [Fact]
    public async Task GetOrCreateCurrencyRate_ShouldReturnNbpCurrencyAndCreateDocument_WhenNotExistsInDb()
    {
        // arrange
        var data = new CurrencyRate
        {
            Currency = "USD",
            Date = new DateOnly(2024, 12, 12),
            BuyRate = 1.0M,
            SellRate = 1.0M
        };

        // act
        var result = await sut.GetOrCreateCurrencyRate("USD", new DateOnly(2024, 12, 12), CancellationToken.None);

        // assert
        Assert.IsType<CurrencyRateResult>(result.Value);
        Assert.Equal(4.02270M, result.AsT0.BuyRate);
        Assert.Equal(4.1039M, result.AsT0.SellRate);

        var dbItems = await GetCollection().AsQueryable().Where(p => p.Currency == "USD" && p.Date == new DateOnly(2024, 12, 12)).ToListAsync();

        Assert.Single(dbItems);
        Assert.Equal(4.02270M, dbItems[0].BuyRate);
        Assert.Equal(4.1039M, dbItems[0].SellRate);

        // cleanup
        await GetCollection().DeleteOneAsync(p => p.Currency == "USD");
    }

    private IMongoCollection<CurrencyRate> GetCollection()
    {
        var _client = new MongoClient(dbConnectionString);
        var db = _client.GetDatabase("Test");
        return db.GetCollection<CurrencyRate>(nameof(CurrencyRate));
    }
}
