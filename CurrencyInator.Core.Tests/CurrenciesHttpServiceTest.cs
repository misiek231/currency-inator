using CurrencyInator.Core.Models;
using CurrencyInator.Core.Services;
using Moq;
using OneOf.Types;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http.Json;

namespace CurrencyInator.Core.Tests;

public class CurrenciesHttpServiceTests
{
    private readonly CurrenciesHttpService sut;
    private readonly Mock<IHttpClientFactory> clientFactoryMock = new();
    private readonly MockHttpMessageHandler httpMessageHandlerMock = new();
    private readonly string baseAddress = "https://api.nbp.pl/api/exchangerates/rates/A/";

    public CurrenciesHttpServiceTests()
    {
        clientFactoryMock.Setup(p => p.CreateClient(It.IsAny<string>()))
            .Throws(new Exception("Invalid client name was provided"));

        clientFactoryMock.Setup(p => p.CreateClient(CurrenciesHttpService.NBP_CLIENT_NAME))
            .Returns(new HttpClient(httpMessageHandlerMock) { BaseAddress = new Uri(baseAddress) });

        sut = new CurrenciesHttpService(clientFactoryMock.Object);
    }

    [Fact]
    public async Task GetCurrencyRate_ShouldReturnNbpCurrency_WhenResponseIsSuccess()
    {
        // arrange
        var requestResult = new NbpCurrencyRate() { Currency = "USD", Code = "USD", Rates = [new() { Mid = 1.0M, EffectiveDate = "", No = "" }], Table = "A" };

        httpMessageHandlerMock.When(HttpMethod.Get, $"{baseAddress}USD/2024-12-12")
            .Respond(HttpStatusCode.OK, JsonContent.Create(requestResult));

        // act
        var result = await sut.GetCurrencyRate("USD", new DateOnly(2024, 12, 12), CancellationToken.None);

        // assert
        httpMessageHandlerMock.VerifyNoOutstandingRequest();
        Assert.IsType<decimal>(result.Value);
        Assert.Equal(1.0M, result.Value);
    }

    [Fact]
    public async Task GetCurrencyRate_ShouldReturnNotFound_WhenResponseIsNotSuccess()
    {
        // arrange
        httpMessageHandlerMock.When(HttpMethod.Get, $"{baseAddress}USD/2024-12-13")
            .Respond(HttpStatusCode.BadRequest);

        // act
        var result = await sut.GetCurrencyRate("USD", new DateOnly(2024, 12, 13), CancellationToken.None);

        // assert
        httpMessageHandlerMock.VerifyNoOutstandingRequest();
        Assert.IsType<NotFound>(result.Value);
    }
}
