using CurrencyInator.Core.Models;
using CurrencyInator.Core.Services;
using Moq;
using OneOf.Types;

namespace CurrencyInator.Core.Tests.Integration;

public class CurrenciesHttpServiceTests
{
    private readonly CurrenciesHttpService sut;
    private readonly Mock<IHttpClientFactory> clientFactoryMock = new();
    private readonly string baseAddress = "https://api.nbp.pl/api/exchangerates/rates/C/";

    public CurrenciesHttpServiceTests()
    {
        clientFactoryMock.Setup(p => p.CreateClient(It.IsAny<string>()))
            .Throws(new Exception("Invalid client name was provided"));

        clientFactoryMock.Setup(p => p.CreateClient(CurrenciesHttpService.NBP_CLIENT_NAME))
            .Returns(new HttpClient { BaseAddress = new Uri(baseAddress) });

        sut = new CurrenciesHttpService(clientFactoryMock.Object);
    }

    [Fact]
    public async Task GetCurrencyRate_ShouldReturnNbpCurrency_WhenDataIsValid()
    {
        // act
        var result = await sut.GetCurrencyRate("USD", new DateOnly(2024, 12, 12), CancellationToken.None);

        // assert
        Assert.IsType<NbpRate>(result.Value);
        Assert.Equal(4.02270M, result.AsT0.Bid);
        Assert.Equal(4.1039M, result.AsT0.Ask);
    }

    [Fact]
    public async Task GetCurrencyRate_ShouldReturnNotFound_WhenDataIsNotValid()
    {
        // act
        var result = await sut.GetCurrencyRate("Invalid", new DateOnly(2024, 12, 13), CancellationToken.None);

        // assert
        Assert.IsType<NotFound>(result.Value);
    }
}
