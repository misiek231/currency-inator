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
            Rate = 1.0M
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
        Assert.Equal(data.Rate, result.AsT0.Rate);
    }

    [Fact]
    public async Task GetOrCreateCurrencyRate_ShouldReturnNbpCurrencyFromApiAndCreateDbModel_WhenNotExistsInDb()
    {
        // arrange
        var data = new CurrencyRate
        {
            Currency = "USD",
            Date = new DateOnly(2024, 12, 12),
            Rate = 1.0M
        };

        currenciesHttpServiceMock.Setup(p => p.GetCurrencyRate("USD", new DateOnly(2024, 12, 12), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1.0M)
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
        Assert.Equal(data.Rate, result.AsT0.Rate);
    }

    [Fact]
    public async Task GetOrCreateCurrencyRate_ShouldReturnNotFound_WhenApiReturnsNotFound()
    {
        // arrange
        var data = new CurrencyRate
        {
            Currency = "USD",
            Date = new DateOnly(2024, 12, 12),
            Rate = 1.0M
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
