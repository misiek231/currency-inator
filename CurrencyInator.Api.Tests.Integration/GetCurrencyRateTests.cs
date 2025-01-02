using CurrencyInator.Core.Models;
using CurrencyInator.Core.Tests.Integration;
using System.Net;
using System.Net.Http.Json;

namespace CurrencyInator.Api.Tests.Integration;

public class GetCurrencyRateTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public GetCurrencyRateTests(ApiFixture apiFixture)
    {
        _client = apiFixture.CreateClient();
    }

    [Fact]
    public async Task GetCurrencyRateEndpoint_ShouldReturnOk_WhenDataIsValid()
    {
        // arrange
        var expected = new CurrencyRateResult
        {
            Currency = "USD",
            Date = new DateOnly(2024, 12, 12),
            BuyRate = 4.0227M,
            SellRate = 4.1039M
        };

        // act
        var result = await _client.GetAsync("USD/2024-12-12");

        // assert
        var model = await result.Content.ReadFromJsonAsync<CurrencyRateResult>();

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equivalent(expected, model);
    }

    [Fact]
    public async Task GetCurrencyRateEndpoint_ShouldReturnNotFound_WhenDataIsNotValid()
    {
        // act
        var result = await _client.GetAsync("Invalid/2024-12-12");

        // assert
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task GetCurrencyRateEndpoint_ShouldReturnValidationError_WhenDateHasInvalidFormat()
    {
        // act
        var result = await _client.GetAsync("USD/2023-54-35");

        // assert
        var stringContent = await result.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Invalid date format", stringContent);
    }
}
