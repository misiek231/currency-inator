using CurrencyInator.Core.Data;
using CurrencyInator.Core.Data.Models;
using CurrencyInator.Core.Data.Repository;
using CurrencyInator.Core.Tests.Mocks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using OneOf.Types;

namespace CurrencyInator.Core.Tests;

public class CurrenciesRepositoryTests
{

    private readonly CurrenciesRepository sut;
    private readonly Mock<IMongoDbContext> dbContextMock = new();
    private readonly Mock<IMongoCollection<CurrencyRate>> collectionMock = new();
    private readonly Mock<ILogger<CurrenciesRepository>> loggerMock = new();

    private static readonly CurrencyRate[] data =
    [
        new ()
        {
            Id = "Test",
            Currency = "USD",
            Date = new DateOnly(2024, 12, 28),
            Rate = 1
        }
    ];

    public CurrenciesRepositoryTests()
    {
        dbContextMock.Setup(p => p.WritableCollection<CurrencyRate>()).Returns(collectionMock.Object);
        dbContextMock.Setup(p => p.ReadableCollection<CurrencyRate>()).Returns(new MongoQueryMock<CurrencyRate>(data));

        sut = new CurrenciesRepository(dbContextMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Find_ShouldReturnCurrencyModel_WhenDocumentExists()
    {
        // act
        var result = await sut.Find(data[0].Currency, data[0].Date, CancellationToken.None);

        // assert
        Assert.IsType<CurrencyRate>(result.Value);
        Assert.Equal(data[0], result.Value);
    }

    public static TheoryData<string, DateOnly> TestData => new()
    {
        { "PLN", data[0].Date },
        { data[0].Currency, new DateOnly(2020, 12, 12) },
        { "PLN", new DateOnly(2020, 12, 12) }
    };

    [Theory]
    [MemberData(nameof(TestData))]
    public async Task Find_ShouldReturnNotFound_WhenDocumentNotExists(string currency, DateOnly date)
    {
        // act
        var result = await sut.Find(currency, date, CancellationToken.None);

        // assert
        Assert.IsType<NotFound>(result.Value);
    }

    [Fact]
    public async Task Create_ShouldCreateDocAndReturnSuccess_WhenNotExists()
    {
        // arrange
        var doc = new CurrencyRate()
        {
            Id = "Test",
            Currency = "EUR",
            Date = new DateOnly(2024, 12, 28),
            Rate = 1
        };

        collectionMock.Setup(p => p.InsertOneAsync(It.IsAny<CurrencyRate>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>())).Verifiable();

        // act
        var result = await sut.Create(doc, CancellationToken.None);

        // assert
        collectionMock.Verify();
        Assert.IsType<Success<CurrencyRate>>(result.Value);
    }

    [Fact]
    public async Task Create_ShouldNotCreateDocAndReturnExistingCurrencyRate_WhenExists()
    {
        // arrange
        collectionMock.Setup(p => p.InsertOneAsync(It.IsAny<CurrencyRate>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>())).Verifiable(Times.Never);

        // act
        var result = await sut.Create(data[0], CancellationToken.None);

        // assert
        collectionMock.Verify();
        Assert.IsType<CurrencyRate>(result.Value);
    }

    [Fact]
    public async Task Create_ShouldBeThreadSafe()
    {
        // arrange
        var list = new List<CurrencyRate>();

        dbContextMock.Setup(p => p.ReadableCollection<CurrencyRate>()).Returns(new MongoQueryMock<CurrencyRate>(list));

        collectionMock
            .Setup(p => p.InsertOneAsync(It.IsAny<CurrencyRate>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
            .Callback<CurrencyRate, InsertOneOptions, CancellationToken>((model, op, ct) => list.Add(model))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        // act
        var result = await Task.WhenAll(
            sut.Create(data[0], CancellationToken.None),
            sut.Create(data[0], CancellationToken.None),
            sut.Create(data[0], CancellationToken.None)
        );

        // assert
        collectionMock.Verify();

        Assert.Collection(result, p => Assert.Equal(data[0], Assert.IsType<Success<CurrencyRate>>(p.Value).Value),
            p => Assert.Equal(data[0], Assert.IsType<CurrencyRate>(p.Value)),
            p => Assert.Equal(data[0], Assert.IsType<CurrencyRate>(p.Value))
        );
    }
}