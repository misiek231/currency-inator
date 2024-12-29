using CurrencyInator.Core.Data.Models;
using CurrencyInator.Core.Data.Repository;
using CurrencyInator.Core.Models;
using CurrencyInator.Core.Services;
using Moq;
using OneOf.Types;

namespace CurrencyInator.Core.Tests;

public class CurrenciesServiceTests
{
    private readonly CurrenciesService sut;
    private readonly Mock<ICurrenciesHttpService> currenciesHttpServiceMock = new();
    private readonly Mock<ICurrenciesRepository> currenciesRepositoryMock = new();

    public CurrenciesServiceTests()
    {
        sut = new(currenciesHttpServiceMock.Object, currenciesRepositoryMock.Object);
    }

    [Fact]
    public async Task GetOrCreateCurrencyRate_ShouldReturnNbpCurrencyFromDbAndNotCallApi_WhenExists()
    {
        // arrange
        var data = new CurrencyRate
        {
            Currency = "USD",
            Date = new DateOnly(2024, 12, 12),
            SellRate = 1.0M,
            BuyRate = 1.0M
        };

        currenciesHttpServiceMock.Setup(p => p.GetCurrencyRate(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).Verifiable(Times.Never);
        currenciesRepositoryMock.Setup(p => p.Find("USD", new DateOnly(2024, 12, 12), It.IsAny<CancellationToken>())).ReturnsAsync(data);
        currenciesRepositoryMock.Setup(p => p.Create(It.IsAny<CurrencyRate>(), It.IsAny<CancellationToken>())).Verifiable(Times.Never);

        // act
        var result = await sut.GetOrCreateCurrencyRate("USD", new DateOnly(2024, 12, 12), CancellationToken.None);

        // assert
        currenciesHttpServiceMock.Verify();
        currenciesRepositoryMock.Verify();
        Assert.IsType<CurrencyRateResult>(result.Value);
        Assert.Equal(data.SellRate, result.AsT0.SellRate);
        Assert.Equal(data.BuyRate, result.AsT0.BuyRate);
    }

    [Fact]
    public async Task GetOrCreateCurrencyRate_ShouldReturnNbpCurrencyFromApiAndCreateDbModel_WhenNotExistsInDb()
    {
        // arrange
        var data = new CurrencyRate
        {
            Currency = "USD",
            Date = new DateOnly(2024, 12, 12),
            BuyRate = 1.0M,
            SellRate = 1.0M
        };

        currenciesHttpServiceMock.Setup(p => p.GetCurrencyRate("USD", new DateOnly(2024, 12, 12), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NbpRate() { Ask = 1.0M, Bid = 1.0M, EffectiveDate = "", No = "" })
            .Verifiable(Times.AtLeastOnce);

        currenciesRepositoryMock.Setup(p => p.Find("USD", new DateOnly(2024, 12, 12), It.IsAny<CancellationToken>())).ReturnsAsync(new NotFound());

        currenciesRepositoryMock.Setup(p => p.Create(It.IsAny<CurrencyRate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Success<CurrencyRate>(data))
            .Verifiable(Times.AtLeastOnce);

        // act
        var result = await sut.GetOrCreateCurrencyRate("USD", new DateOnly(2024, 12, 12), CancellationToken.None);

        // assert
        currenciesHttpServiceMock.Verify();
        currenciesRepositoryMock.Verify();
        Assert.IsType<CurrencyRateResult>(result.Value);
        Assert.Equal(data.SellRate, result.AsT0.SellRate);
        Assert.Equal(data.BuyRate, result.AsT0.BuyRate);
    }

    [Fact]
    public async Task GetOrCreateCurrencyRate_ShouldReturnNotFound_WhenApiReturnsNotFound()
    {
        // arrange
        var data = new CurrencyRate
        {
            Currency = "USD",
            Date = new DateOnly(2024, 12, 12),
            BuyRate = 1.0M,
            SellRate = 1.0M
        };

        currenciesHttpServiceMock.Setup(p => p.GetCurrencyRate("USD", new DateOnly(2024, 12, 12), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound())
            .Verifiable(Times.AtLeastOnce);

        currenciesRepositoryMock.Setup(p => p.Find("USD", new DateOnly(2024, 12, 12), It.IsAny<CancellationToken>())).ReturnsAsync(new NotFound());

        currenciesRepositoryMock.Setup(p => p.Create(It.IsAny<CurrencyRate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Success<CurrencyRate>(data))
            .Verifiable(Times.Never);

        // act
        var result = await sut.GetOrCreateCurrencyRate("USD", new DateOnly(2024, 12, 12), CancellationToken.None);

        // assert
        currenciesHttpServiceMock.Verify();
        currenciesRepositoryMock.Verify();
        Assert.IsType<NotFound>(result.Value);
    }
}
